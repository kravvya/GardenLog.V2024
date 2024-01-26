namespace UserManagement.Contract.Command;


public record SendEmailCommand: EmailMessageBase
{
}


public class SendEmailCommandValidator : EmailMessageValidator<EmailMessageBase>
{
    public SendEmailCommandValidator()
    {
    }
}
