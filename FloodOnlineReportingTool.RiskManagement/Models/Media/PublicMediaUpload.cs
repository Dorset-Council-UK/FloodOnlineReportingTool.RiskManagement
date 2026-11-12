using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Media;

public class PublicMediaUpload : MediaUpload
{
    public Guid SubmissionId { get; } = Guid.CreateVersion7();

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? Name { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Input)]
    public string? EmailAddress { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Checkbox)]
    public bool SharingAgreementAccepted { get; set; }
}
