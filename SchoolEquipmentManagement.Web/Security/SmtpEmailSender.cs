using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace SchoolEquipmentManagement.Web.Security
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation(
                    "Email sending is disabled. Intended message to {ToAddress}: {Subject}\n{Body}",
                    message.ToAddress,
                    message.Subject,
                    message.PlainTextBody);
                return;
            }

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = message.Subject,
                Body = message.PlainTextBody,
                IsBodyHtml = false
            };

            mailMessage.To.Add(message.ToAddress);

            using var smtpClient = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl = _options.UseSsl,
                Credentials = new NetworkCredential(_options.UserName, _options.Password)
            };

            cancellationToken.ThrowIfCancellationRequested();
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
