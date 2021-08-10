using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace TusTest
{
    public class Helper
    {
        
    }

    public static class DownloadFileEndpoint
    {
        public static async Task HandleRoute(HttpContext context)
        {
            var config = context.RequestServices.GetRequiredService<DefaultTusConfiguration>();

            if (!(config.Store is ITusReadableStore store))
            {
                return;
            }

            var fileId = (string)context.Request.RouteValues["fileId"];
            var file = await store.GetFileAsync(fileId, context.RequestAborted);

            if (file == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"File with id {fileId} was not found.", context.RequestAborted);
                return;
            }

            var fileStream = await file.GetContentAsync(context.RequestAborted);
            var metadata = await file.GetMetadataAsync(context.RequestAborted);

            context.Response.ContentType = GetContentTypeOrDefault(metadata);
            context.Response.ContentLength = fileStream.Length;

            if (metadata.TryGetValue("name", out var nameMeta))
            {
                context.Response.Headers.Add("Content-Disposition",
                    new[] { $"attachment; filename=\"{nameMeta.GetString(Encoding.UTF8)}\"" });
            }

            using (fileStream)
            {
                await fileStream.CopyToAsync(context.Response.Body, 81920, context.RequestAborted);
            }
        }

        private static string GetContentTypeOrDefault(Dictionary<string, Metadata> metadata)
        {
            if (metadata.TryGetValue("contentType", out var contentType))
            {
                return contentType.GetString(Encoding.UTF8);
            }

            return "application/octet-stream";
        }
    }

    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        // Don't do this in production...
        private const string Username = "test";
        private const string Password = "test";

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.NoResult());

            bool isAuthenticated;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                isAuthenticated = Authenticate(credentials[0], credentials[1]);
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }

            if (!isAuthenticated)
                return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, Username),
                new Claim(ClaimTypes.Name, Username),
            };

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name)));
        }

        private bool Authenticate(string username, string password)
        {
            return username == Username && password == Password;
        }
    }


}
