using System.ComponentModel.DataAnnotations;

namespace KodaStore.Domain.Entities;

public sealed class Product(int customerId, string productCode, string name, decimal price, int stock, string category)
{
    [Required]
    public int CustomerId { get; private set; } = customerId;
    [Required]
    public string ProductCode { get; private set; } = productCode;
    [Required]
    public string Name { get; private set; } = name;
    [Required, Range(10000, 1000000000)]
    public decimal Price { get; private set; } = price;
    [Required]
    public int Stock { get; private set; } = stock;
    public int? CategoryId { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTime? DeactivatedAtUtc { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public Category? CategoryEntity { get; private set; } = null!;
    public byte[] RowVersion { get; private set; } = [];
    public Customer? Customer { get; private set; }
    public ICollection<OrderItem> OrderItems { get; private set; } = [];

    public void UpdateDetail(int customerId, string productCode, string name, decimal price, int stock, string category)
    {
        var validated = new Product(customerId, productCode, name, price, stock, category);

        CustomerId = validated.CustomerId;
        ProductCode = validated.ProductCode;
        Name = validated.Name;
        Price = validated.Price;
        Stock = validated.Stock;
        Category = validated.Category;
    }

    public void AssignCategory(Category categoryEntity)
    {
        ArgumentNullException.ThrowIfNull(category);
        if (categoryEntity.Id <= 0) throw new InvalidOperationException("Save category first to get CategoryId");

        CategoryId = categoryEntity.Id;
        CategoryEntity = categoryEntity;
        Name = categoryEntity.Name;
    }

    public void Activate()
    {
        IsActive = true;
        DeactivatedAtUtc = null;
    }

    public void Deactivate()
    {
        IsActive = false;
        DeactivatedAtUtc = DateTime.UtcNow;
    }
}
