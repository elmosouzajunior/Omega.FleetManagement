using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [Authorize]
    [Route("api/v1/drivers")]
    public class DriversController : ApiControllerBase
    {
        private readonly IDriverAppService _driverAppService;
        private readonly ILogger<DriversController> _logger;

        public DriversController(IDriverAppService driverAppService, ILogger<DriversController> logger)
        {
            _driverAppService = driverAppService;
            _logger = logger;
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
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "criar motorista");
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
                return InternalServerError(_logger, ex, "listar motoristas");
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDriver(Guid id, [FromBody] EditDriverRequest dto)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                var updated = await _driverAppService.UpdateDriverAsync(id, dto, companyId);
                if (!updated)
                    return NotFound(new { success = false, message = "Motorista não encontrado." });

                return Ok(new { success = true, message = "Motorista atualizado com sucesso!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "atualizar motorista");
            }
        }

        private bool TryGetCompanyId(out Guid companyId)
        {
            return Guid.TryParse(User.FindFirst("CompanyId")?.Value, out companyId);
        }
    }
}
