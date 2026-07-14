using AutoMapper;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Models.enums;
using KejaHUnt_PropertiesAPI.Models.Enums;    // <-- added for UnitStatus
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            HttpClient httpClient,
            IConfiguration config,
            IUnitPaymentsRepository unitPaymentsRepo,
            IUnitRepository unitRepository,
            IPaymentTransactionRepository transactionRepo,
            IMapper mapper,
            ILogger<PaymentService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _unitPaymentsRepo = unitPaymentsRepo;
            _unitRepository = unitRepository;
            _transactionRepo = transactionRepo;
            _mapper = mapper;
            _logger = logger;
        }

        //  INITIALIZE PAYMENT
        public async Task<InitializePaymentResponse> InitiateManualMpesaAsync(InitiateManualMpesaDto dto)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0.");
        
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
                Gateway = "manual_mpesa",
                AccountId = $"{_config["PaymentService:ClientId"]}-{dto.PropertyId}",
                Amount = dto.Amount,
                Currency = "KES",
                Description = $"Rent {dto.PeriodMonth}/{dto.PeriodYear}",
                WebhookUrl = _config["PaymentService:WebhookUrl"],
                GatewaySecretKey = _config["PaymentService:GatewaySecretKey"]
            };
        
            var json = JsonSerializer.Serialize(request,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
            var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                _config["PaymentService:BaseUrl"] + _config["PaymentService:InitializeEndpoint"]);
            httpRequest.Headers.Add("X-Api-Key", _config["PaymentService:ApiKey"]);
            httpRequest.Headers.Add("X-Client-Id", _config["PaymentService:ClientId"]);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
            var response = await _httpClient.SendAsync(httpRequest);
        
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

        //  INITIALIZE PAYMENT (Paystack/KCB gateway flow)
        public async Task<InitializePaymentResponse> InitializePaymentAsync(CreateUnitPaymentsDto dto)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0.");
        
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
                Gateway = dto.Gateway,
                AccountId = $"{_config["PaymentService:ClientId"]}-{dto.PropertyId}",
                PhoneNumber = dto.PhoneNumber,
                Email = dto.UserEmail,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Description = $"Rent {dto.PeriodMonth}/{dto.PeriodYear}",
                WebhookUrl = _config["PaymentService:WebhookUrl"],
                GatewaySecretKey = _config["PaymentService:GatewaySecretKey"]
            };
        
            var json = JsonSerializer.Serialize(request,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
            var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                _config["PaymentService:BaseUrl"] + _config["PaymentService:InitializeEndpoint"]);
            httpRequest.Headers.Add("X-Api-Key", _config["PaymentService:ApiKey"]);
            httpRequest.Headers.Add("X-Client-Id", _config["PaymentService:ClientId"]);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
            var response = await _httpClient.SendAsync(httpRequest);
        
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

            var gatewayStatus = (PaymentStatus)statusInt;
            var newStatus = MapGatewayStatus(gatewayStatus);
            transaction.Status = newStatus;
            await _transactionRepo.UpdateAsync(transaction);

            if (newStatus != PaymentTransactionStatus.Success)
                return;

            var unitPayment = await _unitPaymentsRepo.GetByIdAsync(transaction.UnitPaymentId);
            if (unitPayment == null) return;

            var transactions = await _transactionRepo
                .GetByUnitPaymentIdAsync(unitPayment.Id);

            unitPayment.PaidAmount = transactions
                .Where(t => t.Status == PaymentTransactionStatus.Success)
                .Sum(t => t.Amount);

            unitPayment.Status = CalculateUnitPaymentStatus(
                unitPayment.PaidAmount,
                unitPayment.ExpectedAmount);

            await _unitPaymentsRepo.UpdateAsync(unitPayment);

            if (unitPayment.Status == UnitPaymentStatus.Paid)
            {
                var unit = await _unitRepository.GetUnitByIdAsync(unitPayment.UnitId);

                // FIXED: compare enum values, not strings
                if (unit != null && (unit.Status == UnitStatus.Reserved || unit.Status == UnitStatus.Available))
                {
                    _logger.LogInformation("Unit {UnitId} is {Status}, closing booking", unitPayment.UnitId, unit.Status);

                    var bookingServiceUrl = _config["BookingService:BaseUrl"];
                    var request = new HttpRequestMessage(
                        HttpMethod.Patch,
                        $"{bookingServiceUrl}/api/booking/unit/{unitPayment.UnitId}/close"
                    );

                    var response = await _httpClient.SendAsync(request);
                    _logger.LogInformation("Booking close response: {StatusCode}", response.StatusCode);
                }
                else
                {
                    _logger.LogInformation("Unit {UnitId} is already Booked, skipping", unitPayment.UnitId);
                }
            }
        }

        public async Task<UnitPaymentsDto> RecordManualPaymentAsync(CreateManualUnitPaymentDto dto)
        {
            if (dto.PaymentType == "mpesa" && string.IsNullOrWhiteSpace(dto.MpesaCode))
                throw new ArgumentException("MpesaCode is required when PaymentType is 'mpesa'.");
        
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
        
            // Property generates the reference — avoids the webhook race condition
            var reference = $"{_config["PaymentService:ClientId"]}_{Guid.NewGuid():N}";
        
            // Save local transaction FIRST, before calling payment API
            var transaction = new PaymentTransaction
            {
                UnitPaymentId = unitPayment.Id,
                Amount = dto.Amount,
                Status = PaymentTransactionStatus.Initialized,
                Reference = reference
            };
            await _transactionRepo.CreateAsync(transaction);
        
            // Now call payment API's manual-payments endpoint
            var manualRequest = new
            {
                reference,
                amount = dto.Amount,
                phoneNumber = dto.PhoneNumber,
                paymentType = dto.PaymentType,
                mpesaCode = dto.MpesaCode,
                description = $"Rent {dto.PeriodMonth}/{dto.PeriodYear}",
                webhookUrl = _config["PaymentService:WebhookUrl"],
                approvedByManagerId = dto.ApprovedByManagerId
            };
        
            var json = JsonSerializer.Serialize(manualRequest,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
            var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                _config["PaymentService:BaseUrl"] + "/api/manual-payments");
            httpRequest.Headers.Add("X-Api-Key", _config["PaymentService:ApiKey"]);
            httpRequest.Headers.Add("X-Client-Id", _config["PaymentService:ClientId"]);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
            var response = await _httpClient.SendAsync(httpRequest);
        
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Manual payment API call failed for {Reference}: {Body}", reference, errorBody);
                throw new Exception($"Payment API error: {errorBody}");
            }
        
            // At this point the webhook has already fired synchronously and updated
            // unitPayment via HandleWebhookAsync — refetch to return current state
            var refreshed = await _unitPaymentsRepo.GetByIdAsync(unitPayment.Id);
            return _mapper.Map<UnitPaymentsDto>(refreshed);
        }


        // TENANT SUBMITS RAW SMS
        public async Task<UnitPaymentsDto?> SubmitTenantMpesaSmsAsync(long unitPaymentId, SubmitTenantSmsDto dto)
        {
            var unitPayment = await _unitPaymentsRepo.GetByIdAsync(unitPaymentId);
            if (unitPayment == null)
                throw new ArgumentException("Unit payment not found.");
        
            var transactions = await _transactionRepo.GetByUnitPaymentIdAsync(unitPaymentId);
            var transaction = transactions
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault(t => t.Status == PaymentTransactionStatus.Initialized);
        
            if (transaction == null || string.IsNullOrEmpty(transaction.Reference))
                throw new ArgumentException("No pending manual payment reference found for this unit payment. Call initiate first.");
        
            var payload = new { rawSms = dto.RawSms, amount = dto.Amount };
            var json = JsonSerializer.Serialize(payload,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
            var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                $"{_config["PaymentService:BaseUrl"]}/api/manual-payments/{transaction.Reference}/tenant-sms");
            httpRequest.Headers.Add("X-Api-Key", _config["PaymentService:ApiKey"]);
            httpRequest.Headers.Add("X-Client-Id", _config["PaymentService:ClientId"]);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
            var response = await _httpClient.SendAsync(httpRequest);
        
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Tenant SMS submission failed for {Reference}: {Body}", transaction.Reference, errorBody);
                throw new Exception($"Payment API error: {errorBody}");
            }
        
            return _mapper.Map<UnitPaymentsDto>(unitPayment);
        }
        
        // MANAGER: GET PENDING MANUAL PAYMENTS FOR A PROPERTY
        public async Task<List<PendingUnitPaymentDto>> GetPendingManualPaymentsAsync(long propertyId)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get,
                $"{_config["PaymentService:BaseUrl"]}/api/manual-payments/pending");
            httpRequest.Headers.Add("X-Api-Key", _config["PaymentService:ApiKey"]);
            httpRequest.Headers.Add("X-Client-Id", _config["PaymentService:ClientId"]);
        
            var response = await _httpClient.SendAsync(httpRequest);
        
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Fetching pending manual payments failed: {Body}", errorBody);
                throw new Exception($"Payment API error: {errorBody}");
            }
        
            var pendingFromPaymentApi = JsonSerializer.Deserialize<List<PaymentApiPendingDto>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PaymentApiPendingDto>();
        
            var result = new List<PendingUnitPaymentDto>();
        
            foreach (var item in pendingFromPaymentApi)
            {
                var transaction = await _transactionRepo.GetByReferenceAsync(item.Reference);
                if (transaction == null) continue;
        
                var unitPayment = await _unitPaymentsRepo.GetByIdAsync(transaction.UnitPaymentId);
                if (unitPayment == null || unitPayment.PropertyId != propertyId) continue;
        
                result.Add(new PendingUnitPaymentDto
                {
                    ManualPaymentId = item.Id,
                    UnitPaymentId = unitPayment.Id,
                    UnitId = unitPayment.UnitId,
                    TenantId = unitPayment.TenantId,
                    PropertyId = unitPayment.PropertyId,
                    Reference = item.Reference,
                    TenantAmount = item.TenantAmount,
                    TenantRawSms = item.TenantRawSms,
                    TenantSubmittedAt = item.TenantSubmittedAt
                });
            }
        
            return result;
        }
        
        // MANAGER: APPROVE MANUAL PAYMENT
        public async Task<UnitPaymentsDto?> ApproveManualPaymentAsync(long unitPaymentId, ApproveUnitPaymentDto dto)
        {
            var unitPayment = await _unitPaymentsRepo.GetByIdAsync(unitPaymentId);
            if (unitPayment == null)
                throw new ArgumentException("Unit payment not found.");
        
            var transactions = await _transactionRepo.GetByUnitPaymentIdAsync(unitPaymentId);
            var transaction = transactions
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault(t => t.Status == PaymentTransactionStatus.Initialized);
        
            if (transaction == null || string.IsNullOrEmpty(transaction.Reference))
                throw new ArgumentException("No pending manual payment found for this unit payment.");
        
            // Fetch the payment API's ManualRentPayment.Id via its GET-by-reference or pending list
            var pendingHttpRequest = new HttpRequestMessage(HttpMethod.Get,
                $"{_config["PaymentService:BaseUrl"]}/api/manual-payments/pending");
            pendingHttpRequest.Headers.Add("X-Api-Key", _config["PaymentService:ApiKey"]);
            pendingHttpRequest.Headers.Add("X-Client-Id", _config["PaymentService:ClientId"]);
        
            var pendingResponse = await _httpClient.SendAsync(pendingHttpRequest);
            if (!pendingResponse.IsSuccessStatusCode)
                throw new Exception(await pendingResponse.Content.ReadAsStringAsync());
        
            var pendingList = JsonSerializer.Deserialize<List<PaymentApiPendingDto>>(
                await pendingResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PaymentApiPendingDto>();
        
            var match = pendingList.FirstOrDefault(p => p.Reference == transaction.Reference);
            if (match == null)
                throw new ArgumentException("This payment is no longer pending manager review.");
        
            var approvePayload = new
            {
                approvedByManagerId = dto.ApprovedByManagerId,
                mpesaCode = dto.MpesaCode,
                amount = dto.Amount
            };
            var json = JsonSerializer.Serialize(approvePayload,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
            var approveHttpRequest = new HttpRequestMessage(HttpMethod.Post,
                $"{_config["PaymentService:BaseUrl"]}/api/manual-payments/{match.Id}/approve");
            approveHttpRequest.Headers.Add("X-Api-Key", _config["PaymentService:ApiKey"]);
            approveHttpRequest.Headers.Add("X-Client-Id", _config["PaymentService:ClientId"]);
            approveHttpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
            var approveResponse = await _httpClient.SendAsync(approveHttpRequest);
        
            if (!approveResponse.IsSuccessStatusCode)
            {
                var errorBody = await approveResponse.Content.ReadAsStringAsync();
                _logger.LogError("Approve manual payment failed for {Reference}: {Body}", transaction.Reference, errorBody);
                throw new Exception($"Payment API error: {errorBody}");
            }
        
            // Approval triggers payment API's webhook synchronously → HandleWebhookAsync already updates UnitPayments
            var refreshed = await _unitPaymentsRepo.GetByIdAsync(unitPaymentId);
            return _mapper.Map<UnitPaymentsDto>(refreshed);
        }


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