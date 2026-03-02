using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [Route("api/v1/trips")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripAppService _tripAppService;
        public TripsController(ITripAppService tripAppService)
        {
            _tripAppService = tripAppService;
        }

        [HttpPost("open")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> OpenTrip([FromForm] OpenTripRequest dto)
        {
            try
            {
                var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value);
                await _tripAppService.OpenTripAsync(dto, companyId);
                return Ok(new { message = "Viagem aberta com sucesso!" });
            }
            catch (InvalidOperationException ex) // Comum para regras de negócio violadas
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Logue o erro no console do VS para você ver o que é
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");

                return StatusCode(500, new
                {
                    message = "Erro interno no servidor",
                    details = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value);
                var trips = await _tripAppService.GetTripsByCompanyIdAsync(companyId);

                if (trips == null || !trips.Any())
                {
                    // Retornamos 200 OK, mas com uma mensagem de aviso e lista vazia
                    return Ok(new
                    {
                        success = true,
                        message = "Não existe nenhuma viagem cadastrada no sistema.",
                        data = new List<object>()
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"{trips.Count()} viagens encontradas.",
                    data = trips
                });
            }
            catch (Exception ex)
            {
                // Logue o erro no console do VS para você ver o que é
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Erro interno no servidor",
                    details = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value);
                var trip = await _tripAppService.GetTripByIdAsync(id, companyId);
                if (trip == null)
                {
                    return NotFound(new { message = "Viagem não encontrada." });
                }
                return Ok(new
                {
                    success = true,
                    message = "Viagem encontrada.",
                    data = trip
                });
            }
            catch (Exception ex)
            {
                // Logue o erro no console do VS para você ver o que é
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Erro interno no servidor",
                    details = ex.Message
                });
            }
        }
    }
}
