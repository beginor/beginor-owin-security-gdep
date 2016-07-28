using System;
using System.Threading.Tasks;

namespace Beginor.Owin.Security.Gdep.Provider {

    public interface IGdepAuthenticationProvider {

        Task Authenticated(GdepAuthenticatedContext context);

        Task ReturnEndpoint(GdepReturnEndpointContext context);

        void ApplyRedirect(GdepApplyRedirectContext context);

    }

}
