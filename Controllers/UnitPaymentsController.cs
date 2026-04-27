using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Services.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KejaHUnt_PropertiesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitPaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<UnitPaymentsController> _logger;

        public UnitPaymentsController(
            IPaymentService paymentService,
            ILogger<UnitPaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // INITIALIZE PAYMENT
        [HttpPost("initialize")]
        public async Task<IActionResult> InitializePayment([FromBody] CreateUnitPaymentsDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request data");

                if (dto.Amount <= 0)
                    return BadRequest("Amount must be greater than 0");

                if (string.IsNullOrWhiteSpace(dto.UserEmail))
                    return BadRequest("Email is required");

                if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    return BadRequest("Phone number is required");

                var response = await _paymentService.InitializePaymentAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = "Payment initialized successfully",
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing payment");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error initializing payment",
                    error = ex.Message
                });
            }
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _paymentService.GetAllAsync();

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET BY ID
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _paymentService.GetByIdAsync(id);

            if (result == null)
                return NotFound($"Unit payment with ID {id} not found");

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET BY TENANT
        [HttpGet("tenant/{tenantId:long}")]
        public async Task<IActionResult> GetByTenant(long tenantId)
        {
            var result = await _paymentService.GetByTenantIdAsync(tenantId);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET BY UNIT
        [HttpGet("unit/{unitId:long}")]
        public async Task<IActionResult> GetByUnit(long unitId)
        {
            var result = await _paymentService.GetByUnitIdAsync(unitId);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // UPDATE
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUnitPaymentsDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request data");

                var result = await _paymentService.UpdateAsync(id, dto);

                if (result == null)
                    return NotFound($"Unit payment with ID {id} not found");

                return Ok(new
                {
                    success = true,
                    message = "Unit payment updated successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unit payment");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating unit payment",
                    error = ex.Message
                });
            }
        }

        // DELETE
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var result = await _paymentService.DeleteAsync(id);

                if (result == null)
                    return NotFound($"Unit payment with ID {id} not found");

                return Ok(new
                {
                    success = true,
                    message = "Unit payment deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting unit payment");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error deleting unit payment",
                    error = ex.Message
                });
            }
        }

        //  WEBHOOK (PAYMENT CALLBACK)
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook(
            [FromQuery] string reference,
            [FromQuery] int status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reference))
                    return BadRequest("Reference is required");

                await _paymentService.HandleWebhookAsync(reference, status);

                return Ok(new
                {
                    success = true,
                    message = "Webhook processed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error processing webhook",
                    error = ex.Message
                });
            }
        }
    }
}
