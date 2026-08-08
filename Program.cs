using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Repositories.Implementation;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using KejaHUnt_PropertiesAPI.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using Serilog;
using Microsoft.AspNetCore.ResponseCompression;
using StackExchange.Redis;
using KejaHUnt_PropertiesAPI.Services.Payments;  // Redis
using Minio; // Minio
using System.Text.Json.Serialization;
using KejaHUnt_PropertiesAPI.Services.WaterBills;

var builder = WebApplication.CreateBuilder(args);

var logPath = builder.Configuration.GetValue<string>("LogPath");
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: logPath ?? "/app/logs/property.log",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7
    ).CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add services to the container.
// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.AddDbContext<ApplicationDbContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Redis connection - ADD THIS
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});
builder.Services.AddScoped<ICacheService, CacheService>();

// MinIO
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    return new MinioClient()
        .WithEndpoint(builder.Configuration["ObjectStorage:Endpoint"] ?? "minio:9000")
        .WithCredentials(
            builder.Configuration["ObjectStorage:AccessKey"] ?? "minioadmin",
            builder.Configuration["ObjectStorage:SecretKey"] ?? "minioadmin123")
        .WithSSL(builder.Configuration.GetValue<bool>("ObjectStorage:UseSSL"))
        .Build();
});

builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPendingPropertyRepository, PendingPropertyRepository>();
builder.Services.AddScoped<IPendingPropertyService, PendingPropertyService>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IFeatureRepository, FeatureRepository>();
builder.Services.AddScoped<IUnitPaymentsRepository, UnitPaymentsRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IWaterRateRepository, WaterRateRepository>();
builder.Services.AddScoped<IWaterMeterReadingRepository, WaterMeterReadingRepository>();
builder.Services.AddScoped<IWaterBillRepository, WaterBillRepository>();
builder.Services.AddScoped<IWaterBillingService, WaterBillingService>();
builder.Services.AddHttpClient(); 
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                    "https://kejahunt.co.ke",
                    "https://portal.kejahunt.co.ke",
                    "http://localhost:8080"
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Added for debuging ***********************************************************************

app.UseResponseCompression();

app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
