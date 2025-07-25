﻿using Application.Common.Interfaces;
using Application.Common.Models;
using FluentValidation.AspNetCore;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using WebApi.Filters;
using WebApi.Services;


namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddScoped<IUser, CurrentUser>();

        services.AddHttpContextAccessor();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddControllers(options =>
        {
            options.Filters.Add<ApiExceptionFilterAttribute>();
        });

        services.AddFluentValidationAutoValidation(options =>
         {
             options.DisableDataAnnotationsValidation = true; 
         })
        .AddFluentValidationClientsideAdapters();


        services.AddRazorPages();
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.None;
            options.Secure = CookieSecurePolicy.Always;
            options.HttpOnly = AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
        });

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        services.AddEndpointsApiExplorer();

        //services.AddOpenApiDocument((configure, sp) =>
        //{
        //    configure.Title = "Template API";

        //    // Add JWT
        //    configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
        //    {
        //        Type = OpenApiSecuritySchemeType.ApiKey,
        //        Name = "Authorization",
        //        In = OpenApiSecurityApiKeyLocation.Header,
        //        Description = "Type into the textbox: Bearer {your JWT token}."
        //    });

        //    configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        //});

        return services;
    }
    public static IServiceCollection AddKeyVaultIfConfigured(this IServiceCollection services, ConfigurationManager configuration)
    {
        //Implementation Here
        return services;
    }

    public static IServiceCollection BindOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtDetail>(configuration.GetSection("Jwt"));
        //services.Configure<EncryptionDetails>(configuration.GetSection("EncryptionDetails"));
        EncryptionDetails.ClientKey = configuration["EncryptionDetails:ClientKey"]??"";
        EncryptionDetails.ClientSalt = configuration["EncryptionDetails:ClientSalt"] ?? "";
        return services;
    }
    public static IServiceCollection OptionsPostConfiguration(this IServiceCollection services)
    {
        //services.PostConfigure<EncryptionDetails>(appOptions => {
        //    var validator = new EncryptionDetailsValidator();
        //    var result = validator.Validate(appOptions);
        //    if (!result.IsValid)
        //    {
        //        throw new Exception(result.Errors.FirstOrDefault()?.ErrorMessage);
        //    }
        //});
        return services;
    }
    
}
