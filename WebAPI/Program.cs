using Scalar.AspNetCore;
using Serilog;
using WebAPI;
using WebAPI.Common.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, logging) => logging
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddControllers(options => options.Filters.Add<ProblemDetailsLoggingFilter>());
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddFeatures();
builder.Services.AddDatabase(
    builder.Configuration.GetConnectionString("School")
    ?? throw new InvalidOperationException("The School connection string is required."),
    builder.Configuration.GetValue<bool>("Database:EnableSensitiveDataLogging"));

var app = builder.Build();
await app.Services.MigrateDatabaseAsync(app.Lifetime.ApplicationStopping);

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // OpenAPI document: /openapi/v1.json
    app.MapScalarApiReference();    // Interactive UI that reads it: /scalar
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
