using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Templates.Core.Domain.SharedServices.Models.Email;
using Templates.Core.Domain.SharedServices.Abstractions.Messaging.Mail;

namespace Templates.Core.Infrastructure.SharedServices.Mail;

public class EmailService : IEmailService
{
	public EmailSettings _emailSettings { get; }
	public ILogger<EmailService> _logger { get; }

	public EmailService(IOptions<EmailSettings> mailSettings, ILogger<EmailService> logger)
	{
		_emailSettings = mailSettings.Value;
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<bool> SendMailAsync(Email email)
	{
		var client = new SendGridClient(_emailSettings.ApiKey);

		var subject = email.Subject;
		var to = new EmailAddress(email.To);
		var emailBody = email.Body;

		var from = new EmailAddress
		{
			Email = _emailSettings.FromAddress,
			Name = _emailSettings.FromName
		};

		var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, emailBody, emailBody);
		var response = await client.SendEmailAsync(sendGridMessage);

		_logger.LogInformation($"Email sent to receiver : {to} with subject: {subject} from sender: {from}");

		if (response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.OK)
			return true;

		_logger.LogWarning($"Email failed to sent to receiver : {to} with subject: {subject} from sender: {from}");

		return false;
	}
}
