using Infrastructure.Persistence;
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

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddWebServices();
    builder.Services.BindOptions(builder.Configuration);
    builder.Services.AddBindingValidation();
    builder.Services.TriggerBindingValidation();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        await app.InitialiseDatabaseAsync();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHealthChecks("/health");
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    //app.UseAuthorization();

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
