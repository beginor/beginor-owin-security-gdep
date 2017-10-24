using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Beginor.Owin.Security.Gdep.Provider;


namespace Beginor.Owin.Security.Gdep {

    public class GdepAuthenticationOptions : AuthenticationOptions {

        public string AppId { get; set; }

        public string AppSecret { get; set; }

        public string AuthorizationEndpoint { get; set; }

        public string TokenEndpoint { get; set; }

        public string UserInformationEndpoint { get; set; }

        public string CallbackUrl { get; set; } = "/signin-gdep";

        public bool ForceHttpsRedirect { get; set; }

        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        public TimeSpan BackchannelTimeout { get; set; }

        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        public string Caption {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        public PathString CallbackPath => new PathString(CallbackUrl);

        public string SignInAsAuthenticationType { get; set; }

        public IGdepAuthenticationProvider Provider { get; set; }

        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        public IList<string> Scope { get; set; }

        public bool SendAppSecretProof { get; set; }

        public GdepAuthenticationOptions() : base(Constants.DefaultAuthenticationType) {
            Caption = Constants.DefaultAuthenticationType;
            AuthenticationMode = AuthenticationMode.Passive;
            Scope = new List<string>();
            BackchannelTimeout = TimeSpan.FromSeconds(60);
            SendAppSecretProof = true;
            AuthorizationEndpoint = Constants.AuthorizationEndpoint;
            TokenEndpoint = Constants.TokenEndpoint;
            UserInformationEndpoint = Constants.UserInformationEndpoint;
        }

    }

}
