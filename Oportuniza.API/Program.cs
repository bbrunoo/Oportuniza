using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Oportuniza.API.Services;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Infrastructure.Data;
using Oportuniza.Infrastructure.Repositories;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<AzureBlobService>();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Insira **Apenas o Token** JWT ",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
});

var AZURE_CONNECTION_STRING = Environment.GetEnvironmentVariable("AZURE_SQL_OPORTUNIZA");
var LOCAL_CONNECTION_STRING = Environment.GetEnvironmentVariable("LOCAL_SQL_OPORTUNIZA");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    //options.UseSqlServer(AZURE_CONNECTION_STRING);
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthenticateUser, AuthenticateUser>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IAreaOfInterest, AreaOfInterestRepository>();
builder.Services.AddScoped<ICurriculumRepository, CurriculumRepository>();
builder.Services.AddScoped<IPublicationRepository, PublicationRepository>();
builder.Services.AddScoped<IUserAreaOfInterestRepository, UserAreaOfInterestRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddSingleton<SmsService>();
builder.Services.AddSingleton<OtpCacheService>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<OpenAIService>();

builder.Services.AddHttpClient<KeycloakAuthService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

//var jwtSettings = builder.Configuration.GetSection("jwt");
//var key = Encoding.ASCII.GetBytes(jwtSettings["secretKey"]);

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

authBuilder.AddMicrosoftIdentityWebApi(
    jwtBearerScheme: JwtBearerDefaults.AuthenticationScheme,
    configurationSection: builder.Configuration.GetSection("AzureAd")
);

authBuilder.AddJwtBearer("KeycloakScheme", options =>
{
    var keycloak = builder.Configuration.GetSection("Keycloak");
    options.Authority = keycloak["Authority"];
    options.Audience = keycloak["Audience"];
    options.RequireHttpsMetadata = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidAudience = keycloak["Audience"],
        ValidateIssuer = true,
        ValidIssuer = keycloak["Authority"],
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = "roles"
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim("idp", "keycloak"));
                if (!string.IsNullOrEmpty(identity.FindFirst("preferred_username")?.Value))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst("preferred_username")!.Value));
                }
            }
            Console.WriteLine($"Token validado pelo esquema KeycloakScheme. Name: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed for KeycloakScheme: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

//builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
//{
//    options.TokenValidationParameters.RoleClaimType = ClaimConstants.Role;
//});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "KeycloakScheme")
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();