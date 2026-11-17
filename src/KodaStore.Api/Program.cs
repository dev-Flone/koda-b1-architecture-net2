using KodaStore.Api.Middlewares;
using KodaStore.Application.Repositories;
using KodaStore.Infrastructure.Repositories;
using Scalar.AspNetCore;
using KodaStore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Membaca daftar origin di env.Development
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

// Register CORS Configuration
builder.Services.AddCors(option =>
{
    option.AddPolicy("ReactClient", policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

// Scalar
builder.Services.AddOpenApi();

// DbContext
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add services to the container.

builder.Services.AddControllersWithViews();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(
    option =>
    {
        option.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance ??= context.HttpContext.Request.Path.Value;
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        };
    }
);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// EfCore (SQL Server)
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<IProductQueryRepository, EfProductQueryRepository>();

// Sql-Client
builder.Services.AddScoped<ICustomerRepository, SqlCustomerRepository>();
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
// builder.Services.AddScoped<IProductRepository, SqlProductRepository>();
// builder.Services.AddScoped<ICustomerRepository, SqlCustomerRepository>();
// builder.Services.AddScoped<IProductImageStorage, LocalProductImageStorage>();

builder.Services.AddInfrastructure(builder.Configuration);

// Service
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<RequestTiming>();

app.UseCors("ReactClient");

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.UseAuthorization();

app.Run();

