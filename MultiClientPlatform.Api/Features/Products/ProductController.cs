using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiClientPlatform.Api.Features.Products.Dtos;
using MultiClientPlatform.Api.Features.Products.Interfaces;

namespace MultiClientPlatform.Api.Features.Products;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // Extracts UserId from the JWT token
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // GET api/product — publicly accessible, no token required
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProductResponse>>> GetAll()
    {
        List<ProductResponse> products = await _productService.GetAllAsync();
        return Ok(products);
    }

    // GET api/product/{id} — publicly accessible, no token required
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        ProductResponse? product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound("Product not found.");

        return Ok(product);
    }

    // POST api/product — Merchant only
    [HttpPost]
    [Authorize(Roles = "Merchant")]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        int userId = GetUserId();

        ProductResponse? response = await _productService.CreateAsync(userId, request);

        // Merchant profile does not exist yet
        if (response == null)
            return BadRequest("Merchant profile not found. Please create a merchant profile first.");

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }


    // PUT api/product/{id} — Merchant only, ownership enforced
    [HttpPut("{id}")]
    [Authorize(Roles = "Merchant")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
    {
        int userId = GetUserId();

        var (found, authorized, response) = await _productService.UpdateAsync(userId, id, request);
        //it is a tuple with 3 values, found and authorized are bools,
        //  response is ProductResponse
        //product found true
        //product authorised true
        //then return updated product response
        if (!found)
            return NotFound("Product not found.");

        if (!authorized)
            return Forbid();

        return Ok(response);
    }
}
