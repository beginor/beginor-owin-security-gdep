using System;
using System.Configuration;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Beginor.Owin.Security.Aes;
using Beginor.Owin.Security.Gdep;
using Beginor.Owin.Security.Gdep.Provider;
using Beginor.Owin.StaticFile;
using Microsoft.Owin.Security.DataProtection;
using Owin;

namespace GdepTest {

    public class Startup {

        public void Configuration(IAppBuilder app) {
            ConfigStaticFile(app);
            ConfigOauth(app);
            ConfigWebApi(app);
        }

        private static void ConfigWebApi(IAppBuilder app) {
            var config = new HttpConfiguration();
            var xml = config.Formatters.XmlFormatter;
            config.Formatters.Remove(xml);
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "rest/{controller}/{id}"
            );
            app.UseWebApi(config);
        }

        private static void ConfigOauth(IAppBuilder app) {
            // config auth
            var provider = new AesDataProtectionProvider("/GdepTest");
            app.SetDataProtectionProvider(provider);
            //app.CreatePerOwinContext<IAuthenticationManager>((IdentityFactoryOptions<IAuthenticationManager> options, IOwinContext context) => {
            //    return null;
            //};);
            app.UseExternalSignInCookie();
            var appSettings = ConfigurationManager.AppSettings;
            // oauth
            var oauthOptions = new GdepAuthenticationOptions {
                AuthenticationType = appSettings["oauth-type"],
                Caption = appSettings["oauth-caption"],
                AppId = appSettings["oauth-id"],
                AppSecret = appSettings["oauth-secret"],
                Scope = appSettings["oauth-scope"].Split(','),
                AuthorizationEndpoint = appSettings["oauth-authorize"],
                TokenEndpoint = appSettings["oauth-token"],
                UserInformationEndpoint = appSettings["oauth-user"],
                CallbackUrl = appSettings["oauth-callback"]
            };
            // setup provider
            var authProvider = new GdepAuthenticationProvider();
            authProvider.OnAuthenticated = (context) => {
                var user = context.User;
                // Console.WriteLine(user.ToString());
                var status = user.Value<int>("status");
                // extract user info to identity when status is 200;
                if (status == 200) {
                    var data = user.GetValue("data");
                    var code = data.Value<string>("user_code");
                    var name = data.Value<string>("user_name");
                    // var email = (string)data["user"]["email"];
                    // var mobile = (string)data["user"]["mobi_tel"];
                    // var addr = (string)data["user"]["user_addr"];
                    // var type = (int)data["user"]["user_type"];
                    var identity = new ClaimsIdentity(
                        oauthOptions.AuthenticationType,
                        ClaimsIdentity.DefaultNameClaimType,
                        ClaimsIdentity.DefaultRoleClaimType
                    );
                    identity.AddClaim(
                        new Claim(
                            ClaimTypes.NameIdentifier,
                            code,
                            ClaimValueTypes.String,
                            oauthOptions.AuthenticationType
                        )
                    );
                    identity.AddClaim(
                        new Claim(
                            ClaimTypes.Name,
                            code,
                            ClaimValueTypes.String,
                            oauthOptions.AuthenticationType
                        )
                    );
                    //identity.AddClaim(
                    //    new Claim(
                    //        ClaimTypes.Surname,
                    //        name,
                    //        ClaimValueTypes.String,
                    //        oauthOptions.AuthenticationType
                    //    )
                    //);
                    //identity.AddClaim(
                    //    new Claim(
                    //        ClaimTypes.Email,
                    //        email,
                    //        ClaimValueTypes.String,
                    //        oauthOptions.AuthenticationType
                    //    )
                    //);
                    //identity.AddClaim(
                    //    new Claim(
                    //        ClaimTypes.MobilePhone,
                    //        mobile,
                    //        ClaimValueTypes.String,
                    //        oauthOptions.AuthenticationType
                    //    )
                    //);
                    //identity.AddClaim(
                    //    new Claim(
                    //        ClaimTypes.StreetAddress,
                    //        addr,
                    //        ClaimValueTypes.String,
                    //        oauthOptions.AuthenticationType
                    //    )
                    //);
                    context.Identity = identity;
                    
                }
                return Task.CompletedTask;
            };
            oauthOptions.Provider = authProvider;
            app.UseGdepAuthentication(oauthOptions);
        }

        private static void ConfigStaticFile(IAppBuilder app) {
            // config static file
            var staticFileOptions = new StaticFileMiddlewareOptions() {
                DefaultFile = "index.html",
                EnableETag = false,
                EnableHtml5LocationMode = false,
                RootDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "../../../wwwroot"
                )
            };
            app.UseStaticFile(staticFileOptions);
        }
    }

}
