﻿using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;

namespace Mvc.Client.Controllers {
    public class AuthenticationController : Controller {
        [HttpGet, Route("~/signin")]
        public ActionResult SignIn() {
            IOwinContext context = HttpContext.GetOwinContext();
            if (context == null) {
                throw new NotSupportedException("An OWIN context cannot be extracted from HttpContext");
            }

            var properties = new AuthenticationProperties {
                RedirectUri = "/"
            };

            // Instruct the OIDC client middleware to redirect the user agent to the identity provider.
            // Note: the authenticationType parameter must match the value configured in Startup.cs
            context.Authentication.Challenge(properties, OpenIdConnectAuthenticationDefaults.AuthenticationType);

            return new HttpStatusCodeResult(401);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post), Route("~/signout")]
        public ActionResult SignOut() {
            IOwinContext context = HttpContext.GetOwinContext();
            if (context == null) {
                throw new NotSupportedException("An OWIN context cannot be extracted from HttpContext");
            }

            // Instruct the cookies middleware to delete the local cookie created when the user agent
            // is redirected from the identity provider after a successful authorization flow.
            // Note: this call doesn't disconnect the user agent at the identity provider level (yet).
            context.Authentication.SignOut("ClientCookie");

            return Redirect("/");
        }
    }
}