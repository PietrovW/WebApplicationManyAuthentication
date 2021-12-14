using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MultiAuthentication.AuthenticationHandlers;
using MultiAuthentication.Options;

string JWTAuthentication = nameof(JWTAuthentication);
string BasicAuthentication = nameof(BasicAuthentication);
var builder = WebApplication.CreateBuilder(args);
try
{
   
   builder.Configuration.AddEnvironmentVariables();
    var jwtOptions = new JwtOptions();
    builder.Configuration.GetSection(JwtOptions.Jwt).Bind(jwtOptions);
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
      options.Cookie.HttpOnly = true;
      options.Cookie.SecurePolicy = CookieSecurePolicy.None ;
      options.Cookie.SameSite = SameSiteMode.Lax;
  })
      //  .AddCookie(cfg => cfg.SlidingExpiration = true)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, (o =>
                    {
                        o.Authority = jwtOptions.Authority;
                        o.Audience = jwtOptions.Audience;
                        o.RequireHttpsMetadata = false;
                        o.Events = new JwtBearerEvents()
                        {
                            OnAuthenticationFailed = c =>
                            {
                                c.NoResult();
                                c.Response.StatusCode = 500;
                                c.Response.ContentType = "text/plain";
                                return c.Response.WriteAsync(c.Exception.ToString());

                            }
                        };
                    })).AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthentication, null);
    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(BasicAuthentication, JwtBearerDefaults.AuthenticationScheme)
            .Build();

        var approvedPolicyBuilder = new AuthorizationPolicyBuilder()
               .RequireAuthenticatedUser()
               .AddAuthenticationSchemes(BasicAuthentication, JwtBearerDefaults.AuthenticationScheme);
        options.AddPolicy("Administrator", new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireClaim("user_roles", "[Administrator]")
                .Build());

            // options.AddPolicy("Administrator", approvedPolicyBuilder.Build());
        });
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
        );
    });
    var app = builder.Build();

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapGet("/auth", [Authorize] () => "This endpoint requires authorization.");

    app.Run();
}
catch (Exception ex)
{

}