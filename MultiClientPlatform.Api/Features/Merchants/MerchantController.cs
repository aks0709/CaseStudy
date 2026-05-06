using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiClientPlatform.Api.Features.Merchants.Dtos;
using MultiClientPlatform.Api.Features.Merchants.Interfaces;
using MultiClientPlatform.Api.Features.Products.Dtos;
using MultiClientPlatform.Api.Features.Products.Interfaces;

namespace MultiClientPlatform.Api.Features.Merchants;

[ApiController]
[Route("api/[controller]")]
public class MerchantController : ControllerBase
{
    private readonly IMerchantService _merchantService;
    private readonly IProductService _productService;

    public MerchantController(IMerchantService merchantService, IProductService productService)
    {
        _merchantService = merchantService;
        _productService = productService;
    }

    // Extracts UserId from the JWT token (ClaimTypes.NameIdentifier)
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // GET api/merchant — publicly accessible
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<MerchantResponse>>> GetAll()
    {
        List<MerchantResponse> merchants = await _merchantService.GetAllAsync();
        return Ok(merchants);
    }

    // GET api/merchant/{id} — publicly accessible
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MerchantResponse>> GetById(int id)
    {
        MerchantResponse? merchant = await _merchantService.GetByIdAsync(id);
        if (merchant == null)
            return NotFound("Merchant not found.");

        return Ok(merchant);
    }

    // GET api/merchant/{id}/products — publicly accessible
    [HttpGet("{id}/products")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProductResponse>>> GetProductsByMerchant(int id)
    {
        MerchantResponse? merchant = await _merchantService.GetByIdAsync(id);
        if (merchant == null)
            return NotFound("Merchant not found.");

        List<ProductResponse> products = await _productService.GetByMerchantIdAsync(id);
        return Ok(products);
    }

    // POST api/merchant/profile — Merchant role only
    [HttpPost("profile")]
    [Authorize(Roles = "Merchant")]
    public async Task<ActionResult<MerchantResponse>> CreateProfile(CreateMerchantRequest request)
    {
        int userId = GetUserId();

        MerchantResponse? response = await _merchantService.CreateProfileAsync(userId, request);

        if (response == null)
            return Conflict("Merchant profile already exists for this account.");

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // GET api/merchant/my-profile — Merchant role only (renamed to avoid conflict with {id} route)
    [HttpGet("my-profile")]
    [Authorize(Roles = "Merchant")]
    public async Task<ActionResult<MerchantResponse>> GetMyProfile()
    {
        int userId = GetUserId();

        MerchantResponse? response = await _merchantService.GetMyProfileAsync(userId);

        if (response == null)
            return NotFound("No merchant profile found. Please create one first.");

        return Ok(response);
    }
}
