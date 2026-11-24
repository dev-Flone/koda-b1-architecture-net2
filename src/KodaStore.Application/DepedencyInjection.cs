using KodaStore.Application.Repositories;
using KodaStore.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KodaStore.Application;

public static class DepedencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductQueryService, ProductQueryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICategoryService, CategoryService>(); ;

        return services;
    }
}
