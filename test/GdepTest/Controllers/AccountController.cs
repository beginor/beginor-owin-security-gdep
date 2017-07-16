﻿using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security;
using GdepTest.Models;
using System.Security.Claims;

namespace GdepTest.Controllers {

    [RoutePrefix("rest/account")]
    public class AccountController : ApiController {

        IAuthenticationManager AuthManager => Request.GetOwinContext().Authentication;

        [HttpGet, Route("external-login")]
        public IHttpActionResult GetExternalLogins() {
            var externalLogins = AuthManager.GetExternalAuthenticationTypes();
            return Ok(externalLogins);
        }

        [HttpPost, Route("external-login")]
        public IHttpActionResult ExternalLogin([FromBody]ExternalLoginModel model) {
            var properties = new AuthenticationProperties();
            var redirectUri = RequestContext.Url.Route("external-login-callback", null);
            properties.RedirectUri = redirectUri;
            AuthManager.Challenge(properties, model.Provider);
            return StatusCode(HttpStatusCode.Unauthorized);
        }

        [HttpGet, Route("external-login-callback", Name = "external-login-callback")]
        public IHttpActionResult ExternalLoginCallback() {
            var info = AuthManager.GetExternalLoginInfo();
            if (info == null) {
                return BadRequest("External Login Error!");
            }
            var name = info.ExternalIdentity.Name;
            var isAuth = info.ExternalIdentity.IsAuthenticated;
            var msg = $"User: {name} , is authenticated: {isAuth}";
            return Ok(msg);
        }

        [HttpGet, Route("test")]
        public IHttpActionResult Test() {
            var identity = new ClaimsIdentity();
            var idClaim = new Claim(
                ClaimTypes.NameIdentifier, "test"
            );
            identity.AddClaim(idClaim);
            var nameClaim = new Claim(
                ClaimTypes.Name, "test"
            );
            identity.AddClaim(nameClaim);
            AuthManager.SignIn(identity);
            return Ok(this.User.Identity.Name);
        }

    }

}