using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class BasicSchemeHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? authHeader = await GetAuthenticationHeaderAsync(Request);

        if (authHeader != null && authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
        {
            string token = authHeader["Basic ".Length..].Trim();
            
            var credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var credentials = credentialsAsEncodedString.Split(':');
            
            if (await _userRepository.Authenticate(credentials[0], credentials[1]))
            {
                var identity = new GenericIdentity(credentials[0]);
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
        request.BodyReader.AdvanceTo(requestBodyInBytes.Buffer.Start, requestBodyInBytes.Buffer.End);

        string authTicketFromHeader = null;

        if (body?.Contains(@"http://schemas.xmlsoap.org/soap/envelope/") == true)
        {
            XNamespace ns = "http://schemas.xmlsoap.org/soap/envelope/";
            var soapEnvelope = XDocument.Parse(body);
            var headers = soapEnvelope.Descendants(ns + "Header").ToList();

            foreach (var header in headers)
            {
                var authorizationElement = header.Element("Authorization");
                if (!string.IsNullOrWhiteSpace(authorizationElement?.Value))
                {
                    authTicketFromHeader = authorizationElement.Value;
                    break;
                }
            }
        }

        return authTicketFromHeader;
    }
}