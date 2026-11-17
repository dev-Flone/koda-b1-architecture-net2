using KodaStore.Application.Repositories;
using KodaStore.Application.Services;
using KodaStore.Infrastructure.Data;
using KodaStore.Infrastructure.Identity;
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

            services.AddScoped<IProductQueryService, ProductQueryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAuthService, IdentityAuthService>();
        });

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

        return services;


    }
}