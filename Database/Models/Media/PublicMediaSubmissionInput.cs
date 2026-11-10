using System.ComponentModel.DataAnnotations;

namespace FloodOnlineReportingTool.Database.Models.Media;

public record PublicMediaSubmissionInput(Guid Id, string? Name, string? EmailAddress, bool SharingAgreementAccepted)
{
    public const int MaxNameLength = 200;
    public const int MaxEmailLength = 254;
    public const string SharingAgreementText = "You agree that your media can be shared with relevant organisations for the purpose of flood risk management";

    public static string? NormalizeContact(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static bool IsValidEmail(string? value)
    {
        var email = NormalizeContact(value);
        return email is null || new EmailAddressAttribute().IsValid(email);
    }
}
