using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

builder.Services.AddTransient<Service>();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

builder.Services.AddAuthentication(AuthenticationScheme)).AddJwtBearer(options =>
{
    var issuerSigningKey = builder.Configuration["IssuerSigningKey"];

    //options.Authority = "https://authorization-server-uri";
    //options.Audience = "my-audience";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        RequireExpirationTime = false,
        //ValidateActor = false,
        //ValidateLifetime = true,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireClaim("name")
        .Build();

    options.AddPolicy("is-teacher", policy => policy.RequireClaim("position", "teacher"));
});

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    var basicHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
    basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.InheritedFromHost;

    serviceBuilder.AddService<Service>(options => 
    {
        //options.BaseAddresses.Add(new Uri("https://anotherurl/service"));
    });

    serviceBuilder.AddServiceEndpoint<Service, IService>(basicHttpBinding, "/Service.svc");

    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpsGetEnabled = true;
});

app.Run();
