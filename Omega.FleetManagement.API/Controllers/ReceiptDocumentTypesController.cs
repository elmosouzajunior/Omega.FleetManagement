using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [Route("api/v1/receipt-document-types")]
    public class ReceiptDocumentTypesController : ApiControllerBase
    {
        private readonly IReceiptDocumentTypeAppService _receiptDocumentTypeAppService;
        private readonly ILogger<ReceiptDocumentTypesController> _logger;

        public ReceiptDocumentTypesController(IReceiptDocumentTypeAppService receiptDocumentTypeAppService, ILogger<ReceiptDocumentTypesController> logger)
        {
            _receiptDocumentTypeAppService = receiptDocumentTypeAppService;
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

                var items = await _receiptDocumentTypeAppService.GetReceiptDocumentTypesAsync(companyId, includeInactive);
                return Ok(new { success = true, data = items });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "listar tipos de documento de recebimento");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Create([FromBody] CreateReceiptDocumentTypeRequest request)
        {
            try
            {
                var created = await _receiptDocumentTypeAppService.CreateAsync(request);
                return Ok(new { success = true, message = "Tipo de documento cadastrado com sucesso.", data = created });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "criar tipo de documento de recebimento");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReceiptDocumentTypeRequest request)
        {
            try
            {
                var updated = await _receiptDocumentTypeAppService.UpdateAsync(id, request);
                if (!updated)
                    return NotFound(new { success = false, message = "Tipo de documento nao encontrado." });

                return Ok(new { success = true, message = "Tipo de documento atualizado com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "atualizar tipo de documento de recebimento");
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateReceiptDocumentTypeStatusRequest request)
        {
            try
            {
                var updated = await _receiptDocumentTypeAppService.UpdateStatusAsync(id, request.IsActive);
                if (!updated)
                    return NotFound(new { success = false, message = "Tipo de documento nao encontrado." });

                var message = request.IsActive
                    ? "Tipo de documento ativado com sucesso."
                    : "Tipo de documento inativado com sucesso.";

                return Ok(new { success = true, message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "atualizar status do tipo de documento de recebimento");
            }
        }
    }
}
