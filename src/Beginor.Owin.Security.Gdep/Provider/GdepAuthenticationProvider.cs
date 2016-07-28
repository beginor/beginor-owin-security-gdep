using System;
using System.Threading.Tasks;

namespace Beginor.Owin.Security.Gdep.Provider {

    public class GdepAuthenticationProvider : IGdepAuthenticationProvider {

        public Func<GdepAuthenticatedContext, Task> OnAuthenticated { get; set; }

        public Func<GdepReturnEndpointContext, Task> OnReturnEndpoint { get; set; }

        public Action<GdepApplyRedirectContext> OnApplyRedirect { get; set; }

        public GdepAuthenticationProvider() {
            OnAuthenticated = context => Task.FromResult<object>(null);
            OnReturnEndpoint = context => Task.FromResult<object>(null);
            OnApplyRedirect = context => context.Response.Redirect(context.RedirectUri);
        }

        public virtual Task Authenticated(GdepAuthenticatedContext context) {
            return OnAuthenticated(context);
        }

        public virtual Task ReturnEndpoint(GdepReturnEndpointContext context) {
            return OnReturnEndpoint(context);
        }

        public virtual void ApplyRedirect(GdepApplyRedirectContext context) {
            OnApplyRedirect(context);
        }

    }

}
