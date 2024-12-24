using Templates.Core.Domain.SharedServices.Models.Email;

namespace Templates.Core.Domain.SharedServices.Abstractions.Messaging.Mail;

public interface IEmailService
{
	Task<bool> SendMailAsync(Email email);
}
