using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FuelFlex.CRM.Api.DTOs;

namespace FuelFlex.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/pumps")]
    public class PumpsController : ControllerBase
    {
        private readonly string _connectionString =
            "Server=localhost\\SQLEXPRESS;Database=FuelFlexCRM;Trusted_Connection=True;TrustServerCertificate=True;";

        /// <summary>
        /// Pompa Yakıt Verme Öncesi Limit, Yetki ve Puan Sorgulama (BRD v1.0 - POST /api/v1/pumps/authorization-check)
        /// </summary>
        [HttpPost("authorization-check")]
        [ProducesResponseType(typeof(AuthorizationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthorizationResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthorizationResponseDto), StatusCodes.Status403Forbidden)]
        public IActionResult CheckAuthorization([FromBody] AuthorizationRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.LicensePlate))
            {
                return BadRequest(new AuthorizationResponseDto
                {
                    IsAuthorized = false,
                    ResponseCode = "ERR_INVALID_INPUT",
                    Message = "Geçersiz istek parametreleri."
                });
            }

            string cleanPlate = request.LicensePlate.Trim().ToUpper();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // 1. Vehicles & Companies Sorgusu
                    string query = @"
                        SELECT v.VehicleID, v.LicensePlate, v.AllowedFuelType, v.DailyLimitTL, v.DailyUsedTL, v.IsBlocked,
                               c.CompanyID, c.CompanyName, c.IsActive AS IsCompanyActive
                        FROM Vehicles v
                        INNER JOIN Companies c ON v.CompanyID = c.CompanyID
                        WHERE v.LicensePlate = @Plate";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Plate", cleanPlate);

                        using (var reader = command.ExecuteReader())
                        {
                            // A) PLAKA FİLO TABLOSUNDA YOKSA -> BİREYSEL MÜŞTERİ AKIŞI (US-201)
                            if (!reader.Read())
                            {
                                reader.Close();
                                return ProcessIndividualCustomer(connection, request);
                            }

                            // B) KURUMSAL FİLO AKIŞI (US-101 & US-102)
                            bool isCompanyActive = reader.GetBoolean(reader.GetOrdinal("IsCompanyActive"));
                            bool isBlocked = reader.GetBoolean(reader.GetOrdinal("IsBlocked"));
                            string allowedFuelType = reader.GetString(reader.GetOrdinal("AllowedFuelType"));
                            decimal dailyLimit = reader.GetDecimal(reader.GetOrdinal("DailyLimitTL"));
                            decimal dailyUsed = reader.GetDecimal(reader.GetOrdinal("DailyUsedTL"));
                            string companyName = reader.GetString(reader.GetOrdinal("CompanyName"));

                            // KURAL 1: Şirket Aktiflik Kontrolü
                            if (!isCompanyActive)
                            {
                                return StatusCode(StatusCodes.Status403Forbidden, new AuthorizationResponseDto
                                {
                                    IsAuthorized = false,
                                    ResponseCode = "ERR_COMPANY_BLOCKED",
                                    Message = "Şirket hesabı askıdadır. Müşteri hizmetleri ile görüşün."
                                });
                            }

                            // KURAL 2: Araç Blokaj Kontrolü
                            if (isBlocked)
                            {
                                return StatusCode(StatusCodes.Status400BadRequest, new AuthorizationResponseDto
                                {
                                    IsAuthorized = false,
                                    ResponseCode = "ERR_VEHICLE_BLOCKED",
                                    Message = "Araç kayıp/çalıntı bildirimi sebebiyle kısıtlanmıştır."
                                });
                            }

                            // KURAL 3: Yakıt Tipi Kontrolü (US-102 / AC-102.1)
                            if (allowedFuelType != "ALL" && !allowedFuelType.Equals(request.RequestedFuelType, StringComparison.OrdinalIgnoreCase))
                            {
                                return StatusCode(StatusCodes.Status400BadRequest, new AuthorizationResponseDto
                                {
                                    IsAuthorized = false,
                                    ResponseCode = "ERR_WRONG_FUEL_TYPE",
                                    Message = $"Bu araç sadece '{allowedFuelType}' yakıt alabilir. Talep edilen: '{request.RequestedFuelType}'"
                                });
                            }

                            // KURAL 4: Günlük Limit Kontrolü (US-101 / AC-101.3)
                            decimal remainingLimit = dailyLimit - dailyUsed;
                            if (request.EstimatedAmountTL > remainingLimit)
                            {
                                return StatusCode(StatusCodes.Status400BadRequest, new AuthorizationResponseDto
                                {
                                    IsAuthorized = false,
                                    ResponseCode = "ERR_LIMIT_EXCEEDED",
                                    Message = $"İşlem reddedildi. Aracın günlük kalan limiti {remainingLimit:N2} TL'dir. İstenen tutar: {request.EstimatedAmountTL:N2} TL"
                                });
                            }

                            // ONAY (AUTH_SUCCESS)
                            return Ok(new AuthorizationResponseDto
                            {
                                IsAuthorized = true,
                                ResponseCode = "AUTH_SUCCESS",
                                Message = "Pompa açılabilir. Limit uygun.",
                                Data = new AuthorizationDataDto
                                {
                                    CompanyName = companyName,
                                    DailyRemainingLimitTL = remainingLimit - request.EstimatedAmountTL,
                                    AllowedFuelType = allowedFuelType
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthorizationResponseDto
                {
                    IsAuthorized = false,
                    ResponseCode = "ERR_SYSTEM_ERROR",
                    Message = $"Sistem hatası: {ex.Message}"
                });
            }
        }

        private IActionResult ProcessIndividualCustomer(SqlConnection connection, AuthorizationRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerPhoneNumber))
            {
                return Ok(new AuthorizationResponseDto
                {
                    IsAuthorized = true,
                    ResponseCode = "AUTH_SUCCESS",
                    Message = "Bireysel müşteri satışı onaylandı. (Telefon girilmediği için puan yüklenmedi)."
                });
            }

            string customerQuery = "SELECT CustomerID, FirstName, LastName, TotalPoints FROM Customers WHERE PhoneNumber = @Phone";
            using (var cmd = new SqlCommand(customerQuery, connection))
            {
                cmd.Parameters.AddWithValue("@Phone", request.CustomerPhoneNumber.Trim());
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int customerId = reader.GetInt32(0);
                        string fullName = $"{reader.GetString(1)} {reader.GetString(2)}";
                        decimal currentPoints = reader.GetDecimal(3);
                        reader.Close();

                        // US-201.1: 100 TL = 1 Puan
                        decimal earnedPoints = Math.Floor(request.EstimatedAmountTL / 100m);
                        decimal newPoints = currentPoints + earnedPoints;

                        string updateQuery = "UPDATE Customers SET TotalPoints = @NewPoints WHERE CustomerID = @CustomerId";
                        using (var updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@NewPoints", newPoints);
                            updateCmd.Parameters.AddWithValue("@CustomerId", customerId);
                            updateCmd.ExecuteNonQuery();
                        }

                        return Ok(new AuthorizationResponseDto
                        {
                            IsAuthorized = true,
                            ResponseCode = "AUTH_SUCCESS",
                            Message = $"Bireysel müşteri satışı onaylandı. Müşteri: {fullName}",
                            Data = new AuthorizationDataDto
                            {
                                CompanyName = "Bireysel Müşteri",
                                EarnedPoints = earnedPoints,
                                TotalPoints = newPoints
                            }
                        });
                    }
                }
            }

            return Ok(new AuthorizationResponseDto
            {
                IsAuthorized = true,
                ResponseCode = "AUTH_SUCCESS",
                Message = "Bireysel müşteri satışı onaylandı. Kayıtlı telefon numarası bulunamadı."
            });
        }
    }
}