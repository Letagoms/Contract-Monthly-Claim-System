using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.Services
{
    public class ClaimValidationService
    {
        private readonly ILogger<ClaimValidationService> _logger;

        // Validation criteria
        private const decimal MAX_HOURLY_RATE = 625m;
        private const decimal MAX_HOURS_WORKED = 160m;
        private const decimal AUTO_APPROVE_THRESHOLD = 5000m; // Auto-approve claims under R5000

        public ClaimValidationService(ILogger<ClaimValidationService> logger)
        {
            _logger = logger;
        }

        public ClaimValidationResult ValidateClaim(Claim claim)
        {
            var result = new ClaimValidationResult
            {
                IsValid = true,
                ValidationMessages = new List<string>()
            };

            // Check hourly rate
            if (claim.HourlyRate > MAX_HOURLY_RATE)
            {
                result.IsValid = false;
                result.ValidationMessages.Add($"Hourly rate (R{claim.HourlyRate}) exceeds maximum allowed (R{MAX_HOURLY_RATE})");
                result.SuggestedStatus = "Rejected";
            }

            // Check hours worked
            if (claim.HoursWorked > MAX_HOURS_WORKED)
            {
                result.IsValid = false;
                result.ValidationMessages.Add($"Hours worked ({claim.HoursWorked}) exceeds maximum allowed ({MAX_HOURS_WORKED})");
                result.SuggestedStatus = "Rejected";
            }

            // Check for negative values
            if (claim.HourlyRate <= 0 || claim.HoursWorked <= 0)
            {
                result.IsValid = false;
                result.ValidationMessages.Add("Hours worked and hourly rate must be greater than zero");
                result.SuggestedStatus = "Rejected";
            }

            // Check total amount calculation
            var expectedTotal = claim.HoursWorked * claim.HourlyRate;
            if (Math.Abs(claim.TotalAmount - expectedTotal) > 0.01m)
            {
                result.IsValid = false;
                result.ValidationMessages.Add("Total amount calculation mismatch");
                result.SuggestedStatus = "Rejected";
            }

            // Auto-approval logic for valid claims
            if (result.IsValid)
            {
                if (claim.TotalAmount <= AUTO_APPROVE_THRESHOLD)
                {
                    result.SuggestedStatus = "Approved";
                    result.ValidationMessages.Add($"Auto-approved: Claim amount (R{claim.TotalAmount:N2}) is within auto-approval threshold");
                }
                else
                {
                    result.SuggestedStatus = "Pending";
                    result.ValidationMessages.Add($"Requires manual review: Claim amount (R{claim.TotalAmount:N2}) exceeds auto-approval threshold (R{AUTO_APPROVE_THRESHOLD:N2})");
                }
            }

            _logger.LogInformation($"Claim ID {claim.Id} validated: {result.SuggestedStatus}");
            return result;
        }
    }

    public class ClaimValidationResult
    {
        public bool IsValid { get; set; }
        public string SuggestedStatus { get; set; } = "Pending";
        public List<string> ValidationMessages { get; set; } = new();
    }
}