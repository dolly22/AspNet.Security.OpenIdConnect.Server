﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Mvc.Server.Models;
using Owin.Security.OpenIdConnect.Server;

namespace Mvc.Server.Providers {
    public class CustomOpenIdConnectServerProvider : OpenIdConnectServerProvider {
        public override async Task ValidateClientAuthentication(OpenIdConnectValidateClientAuthenticationContext context) {
            string clientId, clientSecret;

            // Retrieve the client credentials from the request body.
            // Note: you can also retrieve them from the Authorization
            // header (basic authentication) using TryGetBasicCredentials.
            context.TryGetFormCredentials(out clientId, out clientSecret);

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret)) {
                context.SetError(
                    error: "invalid_request",
                    errorDescription: "Missing credentials: ensure that your credentials " +
                                      "were correctly flowed in the request body");

                return;
            }

            using (var db = new ApplicationContext()) {
                // Retrieve the application details corresponding to the requested client_id.
                Application application = await (from entity in db.Applications
                                                 where entity.ApplicationID == clientId
                                                 select entity).SingleOrDefaultAsync(context.Request.CallCancelled);

                if (application == null) {
                    context.SetError(
                        error: "invalid_client",
                        errorDescription: "Application not found in the database: " +
                                          "ensure that your client_id is correct");
                    return;
                }

                if (!string.Equals(clientSecret, application.Secret, StringComparison.Ordinal)) {
                    context.SetError(
                        error: "invalid_client",
                        errorDescription: "Invalid credentials: ensure that you " +
                                          "specified a correct client_secret");

                    return;
                }

                context.Validated(clientId);
            }
        }

        public override async Task ValidateClientRedirectUri(OpenIdConnectValidateClientRedirectUriContext context) {
            using (var db = new ApplicationContext()) {
                // Retrieve the application details corresponding to the requested client_id.
                Application application = await (from entity in db.Applications
                                                 where entity.ApplicationID == context.ClientId
                                                 select entity).SingleOrDefaultAsync(context.Request.CallCancelled);

                if (application == null) {
                    context.SetError(
                        error: "invalid_client",
                        errorDescription: "Application not found in the database: " +
                                          "ensure that your client_id is correct");
                    return;
                }

                if (!string.IsNullOrEmpty(context.RedirectUri)) {
                    if (!string.Equals(context.RedirectUri, application.RedirectUri, StringComparison.Ordinal)) {
                        context.SetError(
                            error: "invalid_client",
                            errorDescription: "Invalid redirect_uri");

                        return;
                    }
                }

                context.Validated(application.RedirectUri);
            }
        }
    }
}