using Microsoft.OpenApi;
using PutZige.Application;
using PutZige.Infrastructure;
using PutZige.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PutZige API", Version = "v1" });
});

// Register layer services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // FIRST

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization(); 
app.MapControllers();
app.Run();
