using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using MultiAuthentication.AuthenticationHandlers;
using MultiAuthentication.Constants;
using MultiAuthentication.Options;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using System.Net.Mime;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
var jwtOptions = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.Jwt).Bind(jwtOptions);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "My API V1");
    });
}
app.MapGet("/auth", [Authorize] () => "This endpoint requires authorization.");
app.Run();
