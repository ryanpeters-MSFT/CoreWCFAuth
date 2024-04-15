using Microsoft.AspNetCore.Authentication;
using WcfService.Repositories;

var builder = WebApplication.CreateBuilder();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

builder.Services.AddTransient<Service>();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
builder.Services.AddSingleton<IAuthenticationRepository, MockAuthenticationRepository>();

builder.Services.AddAuthentication("Basic").AddScheme<AuthenticationSchemeOptions, BasicSchemeHandler>("Basic", options =>
{
    //
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    // Check if currently authenticated
    var authResult = await context.AuthenticateAsync("Basic");

    if (authResult.None)
    {
        // If the client hasn't authenticated, send a challenge to the client and complete request
        await context.ChallengeAsync("Basic");

        return;
    }

    // Call the next delegate/middleware in the pipeline.
    // Either the request was authenticated of it's for a path which doesn't require basic auth
    await next(context);
});

app.UseServiceModel(serviceBuilder =>
{
    var basicHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
    basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

    serviceBuilder.AddService<Service>(options =>
    {
        //options.BaseAddresses.Add(new Uri("https://anotherurl/service"));
    });

    serviceBuilder.AddServiceEndpoint<Service, IService>(basicHttpBinding, "/Service.svc");

    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpsGetEnabled = true;
});

app.Run();
