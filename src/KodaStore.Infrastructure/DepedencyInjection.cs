using KodaStore.Application.Abstractions.Persistence;
using KodaStore.Application.Repositories;
using KodaStore.Application.Services;
using KodaStore.Infrastructure.Data;
using KodaStore.Infrastructure.Identity;
using KodaStore.Infrastructure.Persistence;
using KodaStore.Infrastructure.Repositories;
using KodaStore.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KodaStore.Infrastructure;

public static class DepedencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ecommercedb")
        ?? throw new InvalidOperationException("ConnectionString : Not yet configured.");

        services.AddDbContext<AppDbContext>(option =>
        {
            option.UseSqlServer(connectionString)
             .UseSeeding((context, _) =>
             {
                 CustomerSeedData.Seed(context);
                 ProductSeedData.Seed(context);
                 OrderSeedData.Seed(context);
                 CategorySeedData.Seed(context);
             })
             .UseAsyncSeeding(async (context, _, ct) =>
             {
                 await CustomerSeedData.SeedAsync(context, ct);
                 await ProductSeedData.SeedAsync(context, ct);
                 await OrderSeedData.SeedAsync(context, ct);
                 await CategorySeedData.SeedAsync(context, ct);
             });
        });

        services.AddScoped<IAuthService, IdentityAuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IProductRepository, EfProductRepository>();
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<IProductQueryRepository, EfProductQueryRepository>();

        services.AddScoped<ICustomerRepository, SqlCustomerRepository>();
        services.AddScoped<ICategoryRepository, EfCategoryRepository>();

        services.AddIdentityCore<AppUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredUniqueChars = 4;

            options.SignIn.RequireConfirmedEmail = true;
        }).AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<AppDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        return services;
    }
}