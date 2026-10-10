using System.Text.Json.Serialization;
using FitTrack.Api.Repositories;
using FitTrack.Api.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        // Validation error keys use the JSON names ("durationMinutes"),
        // not the C# property names ("DurationMinutes").
        options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
    })
    .AddJsonOptions(options =>
    {
        // Clients send and receive "Running" instead of 0.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Repository is a singleton (it owns the file lock); service is scoped per request.
builder.Services.AddSingleton<IWorkoutRepository, JsonWorkoutRepository>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();

var app = builder.Build();

// One place for unexpected errors; returns a problem-details 500.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
