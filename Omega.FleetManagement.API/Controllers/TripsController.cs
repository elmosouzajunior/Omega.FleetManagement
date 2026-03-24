using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [Authorize]
    [Route("api/v1/trips")]
    public class TripsController : ApiControllerBase
    {
        private readonly ITripAppService _tripAppService;
        private readonly ILogger<TripsController> _logger;

        public TripsController(ITripAppService tripAppService, ILogger<TripsController> logger)
        {
            _tripAppService = tripAppService;
            _logger = logger;
        }

        [HttpPost("open")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> OpenTrip([FromForm] OpenTripRequest dto)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { message = "CompanyId inválido no token." });

                await _tripAppService.OpenTripAsync(dto, companyId);
                return Ok(new { message = "Viagem aberta com sucesso!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "abrir viagem");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { message = "CompanyId inválido no token." });

                var trips = await _tripAppService.GetTripsByCompanyIdAsync(companyId);

                if (trips == null || !trips.Any())
                {
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
                return InternalServerError(_logger, ex, "listar viagens");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { message = "CompanyId inválido no token." });

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
                return InternalServerError(_logger, ex, "buscar viagem por id");
            }
        }

        [HttpPatch("{id}/reopen")]
        public async Task<IActionResult> Reopen(Guid id)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                await _tripAppService.ReopenTripAsync(id, companyId);
                return Ok(new { success = true, message = "Viagem reaberta com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "reabrir viagem");
            }
        }

        [HttpPatch("{id}/finish")]
        public async Task<IActionResult> Finish(Guid id, [FromBody] FinishTripRequest request)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                await _tripAppService.FinishTripAsync(id, request, companyId);
                return Ok(new { success = true, message = "Viagem finalizada com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "finalizar viagem");
            }
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                await _tripAppService.CancelTripAsync(id, companyId);
                return Ok(new { success = true, message = "Viagem cancelada com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "cancelar viagem");
            }
        }

        [HttpPost("{id}/expenses")]
        public async Task<IActionResult> AddExpense(Guid id, [FromBody] CreateTripExpenseRequest request)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                await _tripAppService.AddExpenseAsync(id, request, companyId);
                return Ok(new { success = true, message = "Despesa lançada para a viagem com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "adicionar despesa da viagem");
            }
        }

        [HttpPut("{tripId}/expenses/{expenseId}")]
        public async Task<IActionResult> UpdateExpense(Guid tripId, Guid expenseId, [FromBody] UpdateTripExpenseRequest request)
        {
            try
            {
                if (!TryGetCompanyId(out var companyId))
                    return Unauthorized(new { success = false, message = "CompanyId inválido no token." });

                await _tripAppService.UpdateExpenseAsync(tripId, expenseId, request, companyId);
                return Ok(new { success = true, message = "Despesa atualizada com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(_logger, ex, "atualizar despesa da viagem");
            }
        }

        private bool TryGetCompanyId(out Guid companyId)
        {
            return Guid.TryParse(User.FindFirst("CompanyId")?.Value, out companyId);
        }
    }
}
