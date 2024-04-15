using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;
using WcfService;

Console.WriteLine("Waiting 2 seconds for service to become available...\n");
Thread.Sleep(2000);

var basicHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

var endpoint = new EndpointAddress("https://127.0.0.1:7276/service.svc");
var endpointBuilder = new EndpointAddressBuilder(endpoint);

var username = "ryan";
var password = "123456";

var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

endpointBuilder.Headers.Add(AddressHeader.CreateAddressHeader("Authorization", "", $"Basic {encoded}"));

var endpointWithAuth = endpointBuilder.ToEndpointAddress();

#region Method 1

var factory = new ChannelFactory<IServiceChannel>(basicHttpBinding, endpointWithAuth);

factory.Credentials.UserName.UserName = username;
factory.Credentials.UserName.Password = password;

// ignore the self-signed TLS cert
factory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
{
    CertificateValidationMode = X509CertificateValidationMode.None,
    RevocationMode = X509RevocationMode.NoCheck
};

var channel = factory.CreateChannel();

var result = channel.GetData(3);

Console.WriteLine(result);

#endregion

#region Method 2

var client = (ServiceClient)Activator.CreateInstance(typeof(ServiceClient), basicHttpBinding, endpointWithAuth)!;

client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
{
    CertificateValidationMode = X509CertificateValidationMode.None,
    RevocationMode = X509RevocationMode.NoCheck
};

client.ClientCredentials.UserName.UserName = username;
client.ClientCredentials.UserName.Password = username;

var result2 = client.GetData(3);

Console.WriteLine(result2); 

#endregion