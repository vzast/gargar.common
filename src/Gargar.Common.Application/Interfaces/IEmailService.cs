namespace Gargar.Common.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);

    Task SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true);

    Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, bool isHtml = true);

    Task SendEmailWithAttachmentAsync(IEnumerable<string> to, string subject, string body, string attachmentPath, bool isHtml = true);

    Task SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<string> attachmentPaths, bool isHtml = true);

    Task SendEmailWithAttachmentsAsync(IEnumerable<string> to, string subject, string body, IEnumerable<string> attachmentPaths, bool isHtml = true);
}