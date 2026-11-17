namespace KodaStore.Domain.Entities;

public sealed class Category
{
    private Category() { }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ICollection<Product> Products { get; private set; } = [];
    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 30) throw new ArgumentException("Category Name must be filled with maximum 30 characters.");

        Name = name.Trim();
    }
}
