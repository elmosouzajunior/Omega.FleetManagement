using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [Authorize]
    [Route("api/v1/drivers")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly IDriverAppService _driverAppService;
        public DriversController(IDriverAppService driverAppService)
        {
            _driverAppService = driverAppService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverRequest dto)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                await _driverAppService.CreateDriverAsync(dto, companyId);
                return Ok(new { success = true, message = "Motorista cadastrado com sucesso!" });
            }
            catch (ArgumentException ex)
            {
                // Aqui vai cair a sua regra de "CPF já cadastrado"
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDriversByCompanyId()
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                var drivers = await _driverAppService.GetDriversByCompanyIdAsync(companyId);
                if (drivers == null || !drivers.Any())
                    return NoContent();
                return Ok(drivers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDriver(Guid id, [FromBody] EditDriverRequest dto)
        {
            try
            {
                await _driverAppService.UpdateDriverAsync(id, dto);
                return Ok(new { success = true, message = "Motorista atualizado com sucesso!" });
            }
            catch (ArgumentException ex)
            {
                // Aqui vai cair a sua regra de "CPF já cadastrado"
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
            }
        }

        private bool TryGetCompanyId(out Guid companyId)
        {
            return Guid.TryParse(User.FindFirst("CompanyId")?.Value, out companyId);
        }
    }
}

        

