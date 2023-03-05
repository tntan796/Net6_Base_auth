using DNBase.ViewModel;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace DNBase.Services
{
    public interface IEmailService
    {
        void Send(string to, string subject, string html, string from = null);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<SmtpConfig> _smtpConfig;

        public EmailService(IConfiguration configuration, IOptions<SmtpConfig> smtpConfig)
        {
            _configuration = configuration;
            _smtpConfig = smtpConfig;
        }

        public void Send(string to, string subject, string html, string from = null)
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from ?? _smtpConfig.Value.EmailFrom));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = html };

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(_smtpConfig.Value.SmtpHost, _smtpConfig.Value.SmtpPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(_smtpConfig.Value.SmtpUser, _smtpConfig.Value.SmtpPass);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        //private string SendEmail()
        //{
        //    string resultSendMail = string.Empty;
        //    try
        //    {
        //        MailMessage ms = new MailMessage();
        //        ms.From = new MailAddress("From", "FromName");
        //        ms.To.Add("To");
        //        //if (!String.IsNullOrEmpty(model.CCEmail) && model.CCEmail.Split(";").Length > 0)
        //        //{
        //        //    foreach (string email in model.CCEmail.Split(";"))
        //        //    {
        //        //        if (!string.IsNullOrEmpty(email)) ms.CC.Add(email);
        //        //    }
        //        //}
        //        ms.BodyEncoding = Encoding.UTF8;
        //        ms.IsBodyHtml = true;
        //        //ms.Subject = model.TitleFeedback;
        //        //ms.Body = model.ContentFeedback;
        //        System.Net.Mail.SmtpClient SendMail = new System.Net.Mail.SmtpClient();
        //        NetworkCredential Authentication = new NetworkCredential();
        //        SendMail.UseDefaultCredentials = false;
        //        SendMail.EnableSsl = true;
        //        //Authentication.UserName = _globalSettings.Smtp.From;
        //        //Authentication.Password = _globalSettings.Smtp.Password;
        //        SendMail.Credentials = Authentication;
        //        //SendMail.Host = _globalSettings.Smtp.Host;
        //        //SendMail.Port = _globalSettings.Smtp.Port;
        //        SendMail.Timeout = 50000;
        //        SendMail.Send(ms);
        //        resultSendMail = "success";
        //    }
        //    catch (Exception ex)
        //    {
        //        resultSendMail = ex.Message.ToString();
        //    }
        //    return resultSendMail;

        //}
    }
}