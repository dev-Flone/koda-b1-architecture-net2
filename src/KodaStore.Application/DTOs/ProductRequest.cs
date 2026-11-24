using System.ComponentModel.DataAnnotations;

namespace KodaStore.Application.DTOs;

public record ProductRequest(
    int CustomerId,

    [Required, StringLength(30)]
    string ProductCode,

    [Required, StringLength(120)]
    string Name,

    [Required, Range(typeof(long), "0", "9999999999")]
    decimal Price,

    [Required, Range(0, int.MaxValue)]
    int Stock,

    [Required, StringLength(120)]
    string Category
    );
