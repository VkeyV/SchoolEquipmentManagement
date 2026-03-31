namespace SchoolEquipmentManagement.Web.Security
{
    public sealed record EmailMessage(
        string ToAddress,
        string Subject,
        string PlainTextBody);
}
