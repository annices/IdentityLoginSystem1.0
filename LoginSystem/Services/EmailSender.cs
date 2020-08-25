using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Net;

namespace LoginSystem.Services
{
    /// <summary>
    /// This class utilizes ASP.NET Identity's IEmailSender interface to enable you to send emails based on SMTP.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        // Our private configuration variables:
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSSL;
        private readonly string _userName;
        private readonly string _password;

        /// <summary>
        /// This constructor injects the email settings specified in appsettings.json to be used once this class is called.
        /// <param name="host">E.g. Gmail.</param>
        /// <param name="port">SMTP port.</param>
        /// <param name="enableSSL">Default set to true.</param>
        /// <param name="userName">E.g. a Gmail address.</param>
        /// <param name="password">E.g. the password to the above Gmail account.</param>
        public EmailSender(string host, int port, bool enableSSL, string userName, string password)
        {
            _host = host;
            _port = port;
            _enableSSL = enableSSL;
            _userName = userName;
            _password = password;
        }

        /// <summary>
        /// This function sends an email based on a specified email address, subject and message.
        /// </summary>
        /// <param name="email">The email the message will be sent to.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlMessage">The email message.</param>
        /// <returns>The email to be sent.</returns>
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_userName, _password),
                EnableSsl = _enableSSL
            };
            return client.SendMailAsync(
                new MailMessage(_userName, email, subject, htmlMessage) { IsBodyHtml = true }
            );
        }

    } // End class.
} // End namespace.
