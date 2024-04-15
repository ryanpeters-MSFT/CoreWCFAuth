using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using WcfService.Repositories;

public class BasicSchemeHandler(IAuthenticationRepository authenticationRepository, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? authHeader = await GetAuthenticationHeaderAsync(Request);

        if (authHeader != null && authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
        {
            string token = authHeader["Basic ".Length..].Trim();
            
            var credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var credentials = credentialsAsEncodedString.Split(':');

            var username = credentials[0];
            var password = credentials[1];
            
            if (authenticationRepository.Authenticate(username, password))
            {
                var identity = new GenericIdentity(username);
                var claimsPrincipal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

                return await Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }

        return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
    }

    private static async Task<string?> GetAuthenticationHeaderAsync(HttpRequest request)
    {
        var requestBodyInBytes = await request.BodyReader.ReadAsync();
        var body = Encoding.UTF8.GetString(requestBodyInBytes.Buffer.FirstSpan);
        var envelope = "http://schemas.xmlsoap.org/soap/envelope/";

        request.BodyReader.AdvanceTo(requestBodyInBytes.Buffer.Start, requestBodyInBytes.Buffer.End);

        if (body?.Contains(envelope) == true)
        {
            XNamespace ns = envelope;
            var soapEnvelope = XDocument.Parse(body);
            var headers = soapEnvelope.Descendants(ns + "Header").ToList();

            foreach (var header in headers)
            {
                var authorizationElement = header.Element("Authorization");
                
                if (!string.IsNullOrWhiteSpace(authorizationElement?.Value))
                {
                    return authorizationElement.Value;
                }
            }
        }

        return null;
    }
}