using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Security.Claims;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json.Linq;
using Beginor.Owin.Security.Gdep.Provider;

namespace Beginor.Owin.Security.Gdep {

    internal class GdepAuthenticationHandler : AuthenticationHandler<GdepAuthenticationOptions> {

        HttpClient httpClient;
        ILogger logger;

        public GdepAuthenticationHandler(HttpClient httpClient, ILogger logger) {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync() {
            AuthenticationProperties properties = null;

            try {
                string code = null;
                string state = null;

                var query = Request.Query;
                var values = query.GetValues("error");
                if (values != null && values.Count >= 1) {
                    logger.WriteVerbose("Remote server returned an error: " + Request.QueryString);
                }

                values = query.GetValues("code");
                if (values != null && values.Count == 1) {
                    code = values[0];
                }

                values = query.GetValues("state");
                if (values != null && values.Count == 1) {
                    state = values[0];
                }

                properties = Options.StateDataFormat.Unprotect(state);
                if (properties == null) {
                    return null;
                }

                // OAuth2 10.12 CSRF
                if (!ValidateCorrelationId(properties, logger)) {
                    return new AuthenticationTicket(null, properties);
                }

                if (code == null) {
                    // Null if the remote server returns an error.
                    return new AuthenticationTicket(null, properties);
                }

                string scheme = GetRedirectScheme();
                var requestPrefix = scheme + Uri.SchemeDelimiter + Request.Host;
                var redirectUri = requestPrefix + Request.PathBase + Options.CallbackPath;

                var tokenRequest = new Dictionary<string, string> {
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri,
                    ["client_id"] = Options.AppId,
                    ["client_secret"] = Options.AppSecret
                };
                var tokenResponse = await httpClient.PostAsync(
                    Options.TokenEndpoint,
                    new FormUrlEncodedContent(tokenRequest)
                );
                tokenResponse.EnsureSuccessStatusCode();

                string json = await tokenResponse.Content.ReadAsStringAsync();
                var form = JObject.Parse(json);

                var accessToken = form.Value<string>("access_token");
                var expires = form.Value<string>("expires_in");
                var tokenType = form.Value<string>("token_type");
                var refreshToken = form.Value<string>("refresh_token");

                string graphAddress = Options.UserInformationEndpoint + "?access_token=" + Uri.EscapeDataString(accessToken);
                if (Options.SendAppSecretProof) {
                    graphAddress += "&appsecret_proof=" + GenerateAppSecretProof(accessToken);
                }

                var graphRequest = new HttpRequestMessage(HttpMethod.Get, graphAddress);
                graphRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var graphResponse = await httpClient.SendAsync(graphRequest, Request.CallCancelled);
                graphResponse.EnsureSuccessStatusCode();
                json = await graphResponse.Content.ReadAsStringAsync();
                JObject user = JObject.Parse(json);

                var context = new GdepAuthenticatedContext(Context, user, accessToken, expires, tokenType, refreshToken);

                var identity = new ClaimsIdentity(
                    Options.AuthenticationType,
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType
                );

                if (!string.IsNullOrEmpty(context.Id)) {
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.Id, ClaimValueTypes.String, Options.AuthenticationType));
                }

                if (!string.IsNullOrEmpty(context.Username)) {
                    identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, context.Username, ClaimValueTypes.String, Options.AuthenticationType));
                }

                if (!string.IsNullOrEmpty(context.Email)) {
                    identity.AddClaim(new Claim(ClaimTypes.Email, context.Email, ClaimValueTypes.String, Options.AuthenticationType));
                }

                if (!string.IsNullOrEmpty(context.DistinguishedName)) {
                    identity.AddClaim(new Claim(ClaimTypes.X500DistinguishedName, context.DistinguishedName, ClaimValueTypes.String, Options.AuthenticationType));
                }

                context.Identity = identity;
                context.Properties = properties;

                await Options.Provider.Authenticated(context);

                return new AuthenticationTicket(context.Identity, context.Properties);
            }
            catch (Exception ex) {
                logger.WriteError("Authentication failed", ex);
                return new AuthenticationTicket(null, properties);
            }
        }

        private string GetRedirectScheme() {
            var scheme = Request.Scheme;
            if (Options.ForceHttpsRedirect) {
                scheme = Uri.UriSchemeHttps;
            }
            return scheme;
        }

        protected override Task ApplyResponseChallengeAsync() {
            if (Response.StatusCode != 401) {
                return Task.FromResult<object>(null);
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);
            if (challenge != null) {
                var scheme = GetRedirectScheme();
                var baseUri = scheme + Uri.SchemeDelimiter + Request.Host + Request.PathBase;
                var currentUri = baseUri + Request.Path + Request.QueryString;
                var redirectUri = baseUri + Options.CallbackPath;

                var properties = challenge.Properties;
                if (string.IsNullOrEmpty(properties.RedirectUri)) {
                    properties.RedirectUri = currentUri;
                }

                // OAuth2 10.12 CSRF
                GenerateCorrelationId(properties);

                var scope = string.Join(" ", Options.Scope);
                var state = Options.StateDataFormat.Protect(properties);

                var authorizationEndpoint = Options.AuthorizationEndpoint +
                        "?response_type=code" +
                        "&client_id=" + Uri.EscapeDataString(Options.AppId) +
                        "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                        "&scope=" + Uri.EscapeDataString(scope) +
                        "&state=" + Uri.EscapeDataString(state);

                var redirectContext = new GdepApplyRedirectContext(Context, Options, properties, authorizationEndpoint);
                Options.Provider.ApplyRedirect(redirectContext);
            }

            return Task.FromResult<object>(null);
        }

        public override async Task<bool> InvokeAsync() {
            return await InvokeReplyPathAsync();
        }

        private async Task<bool> InvokeReplyPathAsync() {
            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path) {
                // TODO: error responses
                var ticker = await AuthenticateAsync();
                if (ticker == null) {
                    logger.WriteWarning("Invalid return state, unable to redirect.");
                    Response.StatusCode = 500;
                    return true;
                }

                var context = new GdepReturnEndpointContext(Context, ticker);
                context.SignInAsAuthenticationType = Options.SignInAsAuthenticationType;
                context.RedirectUri = ticker.Properties.RedirectUri;

                await Options.Provider.ReturnEndpoint(context);

                if (context.SignInAsAuthenticationType != null && context.Identity != null) {
                    var grantIdentity = context.Identity;
                    if (!string.Equals(grantIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal)) {
                        grantIdentity = new ClaimsIdentity(grantIdentity.Claims, context.SignInAsAuthenticationType, grantIdentity.NameClaimType, grantIdentity.RoleClaimType);
                    }
                    Context.Authentication.SignIn(context.Properties, grantIdentity);
                }

                if (!context.IsRequestCompleted && context.RedirectUri != null) {
                    var redirectUri = context.RedirectUri;
                    if (context.Identity == null) {
                        redirectUri = WebUtilities.AddQueryString(redirectUri, "error", "access_denied");
                    }
                    Response.Redirect(redirectUri);
                    context.RequestCompleted();
                }

                return context.IsRequestCompleted;
            }
            return false;
        }

        private string GenerateAppSecretProof(string accessToken) {
            using (HMACSHA256 algorithm = new HMACSHA256(Encoding.ASCII.GetBytes(Options.AppSecret))) {
                byte[] hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
                var builder = new StringBuilder();
                for (var i = 0; i < hash.Length; i++) {
                    builder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
                }
                return builder.ToString();
            }
        }

    }

}
