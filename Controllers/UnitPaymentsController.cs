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

        // RECORD MANUAL PAYMENT (CASH OR MPESA)
        [HttpPost("manual")]
        public async Task<IActionResult> RecordManualPayment([FromBody] CreateManualUnitPaymentDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request data");
        
                if (dto.Amount <= 0)
                    return BadRequest("Amount must be greater than 0");
        
                if (dto.PaymentType == "mpesa" && string.IsNullOrWhiteSpace(dto.MpesaCode))
                    return BadRequest("MpesaCode is required when PaymentType is 'mpesa'");
        
                if (string.IsNullOrWhiteSpace(dto.ApprovedByManagerId))
                    return BadRequest("ApprovedByManagerId is required");
        
                var response = await _paymentService.RecordManualPaymentAsync(dto);
        
                return Ok(new
                {
                    success = true,
                    message = "Manual payment recorded successfully",
                    data = response
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording manual payment");
        
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error recording manual payment",
                    error = ex.Message
                });
            }
        }        

        // TENANT: "ALREADY PAID" — INITIATE MANUAL MPESA SUBMISSION
        [HttpPost("manual/initiate")]
        public async Task<IActionResult> InitiateManualMpesa([FromBody] InitiateManualMpesaDto dto)
                {
                    try
                    {
                        if (dto == null)
                            return BadRequest("Invalid request data");
                
                        if (dto.Amount <= 0)
                            return BadRequest("Amount must be greater than 0");
                
                        var response = await _paymentService.InitiateManualMpesaAsync(dto);
                
                        return Ok(new
                        {
                            success = true,
                            message = "Manual payment initiated. Reference generated for SMS submission.",
                            data = response
                        });
                    }
                    catch (ArgumentException ex)
                    {
                        return BadRequest(new { success = false, message = ex.Message });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error initiating manual mpesa payment");
                
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Error initiating manual mpesa payment",
                            error = ex.Message
                        });
                    }
                }
        
        // TENANT: SUBMIT RAW MPESA SMS
        [HttpPost("{unitPaymentId:long}/tenant-sms")]
        public async Task<IActionResult> SubmitTenantSms(long unitPaymentId, [FromBody] SubmitTenantSmsDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request data");
        
                if (string.IsNullOrWhiteSpace(dto.RawSms))
                    return BadRequest("RawSms is required");
        
                if (dto.Amount <= 0)
                    return BadRequest("Amount must be greater than 0");
        
                var response = await _paymentService.SubmitTenantMpesaSmsAsync(unitPaymentId, dto);
        
                return Ok(new
                {
                    success = true,
                    message = "SMS submitted. Awaiting manager review.",
                    data = response
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting tenant SMS");
        
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error submitting tenant SMS",
                    error = ex.Message
                });
            }
        }
        
        // MANAGER: GET PENDING MANUAL PAYMENTS FOR A PROPERTY
        [HttpGet("property/{propertyId:long}/pending-manual")]
        public async Task<IActionResult> GetPendingManual(long propertyId)
        {
            try
            {
                var result = await _paymentService.GetPendingManualPaymentsAsync(propertyId);
        
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending manual payments");
        
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error fetching pending manual payments",
                    error = ex.Message
                });
            }
        }
        
        // MANAGER: APPROVE MANUAL PAYMENT
        [HttpPost("{unitPaymentId:long}/approve-manual")]
        public async Task<IActionResult> ApproveManual(long unitPaymentId, [FromBody] ApproveUnitPaymentDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request data");
        
                if (string.IsNullOrWhiteSpace(dto.MpesaCode))
                    return BadRequest("MpesaCode is required");
        
                if (string.IsNullOrWhiteSpace(dto.ApprovedByManagerId))
                    return BadRequest("ApprovedByManagerId is required");
        
                var response = await _paymentService.ApproveManualPaymentAsync(unitPaymentId, dto);
        
                return Ok(new
                {
                    success = true,
                    message = "Manual payment approved successfully",
                    data = response
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving manual payment");
        
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error approving manual payment",
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
