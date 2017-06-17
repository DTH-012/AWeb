using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace DoAn_Auction.Helpers
{
    public class MailHelper
    {
        public static void SendEmail(string toEmail, string subject, string body)
        {
            string smtpHost = ConfigurationManager.AppSettings["SMTPHost"].ToString();
            string SMTPPort = ConfigurationManager.AppSettings["SMTPPort"].ToString();
            string FromEmail = ConfigurationManager.AppSettings["FromEmail"].ToString();
            string FromEmailDisplayName = ConfigurationManager.AppSettings["FromEmailDisplayName"].ToString();
            string FromEmailPWD = ConfigurationManager.AppSettings["FromEmailPWD"].ToString();


            MailMessage msg = new MailMessage();
            MailAddress from = new MailAddress(FromEmail, FromEmailDisplayName);
            msg.From = from;
            msg.To.Add(toEmail);
            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.Host = smtpHost;
            client.Port = Convert.ToInt32(SMTPPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(FromEmail, FromEmailPWD);
            client.EnableSsl = true;

            client.Send(msg);

        }
    }
}