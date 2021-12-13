using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MultiAuthentication.AuthenticationHandlers;
using MultiAuthentication.Options;

string JWTAuthentication = nameof(JWTAuthentication);
string BasicAuthentication = nameof(BasicAuthentication);
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
builder.Configuration.AddEnvironmentVariables();
var jwtOptions = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.Jwt).Bind(jwtOptions);
builder.Services.AddAuthentication()
     .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthentication, null)
.AddJwtBearer(JWTAuthentication, (o =>
                {
                   o.MetadataAddress = jwtOptions.Authority;
                    o.RequireHttpsMetadata = false; // only for dev
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        // NOTE: Usually you don't need to set the issuer since the middleware will extract it 
                        // from the .well-known endpoint provided above. but since I am using the container name in
                        // the above URL which is not what is published issueer by the well-known, I'm setting it here.
                        ValidIssuer = "http://localhost:8080/auth/realms/AuthDemoRealm",

                        ValidAudience = "auth-demo-web-api",
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                }));
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(BasicAuthentication, JwtBearerDefaults.AuthenticationScheme)
        .Build();

    var approvedPolicyBuilder = new AuthorizationPolicyBuilder()
           .RequireAuthenticatedUser()
           .AddAuthenticationSchemes(BasicAuthentication, JwtBearerDefaults.AuthenticationScheme)
           ;
    options.AddPolicy("Administrator", new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(JWTAuthentication)
            .RequireClaim("user_roles", "[Administrator]")
            .Build());

    // options.AddPolicy("Administrator", approvedPolicyBuilder.Build());
});

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => "Hello World!").RequireAuthorization();

app.Run();
