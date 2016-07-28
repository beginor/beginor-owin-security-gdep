using Microsoft.Owin.Security.Provider;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Beginor.Owin.Security.Gdep.Provider {
    
    public class GdepApplyRedirectContext : BaseContext<GdepAuthenticationOptions> {

        public string RedirectUri { get; private set; }

        public AuthenticationProperties Properties { get; private set; }

        public GdepApplyRedirectContext(IOwinContext context, GdepAuthenticationOptions options, AuthenticationProperties properties, string redirectUri) : base(context, options) {
            RedirectUri = redirectUri;
            Properties = properties;
        }

    }
}
