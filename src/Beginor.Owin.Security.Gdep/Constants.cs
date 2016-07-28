using System;

namespace Beginor.Owin.Security.Gdep {

    internal static class Constants {

        public static readonly string DefaultAuthenticationType = "GDEP_OAuth";

        internal static readonly string AuthorizationEndpoint = "https://app.gdep.gov.cn/oauth2/authorize";
        internal static readonly string TokenEndpoint = "https://app.gdep.gov.cn/oauth2/token";
        internal static readonly string UserInformationEndpoint = "https://app.gdep.gov.cn/oauthuser/api/user";

    }

}

