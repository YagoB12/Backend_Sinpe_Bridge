using Backend_Bridge.Data;
using Backend_Bridge.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("products")]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_context.Products.ToList());
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();

        return Ok(product);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Product updated)
    {
        var product = _context.Products.Find(id);

        if (product == null) return NotFound();

        product.Name = updated.Name;
        product.Price = updated.Price;
        product.Stock = updated.Stock;

        _context.SaveChanges();

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var product = _context.Products.Find(id);

        if (product == null) return NotFound();

        _context.Products.Remove(product);
        _context.SaveChanges();

        return Ok();
    }
}