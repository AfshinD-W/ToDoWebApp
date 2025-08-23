using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace SSToDo.Shared
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }


    public class EmailService : IEmailService
    {
        private readonly EmaileSettings _emailSettings;
        public EmailService(IOptions<EmaileSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromAddress = new MailAddress(_emailSettings.FromAddress, _emailSettings.FromName);
            using var smtp = new SmtpClient
            {
                Host = _emailSettings.Host,
                Port = _emailSettings.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(_emailSettings.FromAddress, _emailSettings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = fromAddress,
                Body = BuildEmailTemplate(subject, body),
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtp.SendMailAsync(mailMessage);
        }

        private string BuildEmailTemplate(string title, string content)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
          <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f6f9;
                padding: 20px;
                color: #333;
            }}
            .container {{
                max-width: 600px;
                margin: auto;
                background: #ffffff;
                border-radius: 12px;
                box-shadow: 0 4px 20px rgba(0,0,0,0.1);
                overflow: hidden;
            }}
            .header {{
                background: linear-gradient(90deg, #4f46e5, #3b82f6);
                padding: 20px;
                text-align: center;
                color: white;
                font-size: 20px;
                font-weight: bold;
            }}
            .content {{
                padding: 30px;
                line-height: 1.6;
            }}
            .btn {{
                display: inline-block;
                margin-top: 20px;
                padding: 12px 25px;
                background: #3b82f6;
                color: white;
                text-decoration: none;
                border-radius: 8px;
                font-weight: bold;
            }}
            .footer {{
                padding: 15px;
                text-align: center;
                font-size: 12px;
                color: #777;
                background: #f9fafb;
            }}
          </style>
        </head>
        <body>
          <div class='container'>
            <div class='header'>{title}</div>
            <div class='content'>
                {content}
                <br/>
                <a href='#' class='btn'>Confirm</a>
            </div>
            <div class='footer'>
                &copy; {DateTime.UtcNow.Year} SSToDo. All rights reserved.
            </div>
          </div>
        </body>
        </html>";
        }
    }

    //Emaile config modle
    public class EmaileSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string Password { get; set; }
    }
}
