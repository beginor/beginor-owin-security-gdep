using System;
using Beginor.Owin.Security.Gdep;

namespace Owin {

    public static class GdepAuthenticationExtensions {

        public static IAppBuilder UseGdepAuthentication(this IAppBuilder app, GdepAuthenticationOptions options) {
            if (app == null) {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null) {
                throw new ArgumentNullException(nameof(options));
            }
            app.Use<GdepAuthenticationMiddleware>(app, options);
            return app;
        }

        public static IAppBuilder UseGdepAuthentication(this IAppBuilder app, string appId, string appSecret) {
            return UseGdepAuthentication(app, new GdepAuthenticationOptions { AppId = appId, AppSecret = appSecret });
        }

    }

}
