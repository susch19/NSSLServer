using System;
using System.Net.Mail;

namespace NSSLServer.Plugin.Userhandling.Manager
{
    public class PasswordRecovery
    {
        public class OutlookDotComMail
        {
            string _sender = "";
            string _password = "";
            public OutlookDotComMail(string sender, string password)
            {
                _sender = sender;
                _password = password;
            }

            public void SendMail(string recipient, string subject, string message)
            {
                SmtpClient client = new SmtpClient("smtp-mail.outlook.com")
                {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true
                };
                System.Net.NetworkCredential credentials =
                    new System.Net.NetworkCredential(_sender, _password);
                client.Credentials = credentials;

                try
                {
                    var mail = new MailMessage(_sender.Trim(), recipient.Trim());
                    mail.Subject = subject;
                    mail.Body = message;
                    mail.From = new MailAddress("nssl@outlook.de", "NonSuckingShoppingList");
                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}