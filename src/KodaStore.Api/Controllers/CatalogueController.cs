using KodaStore.Api.ViewModels;
using KodaStore.Application.DTOs;
using KodaStore.Application.Security;
using KodaStore.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KodaStore.Api.Controllers;


[Route("catalogue")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class CatalogueController(IProductService service) : Controller
{
    private readonly IProductService _service = service;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await _service.GetAllAsync(cancellationToken);

        return View(products);
    }

    [HttpGet("details/{productCode}")]
    public async Task<IActionResult> Details(string productCode, CancellationToken cancellationToken)
    {
        var product = await _service.GetByCodeAsync(productCode, cancellationToken);

        if (product is null) return NotFound();

        return View(product);
    }

    [HttpGet("create")]
    [ValidateAntiForgeryToken]
    public IActionResult Create()
    {
        return View(new CreateProductViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var request = new ProductRequest(
                model.CustomerId,
                model.ProductCode,
                model.Name,
                model.Price ?? 0,
                model.Stock ?? 0,
                model.Category
            );

            var created = await _service.CreateAsync(request, cancellationToken);

            if (created is null)
            {
                ModelState.AddModelError(nameof(model.ProductCode), "Product Code already used");

                return View(model);
            }

            TempData["Success"] = "Product added successfully";

            return RedirectToAction(nameof(Details), new { productCode = created.ProductCode });
        }
        catch (ArgumentException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);

            return View(model);
        }
    }

    [HttpGet("edit/{productCode}")]
    public async Task<IActionResult> Edit(string productCode, CancellationToken cancellationToken)
    {
        var product = await _service.GetByCodeAsync(productCode, cancellationToken);

        if (product is null) return NotFound();

        ViewData["ProductCode"] = product.ProductCode;

        var model = new EditProductViewModel
        {
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            Category = product.Category
        };

        return View(model);
    }

    [HttpPost("edit/{productCode}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string productCode, EditProductViewModel model, CancellationToken cancellationToken)
    {
        ViewData["ProductCode"] = productCode;

        if (!ModelState.IsValid) return View(model);

        try
        {
            var request = new UpdateProductRequest(
                model.CustomerId,
                model.Name,
                model.Price ?? 0,
                model.Stock ?? 0,
                model.Category
            );

            var updated = await _service.UpdateAsync(productCode, request, cancellationToken);

            if (updated is null) return NotFound();

            TempData["Success"] = "Product updated successfully";

            return RedirectToAction(nameof(Details), new { productCode = updated.ProductCode });
        }
        catch (ArgumentException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);

            return View(model);
        }
    }

    [HttpGet("delete/{productCode}")]
    public async Task<IActionResult> Delete(string productCode, CancellationToken cancellationToken)
    {
        var product = _service.DeleteAsync(productCode, cancellationToken);

        if (product is null) return NotFound();

        return View(product);
    }

    [HttpPost("delete/{productCode}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string productCode, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(productCode, cancellationToken);

        if (!deleted) return NotFound();

        TempData["Success"] = "Product deleted successfully";

        return RedirectToAction(nameof(Index));
    }
}
