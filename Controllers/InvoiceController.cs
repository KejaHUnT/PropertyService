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

        // ADMIN/TESTING: BACKFILL AN INVOICE FOR A SPECIFIC PERIOD (e.g. August, before the 28th automation starts)
        [HttpPost("generate-for-period")]
        public async Task<IActionResult> GenerateForPeriod([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                if (month is < 1 or > 12)
                    return BadRequest("Month must be between 1 and 12");
        
                await _invoiceService.GenerateInvoicesForPeriodAsync(month, year);
        
                return Ok(new
                {
                    success = true,
                    message = $"Invoice generation completed for {month}/{year}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running invoice generation for period");
        
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error running invoice generation for period",
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