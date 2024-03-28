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

factory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
{
    CertificateValidationMode = X509CertificateValidationMode.None,
    RevocationMode = X509RevocationMode.NoCheck
};

var channel = factory.CreateChannel();

var httpRequestProperty = new HttpRequestMessageProperty();

// JWT token
var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwicG9zaXRpb24iOiJ0ZWFjaGVyIiwiaWF0IjoxNTE2MjM5MDIyfQ.YT4H1tnwk0HnLiBUxuLpSMbSYPQD2OdaX2cvnYDwujE";

httpRequestProperty.Headers[HttpRequestHeader.Authorization] = $"Bearer {token}";

OperationContext.Current = new OperationContext(channel);
OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

var result = channel.GetData(3);

Console.WriteLine(result);