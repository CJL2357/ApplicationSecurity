using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;

public class EmailSender
{
    private readonly IConfiguration _configuration;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmailSender(IConfiguration configuration, IUrlHelperFactory urlHelperFactory, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _urlHelperFactory = urlHelperFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SendPasswordResetEmailAsync(string email, string token)
    {
        // Generate the reset link
        var httpContext = _httpContextAccessor.HttpContext;
        var urlHelper = _urlHelperFactory.GetUrlHelper(new ActionContext(httpContext, httpContext.GetRouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));
        var resetLink = urlHelper.Page("/ResetPassword", null, new { email = email, token = token }, httpContext.Request.Scheme);

        // Create the subject and body for the email
        string subject = "Password Reset Request";
        string body = $"<p>Please reset your password by clicking here: <a href='{resetLink}'>Reset your password</a></p>";

        // Send the email
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpSettings = _configuration.GetSection("Smtp");
        var smtpClient = new SmtpClient
        {
            Host = smtpSettings["Host"],
            Port = int.Parse(smtpSettings["Port"]),
            Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
            EnableSsl = bool.Parse(smtpSettings["EnableSsl"])
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpSettings["Username"]),
            Subject = subject,
            Body = message,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }
}