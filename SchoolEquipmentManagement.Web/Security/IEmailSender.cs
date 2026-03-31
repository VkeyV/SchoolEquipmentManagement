namespace SchoolEquipmentManagement.Web.Security
{
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    }
}
