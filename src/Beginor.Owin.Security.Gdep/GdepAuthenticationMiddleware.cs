using System;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Logging;
using System.Net.Http;
using Beginor.Owin.Security.Gdep.Provider;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security;

namespace Beginor.Owin.Security.Gdep {

    public class GdepAuthenticationMiddleware : AuthenticationMiddleware<GdepAuthenticationOptions> {

        private ILogger logger;
        private HttpClient httpClient;

        public GdepAuthenticationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            GdepAuthenticationOptions options
        ) : base(next, options) {
            if (string.IsNullOrEmpty(Options.AppId)) {
                throw new ArgumentException("AppId must be provided.");
            }
            if (string.IsNullOrEmpty(Options.AppSecret)) {
                throw new ArgumentException("AppSecret must be provided.");
            }

            logger = app.CreateLogger<GdepAuthenticationMiddleware>();

            if (Options.Provider == null) {
                Options.Provider = new GdepAuthenticationProvider();
            }

            if (Options.StateDataFormat == null) {
                var dataProtector = app.CreateDataProtector(
                    typeof(GdepAuthenticationMiddleware).FullName,
                    Options.AuthenticationType,
                    "v1"
                );
                Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }

            if (string.IsNullOrEmpty(Options.SignInAsAuthenticationType)) {
                Options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
            }

            httpClient = new HttpClient(ResolveHttpMessageHandler(Options));
            httpClient.Timeout = Options.BackchannelTimeout;
            httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10M
        }

        protected override AuthenticationHandler<GdepAuthenticationOptions> CreateHandler() {
            return new GdepAuthenticationHandler(httpClient, logger);
        }

        private static HttpMessageHandler ResolveHttpMessageHandler(GdepAuthenticationOptions options) {
            HttpMessageHandler handler = options.BackchannelHttpHandler ?? new WebRequestHandler();

            if (options.BackchannelCertificateValidator != null) {
                var webRequestHandler = handler as WebRequestHandler;

                if (webRequestHandler == null) {
                    throw new InvalidOperationException("Validator handler is mismatch!");
                }

                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate;
            }

            return handler;
        }
    }

}
