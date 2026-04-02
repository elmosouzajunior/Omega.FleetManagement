using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [Route("api/v1/expense-types")]
    public class ExpenseTypesController : ApiControllerBase
    {
        private readonly IExpenseTypeAppService _expenseTypeAppService;
        private readonly ILogger<ExpenseTypesController> _logger;

        public ExpenseTypesController(IExpenseTypeAppService expenseTypeAppService, ILogger<ExpenseTypesController> logger)
        {
            _expenseTypeAppService = expenseTypeAppService;
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

                var types = await _expenseTypeAppService.GetExpenseTypesAsync(companyId, includeInactive);
                return Ok(new { success = true, data = types });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "listar tipos de despesa");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Create([FromBody] CreateExpenseTypeRequest request)
        {
            try
            {
                var created = await _expenseTypeAppService.CreateAsync(request);
                return Ok(new { success = true, message = "Tipo de despesa cadastrado com sucesso.", data = created });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "criar tipo de despesa");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseTypeRequest request)
        {
            try
            {
                var updated = await _expenseTypeAppService.UpdateAsync(id, request);
                if (!updated)
                    return NotFound(new { success = false, message = "Tipo de despesa nao encontrado." });

                return Ok(new { success = true, message = "Tipo de despesa atualizado com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "atualizar tipo de despesa");
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateExpenseTypeStatusRequest request)
        {
            try
            {
                var updated = await _expenseTypeAppService.UpdateStatusAsync(id, request.IsActive);
                if (!updated)
                    return NotFound(new { success = false, message = "Tipo de despesa nao encontrado." });

                var message = request.IsActive
                    ? "Tipo de despesa ativado com sucesso."
                    : "Tipo de despesa inativado com sucesso.";

                return Ok(new { success = true, message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "atualizar status do tipo de despesa");
            }
        }
    }
}
