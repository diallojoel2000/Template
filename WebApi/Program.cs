using Application.Common.Mappings;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Enrichers.Sensitive;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext().Enrich.WithSensitiveDataMasking(options =>
    {
        //Add other secrets here
        options.MaskProperties.Add("X-CallerPassword");
        options.MaskProperties.Add("password");
        options.MaskProperties.Add("token");
        options.MaskProperties.Add("refreshToken");
        options.MaskProperties.Add("answer");
        options.MaskingOperators = new List<IMaskingOperator>
        {
            new IbanMaskingOperator(),
            new CreditCardMaskingOperator(),
            // etc etc
        };
    })
    .ReadFrom.Configuration(config)
        .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddKeyVaultIfConfigured(builder.Configuration);

    //The arrangement is very important or you gonna learn
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddWebServices();
    builder.Services.BindOptions(builder.Configuration);
    builder.Services.OptionsPostConfiguration();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
        
    builder.Services.AddCors(option => 
    {
        var origins = builder.Configuration.GetSection("AllowedCorsOrigin").Get<string[]>();
        option.AddPolicy("AllowAll", builder =>
        {
            builder
            .WithOrigins(origins?? [""])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            ;
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });
    }

    await app.InitialiseDatabaseAsync();

    app.UseCors("AllowAll");
    app.UseHealthChecks("/health");
    
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCookiePolicy();

    app.MapControllers();

    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
