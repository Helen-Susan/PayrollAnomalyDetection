using anamoly_detection_api.Data;
using anamoly_detection_api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

// Controllers
builder.Services.AddOpenApi();
builder.Services.AddControllers();


// CORS ✅
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IRegisterSevice, RegisterService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<ICycleInformationService, CycleInformationService>();
builder.Services.AddScoped<IAiCycleInformationService, AiCycleInformationService>();
builder.Services.AddScoped<IDeleteCycleService, DeleteCycleService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAnalyticsPersistenceService,AnalyticsPersistenceService>();
builder.Services.AddScoped<IRiskScoreService,RiskScoreService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IFabricIngestionService, FabricIngestionService>();
builder.Services.AddScoped<IFabricAnomalySyncService, FabricAnomalySyncService>();
builder.Services.AddHostedService<PayrollProcessingBackgroundService>();
builder.Services.AddScoped<INotificationService,NotificationService>();
builder.Services.AddScoped<IExecutiveReportAgentService, ExecutiveReportAgentService>();
builder.Services.AddHttpClient();



var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    // Generates the OpenAPI JSON spec
    app.MapOpenApi();

    // Scalar UI — access at /scalar/v1
    app.MapScalarApiReference();
}


// Middleware
app.UseHttpsRedirection();


// ENABLE CORS BEFORE AUTHORIZATION ✅
app.UseCors("AllowAngular");
app.UseCors("AllowAll");
app.Use(async (context, next) =>
{
    Console.WriteLine($"REQUEST RECEIVED: {context.Request.Method} {context.Request.Path}");

    await next();
});

app.UseAuthorization();

app.MapControllers();

app.Run();