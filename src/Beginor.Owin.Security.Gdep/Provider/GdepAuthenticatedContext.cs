using System;
using Microsoft.Owin.Security.Provider;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Microsoft.Owin.Security;
using System.Globalization;

namespace Beginor.Owin.Security.Gdep.Provider {

    public class GdepAuthenticatedContext : BaseContext {

        public JObject User { get; private set; }

        public string AccessToken { get; set; }

        public TimeSpan? ExpiresIn { get; set; }

        public string TokenType { get; set; }

        public string RefreshToken { get; set; }

        public ClaimsIdentity Identity { get; set; }

        public AuthenticationProperties Properties { get; set; }

        public string Id { get; private set; }

        public string Username { get; private set; }

        public string Email { get; set; }

        public string DistinguishedName { get; set; }

        public GdepAuthenticatedContext(
            IOwinContext context,
            JObject user,
            string accessToken,
            string expires,
            string tokenType,
            string refreshToken
        ) : base(context) {
            User = user;
            AccessToken = accessToken;

            int expiresValue;
            if (Int32.TryParse(expires, NumberStyles.Integer, CultureInfo.InvariantCulture, out expiresValue)) {
                ExpiresIn = TimeSpan.FromSeconds(expiresValue);
            }
            TokenType = tokenType;
            RefreshToken = refreshToken;

            Id = TryGetValue(user, "uid");
            Username = TryGetValue(user, "sAMAccountName");
            Email = TryGetValue(user, "mail");
            DistinguishedName = TryGetValue(user, "DistinguishedName");
        }

        private static string TryGetValue(JObject user, string propertyName) {
            JToken value;
            return user.TryGetValue(propertyName, out value) ? value.ToString() : null;
        }

    }

}
