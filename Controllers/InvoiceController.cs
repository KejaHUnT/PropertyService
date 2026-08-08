using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Services.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace KejaHUnt_PropertiesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(IInvoiceService invoiceService, ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        // MANAGER: UPDATE WATER BILL FOR A UNIT/PERIOD
        [HttpPut("water-bill")]
        public async Task<IActionResult> UpdateWaterBill([FromBody] UpdateWaterBillDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request data");

                if (dto.WaterBillAmount < 0)
                    return BadRequest("Water bill amount cannot be negative");

                var result = await _invoiceService.UpdateWaterBillAsync(
                    dto.UnitId, dto.PeriodMonth, dto.PeriodYear, dto.WaterBillAmount);

                return Ok(new
                {
                    success = true,
                    message = "Water bill updated on invoice",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating water bill on invoice");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating water bill",
                    error = ex.Message
                });
            }
        }

        // ADMIN/TESTING: MANUALLY TRIGGER THE MONTHLY GENERATION JOB
        [HttpPost("generate-monthly")]
        public async Task<IActionResult> GenerateMonthly()
        {
            try
            {
                await _invoiceService.GenerateMonthlyInvoicesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Monthly invoice generation completed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running monthly invoice generation");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error running monthly invoice generation",
                    error = ex.Message
                });
            }
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _invoiceService.GetAllAsync();

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
            var result = await _invoiceService.GetByIdAsync(id);

            if (result == null)
                return NotFound($"Invoice with ID {id} not found");

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // GET BY PROPERTY
        [HttpGet("property/{propertyId:long}")]
        public async Task<IActionResult> GetByProperty(long propertyId)
        {
            var result = await _invoiceService.GetByPropertyIdAsync(propertyId);

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
            var result = await _invoiceService.GetByUnitIdAsync(unitId);

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
            var result = await _invoiceService.GetByTenantIdAsync(tenantId);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
    }
}