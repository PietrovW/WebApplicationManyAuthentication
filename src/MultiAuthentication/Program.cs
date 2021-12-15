using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MultiAuthentication.AuthenticationHandlers;
using MultiAuthentication.Constants;
using MultiAuthentication.Options;
using System.Net;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);
try
{
    
    builder.Configuration.AddEnvironmentVariables();
    var jwtOptions = new JwtOptions();
    builder.Configuration.GetSection(JwtOptions.Jwt).Bind(jwtOptions);
    builder.Services.AddAuthentication()
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
                                c.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                c.Response.ContentType = MediaTypeNames.Text.Plain;
                                return c.Response.WriteAsync(c.Exception.ToString());

                            }
                        };
                    })).AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthenticationConstants.BasicAuthentication, null);
    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(AuthenticationConstants.BasicAuthentication, JwtBearerDefaults.AuthenticationScheme)
            .Build();

        //var approvedPolicyBuilder = new AuthorizationPolicyBuilder()
          //     .RequireAuthenticatedUser()
            //   .AddAuthenticationSchemes(AuthenticationConstants.BasicAuthentication, JwtBearerDefaults.AuthenticationScheme);
        options.AddPolicy(PolicyConstants.Administrator, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireClaim("user_roles", "[Administrator]")
                .Build());

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