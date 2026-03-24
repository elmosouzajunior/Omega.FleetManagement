using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [Route("api/v1/expense-types")]
    [ApiController]
    public class ExpenseTypesController : ControllerBase
    {
        private readonly IExpenseTypeAppService _expenseTypeAppService;

        public ExpenseTypesController(IExpenseTypeAppService expenseTypeAppService)
        {
            _expenseTypeAppService = expenseTypeAppService;
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
                Console.WriteLine($"[Erro Critico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Create([FromBody] CreateExpenseTypeRequest request)
        {
            try
            {
                await _expenseTypeAppService.CreateAsync(request);
                return Ok(new { success = true, message = "Tipo de despesa cadastrado com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro Critico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
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
                Console.WriteLine($"[Erro Critico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
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
                Console.WriteLine($"[Erro Critico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
            }
        }
    }
}
