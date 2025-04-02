using EasyRepository.EFCore.Generic;
using Flexiro.Api.AutoMapper;
using Flexiro.API.Middleware;
using Flexiro.API.Swagger;
using Flexiro.Application.Database;
using Flexiro.Application.Models;
using Flexiro.Identity;
using Flexiro.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Text.Json;
using Braintree;
using Flexiro.Services.Services;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;

        string keyVaultUri = "https://flexirovault.vault.azure.net/";

        try
        {
            var secretClient = new SecretClient(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());

            string dbConnectionString = secretClient.GetSecret("FlexiroDbConnectionString").Value.ToString();
            config["ConnectionStrings:Database"] = dbConnectionString;

            string jwtKey = secretClient.GetSecret("JwtKey").Value.ToString();
            config["Jwt:Key"] = jwtKey;

            string blobStorageConnectionString = secretClient.GetSecret("BlobStorageConnectionString").Value.ToString();
            config["AzureBlobStorage:ConnectionString"] = blobStorageConnectionString;

            string braintreeMerchantId = secretClient.GetSecret("MerchantId").Value.ToString();
            string braintreePublicKey = secretClient.GetSecret("PublicKey").Value.ToString();
            string braintreePrivateKey = secretClient.GetSecret("PrivateKey").Value.ToString();

            config["Braintree:MerchantId"] = braintreeMerchantId;
            config["Braintree:PublicKey"] = braintreePublicKey;
            config["Braintree:PrivateKey"] = braintreePrivateKey;

            JwtTokenGenerator.Initialize(secretClient);

            Console.WriteLine("Successfully loaded secrets from Azure Key Vault");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading secrets from Key Vault: {ex.Message}");

            if (builder.Environment.IsDevelopment())
            {
                Console.WriteLine("Using local configuration values for development");
            }
            else
            {
                throw new Exception("Failed to load the configurations from the Key Vault");
            }
        }

        var connectionString = config.GetConnectionString("Database");

        builder.Services.AddDbContext<FlexiroDbContext>(options =>
                options.UseSqlServer(connectionString));
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                 .AddEntityFrameworkStores<FlexiroDbContext>()
                 .AddDefaultTokenProviders();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                ValidateIssuer = true,
                ValidateAudience = true
            };
        });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddAutoMapper(typeof(Program).Assembly);
        builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
        builder.Services.AddApplication();
        builder.Services.ApplyEasyRepository<FlexiroDbContext>();
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwagger>();
        builder.Services.AddControllers();

        builder.Services.AddSingleton<IBraintreeGateway>(sp =>
        {
            return new BraintreeGateway
            {
                Environment = Braintree.Environment.SANDBOX,
                MerchantId = config["Braintree:MerchantId"],
                PublicKey = config["Braintree:PublicKey"],
                PrivateKey = config["Braintree:PrivateKey"]
            };
        });

        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

        builder.Services.AddSignalR();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhostAndProduction",
                builder => builder
                    .WithOrigins("http://localhost:3000", "https://flexiroapi-d7akfuaug8d7esdg.uksouth-01.azurewebsites.net", "https://flexiroweb-h3g0fvfkdbhcdvgk.uksouth-01.azurewebsites.net")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors("AllowLocalhostAndProduction");
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/swagger/index.html");
                return;
            }
            await next();
        });
        app.UseMiddleware<ValidationMappingMiddleware>();
        app.UseMiddleware<JwtMiddleware>();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseStaticFiles();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<NotificationHub>("/notificationHub");
        });

        app.MapControllers();

        app.Run();
    }
}