using KodaStore.Application.Repositories;
// using KodaStore.Infrastructure.Persistence;
// using KodaStore.Domain.Entities;

// namespace KodaStore.Infrastructure.Repositories;

// public sealed class InMemoryProductRepository(InMemoryProductCatalogue catalogue) : IProductRepository
// {
//     private readonly InMemoryProductCatalogue _catalogue = catalogue;

//     public IReadOnlyList<Product> GetAll() => _catalogue.GetAll();
//     public Product? GetByCode(string productCode) => _catalogue.GetByCode(productCode);

//     public bool TryAdd(Product product) => _catalogue.TryCreateProduct(product);
//     public bool TryUpdate(Product product) => _catalogue.TryUpdateProduct(product);

//     public bool TryDelete(string productCode) => _catalogue.TryDeleteProduct(productCode);
// }

