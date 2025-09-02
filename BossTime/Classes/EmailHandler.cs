using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace BossTime
{
    public static class EmailHandler
    {

        public static void SendEmail(string to, string subject, string body, string displayname, bool isHtml=true)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient(SystemVariables.EmailSMTP, SystemVariables.EmailPort);

                smtpClient.Credentials = new System.Net.NetworkCredential(SystemVariables.EmailAddress, SystemVariables.EmailPassword);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = SystemVariables.EmailTLS;
                MailMessage mail = new MailMessage();

                
                mail.From = new MailAddress(SystemVariables.EmailAddress, displayname);
                mail.To.Add(new MailAddress(to));
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = isHtml;
                

                
                smtpClient.Send(mail);
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to send email.");
            }
        }

    }
}