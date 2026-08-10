using System.ComponentModel.DataAnnotations;

namespace FuelFlex.CRM.Api.DTOs
{
    // İstek Gövdesi (BRD v1.0 / AuthorizationRequest)
    public class AuthorizationRequestDto
    {
        [Required]
        public int StationId { get; set; }

        [Required]
        public int PumpNumber { get; set; }

        [Required]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        public string RequestedFuelType { get; set; } = string.Empty;

        [Required]
        public decimal EstimatedAmountTL { get; set; }

        public string? CustomerPhoneNumber { get; set; }
    }

    // Yanıt Gövdesi (BRD v1.0 / AuthorizationResponse)
    public class AuthorizationResponseDto
    {
        public bool IsAuthorized { get; set; }
        public string ResponseCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public AuthorizationDataDto? Data { get; set; }
    }

    public class AuthorizationDataDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public decimal DailyRemainingLimitTL { get; set; }
        public string AllowedFuelType { get; set; } = string.Empty;
        public decimal EarnedPoints { get; set; }
        public decimal TotalPoints { get; set; }
    }
}