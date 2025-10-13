using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Oportuniza.API.Services;
using Oportuniza.API.Services.Oportuniza.API.Services;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Infrastructure.Data;
using Oportuniza.Infrastructure.Repositories;
using Oportuniza.Infrastructure.Services;
using static Oportuniza.API.Services.AzureEmailService;

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

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "DualJwt";
    options.DefaultChallengeScheme = "DualJwt";
});

authBuilder.AddJwtBearer("Keycloak", options =>
{
    var keycloak = builder.Configuration.GetSection("Keycloak");
    options.Authority = keycloak["Authority"];
    options.Audience = keycloak["Audience"];
    options.RequireHttpsMetadata = false;
});

authBuilder.AddJwtBearer("Backend", options =>
{
    var jwtSection = builder.Configuration.GetSection("jwt");
    var secret = jwtSection["secretKey"];
    if (string.IsNullOrEmpty(secret))
        throw new InvalidOperationException("jwt:secretKey não está configurado!");

    var key = System.Text.Encoding.ASCII.GetBytes(secret);

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSection["issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSection["audience"],
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthentication()
    .AddPolicyScheme("DualJwt", "JWT Dual Scheme", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);

                if (jwt.Claims.Any(c => c.Type == "company_id"))
                    return "Backend";

                return "Keycloak";
            }

            return "Keycloak";
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes("DualJwt")
        .RequireAuthenticatedUser()
        .Build();
});

//var authBuilder = builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = "Keycloak";
//    options.DefaultChallengeScheme = "Keycloak";
//});

//authBuilder.AddJwtBearer("Keycloak", options =>
//{
//    var keycloak = builder.Configuration.GetSection("Keycloak");
//    options.Authority = keycloak["Authority"];
//    options.Audience = keycloak["Audience"];
//    options.RequireHttpsMetadata = false;
//});

//builder.Services.AddAuthorization(options =>
//{
//    options.DefaultPolicy = new AuthorizationPolicyBuilder()
//        .AddAuthenticationSchemes("Keycloak")
//        .RequireAuthenticatedUser()
//        .Build();
//});

var AZURE_CONNECTION_STRING = Environment.GetEnvironmentVariable("AZURE_SQL_OPORTUNIZA");
var LOCAL_CONNECTION_STRING = Environment.GetEnvironmentVariable("LOCAL_SQL_OPORTUNIZA");

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthenticateUser, AuthenticateUser>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyEmployeeRepository, CompanyEmployeeRepository>();
builder.Services.AddScoped<IAreaOfInterest, AreaOfInterestRepository>();
builder.Services.AddScoped<IPublicationRepository, PublicationRepository>();
builder.Services.AddScoped<IUserAreaOfInterestRepository, UserAreaOfInterestRepository>();
builder.Services.AddScoped<ICandidateApplicationRepository, CandidateApplicationRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICompanyRoleRepository, CompanyRoleRepository>();
builder.Services.AddScoped<IActiveContextService, ActiveContextService>();

builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();
builder.Services.AddScoped<AzureEmailService.IEmailService, AzureEmailService.EmailService>();

builder.Services.AddScoped<UserRegistrationFilterAttribute>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddHttpClient<KeycloakAuthService>();

builder.Services.AddHttpClient<GeminiClientService>();

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