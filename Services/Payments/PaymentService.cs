using AutoMapper;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Models.enums;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using System.Text;
using System.Text.Json;

namespace KejaHUnt_PropertiesAPI.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IUnitPaymentsRepository _unitPaymentsRepo;
        private readonly IUnitRepository _unitRepository;
        private readonly IPaymentTransactionRepository _transactionRepo;
        private readonly IMapper _mapper;

        public PaymentService(
            HttpClient httpClient,
            IConfiguration config,
            IUnitPaymentsRepository unitPaymentsRepo,
            IUnitRepository unitRepository,
            IPaymentTransactionRepository transactionRepo,
            IMapper mapper)
        {
            _httpClient = httpClient;
            _config = config;
            _unitPaymentsRepo = unitPaymentsRepo;
            _unitRepository = unitRepository;
            _transactionRepo = transactionRepo;
            _mapper = mapper;
        }

        //  INITIALIZE PAYMENT
        public async Task<InitializePaymentResponse> InitializePaymentAsync(CreateUnitPaymentsDto dto)
        {
            var unitPayment = await _unitPaymentsRepo
                .GetByUnitAndPeriodAsync(dto.UnitId, dto.PeriodMonth, dto.PeriodYear);

            var unit = await _unitRepository.GetUnitByIdAsync(dto.UnitId);

            if (unitPayment == null)
            {
                unitPayment = new UnitPayments
                {
                    UnitId = dto.UnitId,
                    PropertyId = dto.PropertyId,
                    TenantId = dto.TenantId,
                    PeriodMonth = dto.PeriodMonth,
                    PeriodYear = dto.PeriodYear,
                    ExpectedAmount = unit.Price,
                    PaidAmount = 0,
                    Status = UnitPaymentStatus.Pending
                };

                await _unitPaymentsRepo.CreateAsync(unitPayment);
            }

            var request = new InitializePaymentRequest
            {
                Gateway = dto.Gateway ?? _config["PaymentService:Gateway"],
                AccountId = dto.AccountId ?? _config["PaymentService:AccountId"],
                PhoneNumber = dto.PhoneNumber,
                Email = dto.UserEmail,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Description = $"Rent {dto.PeriodMonth}/{dto.PeriodYear}",
                CallbackUrl = _config["PaymentService:CallbackUrl"],
                GatewaySecretKey = _config["PaymentService:GatewaySecretKey"]
            };

            var json = JsonSerializer.Serialize(request,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var response = await _httpClient.PostAsync(
                _config["PaymentService:BaseUrl"] + _config["PaymentService:InitializeEndpoint"],
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var paymentResponse = JsonSerializer.Deserialize<InitializePaymentResponse>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (paymentResponse == null)
                throw new Exception("Invalid payment response");

            var transaction = new PaymentTransaction
            {
                UnitPaymentId = unitPayment.Id,
                ExternalPaymentId = paymentResponse.Id,
                Amount = dto.Amount,
                Status = PaymentTransactionStatus.Initialized,
                Reference = paymentResponse.Reference
            };

            await _transactionRepo.CreateAsync(transaction);

            return paymentResponse;
        }

        // GET ALL
        public async Task<List<UnitPaymentsDto>> GetAllAsync()
        {
            var data = await _unitPaymentsRepo.GetAllAsync();
            return _mapper.Map<List<UnitPaymentsDto>>(data);
        }

        // GET BY ID
        public async Task<UnitPaymentsDto?> GetByIdAsync(long id)
        {
            var data = await _unitPaymentsRepo.GetByIdAsync(id);
            return data == null ? null : _mapper.Map<UnitPaymentsDto>(data);
        }

        // GET BY TENANT
        public async Task<List<UnitPaymentsDto>> GetByTenantIdAsync(long tenantId)
        {
            var data = await _unitPaymentsRepo.GetByTenantIdAsync(tenantId);
            return _mapper.Map<List<UnitPaymentsDto>>(data);
        }

        // GET BY UNIT
        public async Task<List<UnitPaymentsDto>> GetByUnitIdAsync(long unitId)
        {
            var data = await _unitPaymentsRepo.GetByUnitIdAsync(unitId);
            return _mapper.Map<List<UnitPaymentsDto>>(data);
        }

        // UPDATE
        public async Task<UnitPaymentsDto?> UpdateAsync(long id, UpdateUnitPaymentsDto dto)
        {
            var existing = await _unitPaymentsRepo.GetByIdAsync(id);
            if (existing == null) return null;

            existing.ExpectedAmount = dto.ExpectedAmount;

            var updated = await _unitPaymentsRepo.UpdateAsync(existing);

            return _mapper.Map<UnitPaymentsDto>(updated);
        }

        // DELETE
        public async Task<UnitPaymentsDto?> DeleteAsync(long id)
        {
            var deleted = await _unitPaymentsRepo.DeleteAsync(id);
            return deleted == null ? null : _mapper.Map<UnitPaymentsDto>(deleted);
        }

        //  WEBHOOK HANDLER (CRITICAL)
        public async Task HandleWebhookAsync(string reference, int statusInt)
        {
            var transaction = await _transactionRepo.GetByReferenceAsync(reference);
            if (transaction == null)
                throw new Exception("Transaction not found");

            // ✅ Convert gateway status
            var gatewayStatus = (PaymentStatus)statusInt;

            // ✅ Map to internal enum
            var newStatus = MapGatewayStatus(gatewayStatus);

            // ✅ Update transaction safely
            transaction.Status = newStatus;
            await _transactionRepo.UpdateAsync(transaction);

            // ❌ Only process successful payments
            if (newStatus != PaymentTransactionStatus.Success)
                return;

            var unitPayment = await _unitPaymentsRepo.GetByIdAsync(transaction.UnitPaymentId);
            if (unitPayment == null) return;

            // 🔥 Recalculate total from ALL successful transactions (NOT +=)
            var transactions = await _transactionRepo
                .GetByUnitPaymentIdAsync(unitPayment.Id);

            unitPayment.PaidAmount = transactions
                .Where(t => t.Status == PaymentTransactionStatus.Success)
                .Sum(t => t.Amount);

            // ✅ Derive status (NEVER assign blindly)
            unitPayment.Status = CalculateUnitPaymentStatus(
                unitPayment.PaidAmount,
                unitPayment.ExpectedAmount);

            await _unitPaymentsRepo.UpdateAsync(unitPayment);
        }

        // HELPER: Map gateway status to internal status
        private PaymentTransactionStatus MapGatewayStatus(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => PaymentTransactionStatus.Pending,
                PaymentStatus.Success => PaymentTransactionStatus.Success,
                PaymentStatus.Failed => PaymentTransactionStatus.Failed,
                _ => PaymentTransactionStatus.Failed
            };
        }

        // HELPER: Calculate UnitPaymentStatus based on amounts
        private UnitPaymentStatus CalculateUnitPaymentStatus(decimal paid, decimal expected)
        {
            if (paid == 0)
                return UnitPaymentStatus.Pending;

            if (paid < expected)
                return UnitPaymentStatus.Partial;

            if (paid == expected)
                return UnitPaymentStatus.Paid;

            if (paid > expected)
                return UnitPaymentStatus.Overpaid;

            return UnitPaymentStatus.Pending;
        }
    }
}