using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [Authorize]
    [Route("api/v1/vehicles")]
    public class VehiclesController : ApiControllerBase
    {
        private readonly IVehicleAppService _vehicleAppService;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(IVehicleAppService vehicleAppService, ILogger<VehiclesController> logger)
        {
            _vehicleAppService = vehicleAppService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest dto)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                await _vehicleAppService.CreateVehicleAsync(dto, companyId);
                return Ok(new { success = true, message = "Veículo cadastrado com sucesso!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "criar veículo");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVehiclesByCompanyId()
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                var vehicles = await _vehicleAppService.GetVehiclesByCompanyIdAsync(companyId);
                if (vehicles == null || !vehicles.Any())
                    return NoContent();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "listar veículos");
            }
        }

        [HttpPatch("{id}/assign-driver")]
        public async Task<IActionResult> AssignDriver(Guid id, [FromBody] AssignDriverRequest request)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                var success = await _vehicleAppService.AssignDriverAsync(id, request?.DriverId, companyId);

                if (!success)
                    return NotFound(new { message = "Veículo não encontrado." });

                var message = request?.DriverId == null
                    ? "Motorista desvinculado com sucesso!"
                    : "Motorista vinculado com sucesso!";

                return Ok(new { message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de negócio ao vincular motorista ao veículo");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "vincular motorista ao veículo");
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] EditVehicleRequest dto)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                var updated = await _vehicleAppService.UpdateVehicleAsync(id, dto, companyId);
                if (!updated)
                    return NotFound(new { success = false, message = "Veículo não encontrado." });

                return Ok(new { success = true, message = "Veículo atualizado com sucesso!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "atualizar veículo");
            }
        }

        [HttpPost("{id}/expenses")]
        public async Task<IActionResult> AddExpense(Guid id, [FromBody] CreateVehicleExpenseRequest request)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                await _vehicleAppService.AddExpenseAsync(id, request, companyId);
                return Ok(new { success = true, message = "Despesa lançada para o veículo com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "adicionar despesa do veículo");
            }
        }

        private bool TryGetCompanyId(out Guid companyId)
        {
            return Guid.TryParse(User.FindFirst("CompanyId")?.Value, out companyId);
        }
    }
}
