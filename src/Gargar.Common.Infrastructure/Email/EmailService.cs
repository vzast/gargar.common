using Gargar.Common.Application.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Gargar.Common.Infrastructure.Email;

/// <summary>
/// Service for sending emails using SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class
    /// </summary>
    /// <param name="options">Email configuration options</param>
    /// <exception cref="ArgumentNullException">Thrown if options is null</exception>
    public EmailService(EmailOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Sends an email to a single recipient
    /// </summary>
    /// <param name="to">Email address of the recipient</param>
    /// <param name="subject">Subject of the email</param>
    /// <param name="body">Content of the email</param>
    /// <param name="isHtml">Specifies whether the body contains HTML</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        await SendEmailAsync([to], subject, body, isHtml);
    }

    /// <summary>
    /// Sends an email to multiple recipients
    /// </summary>
    /// <param name="to">Collection of recipient email addresses</param>
    /// <param name="subject">Subject of the email</param>
    /// <param name="body">Content of the email</param>
    /// <param name="isHtml">Specifies whether the body contains HTML</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true)
    {
        using var message = CreateMailMessage(to, subject, body, isHtml);
        await SendMailAsync(message);
    }

    /// <summary>
    /// Sends an email with a single attachment to a single recipient
    /// </summary>
    /// <param name="to">Email address of the recipient</param>
    /// <param name="subject">Subject of the email</param>
    /// <param name="body">Content of the email</param>
    /// <param name="attachmentPath">Path to the attachment file</param>
    /// <param name="isHtml">Specifies whether the body contains HTML</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, bool isHtml = true)
    {
        await SendEmailWithAttachmentAsync([to], subject, body, attachmentPath, isHtml);
    }

    /// <summary>
    /// Sends an email with a single attachment to multiple recipients
    /// </summary>
    /// <param name="to">Collection of recipient email addresses</param>
    /// <param name="subject">Subject of the email</param>
    /// <param name="body">Content of the email</param>
    /// <param name="attachmentPath">Path to the attachment file</param>
    /// <param name="isHtml">Specifies whether the body contains HTML</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SendEmailWithAttachmentAsync(IEnumerable<string> to, string subject, string body, string attachmentPath, bool isHtml = true)
    {
        await SendEmailWithAttachmentsAsync(to, subject, body, [attachmentPath], isHtml);
    }

    /// <summary>
    /// Sends an email with multiple attachments to a single recipient
    /// </summary>
    /// <param name="to">Email address of the recipient</param>
    /// <param name="subject">Subject of the email</param>
    /// <param name="body">Content of the email</param>
    /// <param name="attachmentPaths">Collection of paths to attachment files</param>
    /// <param name="isHtml">Specifies whether the body contains HTML</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<string> attachmentPaths, bool isHtml = true)
    {
        await SendEmailWithAttachmentsAsync([to], subject, body, attachmentPaths, isHtml);
    }

    /// <summary>
    /// Sends an email with multiple attachments to multiple recipients
    /// </summary>
    /// <param name="to">Collection of recipient email addresses</param>
    /// <param name="subject">Subject of the email</param>
    /// <param name="body">Content of the email</param>
    /// <param name="attachmentPaths">Collection of paths to attachment files</param>
    /// <param name="isHtml">Specifies whether the body contains HTML</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SendEmailWithAttachmentsAsync(IEnumerable<string> to, string subject, string body, IEnumerable<string> attachmentPaths, bool isHtml = true)
    {
        using var message = CreateMailMessage(to, subject, body, isHtml);

        foreach (var attachmentPath in attachmentPaths)
        {
            if (string.IsNullOrEmpty(attachmentPath) || !File.Exists(attachmentPath))
                continue;

            message.Attachments.Add(new Attachment(attachmentPath));
        }

        await SendMailAsync(message);
    }

    /// <summary>
    /// Creates a MailMessage with the specified parameters
    /// </summary>
    private MailMessage CreateMailMessage(IEnumerable<string> to, string subject, string body, bool isHtml)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_options.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        foreach (var recipient in to.Where(email => !string.IsNullOrWhiteSpace(email)))
        {
            message.To.Add(recipient);
        }

        return message;
    }

    /// <summary>
    /// Sends an email using SmtpClient
    /// </summary>
    private async Task SendMailAsync(MailMessage message)
    {
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        await client.SendMailAsync(message);
    }
}