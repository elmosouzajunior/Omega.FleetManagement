using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [Route("api/v1/products")]
    public class ProductsController : ApiControllerBase
    {
        private readonly IProductAppService _productAppService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductAppService productAppService, ILogger<ProductsController> logger)
        {
            _productAppService = productAppService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] Guid? companyId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                if (!User.IsInRole("Master"))
                {
                    var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                    if (!Guid.TryParse(companyIdClaim, out var adminCompanyId))
                        return Unauthorized(new { message = "Token sem empresa valida." });

                    companyId = adminCompanyId;
                }

                var products = await _productAppService.GetProductsAsync(companyId, includeInactive);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "listar produtos");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            try
            {
                var created = await _productAppService.CreateAsync(request);
                return Ok(new { success = true, message = "Produto cadastrado com sucesso.", data = created });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "criar produto");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                var updated = await _productAppService.UpdateAsync(id, request);
                if (!updated)
                    return NotFound(new { success = false, message = "Produto nao encontrado." });

                return Ok(new { success = true, message = "Produto atualizado com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "atualizar produto");
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateProductStatusRequest request)
        {
            try
            {
                var updated = await _productAppService.UpdateStatusAsync(id, request.IsActive);
                if (!updated)
                    return NotFound(new { success = false, message = "Produto nao encontrado." });

                var message = request.IsActive
                    ? "Produto ativado com sucesso."
                    : "Produto inativado com sucesso.";

                return Ok(new { success = true, message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "atualizar status do produto");
            }
        }
    }
}
