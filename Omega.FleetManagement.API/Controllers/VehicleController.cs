using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Application.Services;

namespace Omega.FleetManagement.API.Controllers
{
    [Route("api/v1/vehicles")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleAppService _vehicleAppService;
        public VehiclesController(IVehicleAppService vehicleAppService)
        {
            _vehicleAppService = vehicleAppService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest dto)
        {
            try
            {
                var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value);
                await _vehicleAppService.CreateVehicleAsync(dto, companyId);
                return Ok(new { success = true, message = "Veículo cadastrado com sucesso!" });
            }
            catch (ArgumentException ex)
            {
                // Aqui vai cair a sua regra de placas já cadastradas"
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVehiclesByCompanyId()
        {
            try
            {
                var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value);
                var vehicles = await _vehicleAppService.GetVehiclesByCompanyIdAsync(companyId);
                if (vehicles == null || !vehicles.Any())
                    return NoContent();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
            }
        }

        [HttpPatch("{id}/assign-driver")]
        public async Task<IActionResult> AssignDriver(Guid id, [FromBody] Guid driverId)
        {
            try
            {
                var success = await _vehicleAppService.AssignDriverAsync(id, driverId);

                if (!success)
                    return NotFound(new { message = "Veículo não encontrado." });

                return Ok(new { message = "Motorista vinculado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] EditVehicleRequest dto)
        {
            try
            {
                await _vehicleAppService.UpdateVehicleAsync(id, dto);
                return Ok(new { success = true, message = "Veículo atualizado com sucesso!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erro interno no servidor", details = ex.Message });
            }
        }
    }
}
