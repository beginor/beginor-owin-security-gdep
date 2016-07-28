using System;
using Microsoft.Owin.Security.Provider;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Beginor.Owin.Security.Gdep.Provider {

    public class GdepReturnEndpointContext : ReturnEndpointContext {

        public GdepReturnEndpointContext(IOwinContext context, AuthenticationTicket ticker) : base(context, ticker) {
        }

    }

}
