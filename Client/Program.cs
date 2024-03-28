using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using WcfService;

Console.WriteLine("Waiting 2 seconds for service to become available...\n");
Thread.Sleep(2000);

var basicHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

var endpoint = new EndpointAddress("https://127.0.0.1:7276/service.svc");

var factory = new ChannelFactory<IServiceChannel>(basicHttpBinding, endpoint);

// ignore the self-signed TLS cert
factory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
{
    CertificateValidationMode = X509CertificateValidationMode.None,
    RevocationMode = X509RevocationMode.NoCheck
};

var channel = factory.CreateChannel();

#region Set JWT Bearer token

// JWT token (assume this came from an identity provider)
var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlJ5YW4iLCJwb3NpdGlvbiI6InRlYWNoZXIiLCJpYXQiOjE1MTYyMzkwMjJ9.i3eWEwxThXYTnahJpynfdkQR6dvjvgJBa-uXSJX5eNk";

var httpRequestProperty = new HttpRequestMessageProperty();

httpRequestProperty.Headers[HttpRequestHeader.Authorization] = $"Bearer {token}";

OperationContext.Current = new OperationContext(channel);
OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty; 

#endregion

var result = channel.GetData(3);

Console.WriteLine(result);