using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserService.Application;
using UserService.Application.Options;
using UserService.Infrastructure;
using UserService.Infrastructure.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration["AdminUiUrl"] ?? "http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UserService API",
        Version = "v1"
    });
});

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection(AppSettings.SectionName));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors();
app.MapControllers();

await app.RunAsync();

