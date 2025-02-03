using MailKit.Net.Smtp;
using MimeKit;

namespace Cars.Services
{
    public class EmailService
    {
        private readonly string _websiteEmail = "chedli.masmoudi97@gmail.com"; 
        private readonly string _emailPassword = "ogmy chwq ryqn qyid"; 
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587; 

        public void SendEmail(string fromName, string fromEmail, string subject, string messageBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AutoHaven", fromEmail));
            message.To.Add(new MailboxAddress(fromName, "chedli.masmoudi01@gmail.com"));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = $"Name: {fromName}\nEmail: {fromEmail}\n\n{messageBody}"
            };

            using (var client = new SmtpClient())
            {
                client.Connect(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(_websiteEmail, _emailPassword);

                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
