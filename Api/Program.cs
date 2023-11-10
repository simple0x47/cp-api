using System.Security.Claims;
using Core.Secrets;
using Cuplan.Organization.Models;
using Cuplan.Organization.Models.Authentication;
using Cuplan.Organization.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Organization;
using Organization.Config;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["IdentityProvider:Authority"];
    options.Audience = builder.Configuration["IdentityProvider:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend",
        policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("Cors").GetSection("Origins").Get<string[]>())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

Initialization initialization = new(builder);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddNewtonsoftJson();

// Other dependencies
builder.Services.AddSingleton<MongoClient>(sp => new MongoClient(initialization.GetMongoDbUri()));
builder.Services.AddSingleton<ISecretsManager, BitwardenSecretsManager>();
builder.Services.AddSingleton<ConfigurationReader>();

// Services
builder.Services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddSingleton<IRoleRepository, RoleRepository>();
builder.Services.AddSingleton<IMemberRepository, MemberRepository>();
builder.Services.AddSingleton<IAuthProvider, Auth0Provider>();

// Models
builder.Services.AddScoped<MembershipManager>();
builder.Services.AddScoped<OrganizationManager>();
builder.Services.AddScoped<RoleManager>();
builder.Services.AddScoped<Authenticator>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}