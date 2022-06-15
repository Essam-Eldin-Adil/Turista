using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class EmailService
    {
        public static bool SendMail(object receivers, object copyTo, string body, string subject, object attachments)
        {
            try
            {
                var emailExchangeInfo = GetEmailExchangeInfo();
                ExchangeService service = CreateConnection(emailExchangeInfo, emailExchangeInfo.LoginName, emailExchangeInfo.Password);
                EmailMessage message = new EmailMessage(service);
                if (receivers is IList || receivers.GetType().IsGenericType)
                {
                    foreach (string to in (List<string>)receivers)
                    {
                        message.ToRecipients.Add(new EmailAddress(to));
                    }
                }
                else
                {
                    message.ToRecipients.Add(new EmailAddress(receivers.ToString()));
                }
                if (copyTo is IList || copyTo.GetType().IsGenericType)
                {
                    foreach (string copy in (List<string>)copyTo)
                    {
                        message.CcRecipients.Add(copy);
                    }
                }
                else
                {
                    message.CcRecipients.Add(copyTo.ToString());
                }

                if (attachments is IList || attachments.GetType().IsGenericType)
                {
                    foreach (string attachment in (List<string>)attachments)
                    {
                        message.Attachments.AddFileAttachment(attachment);
                    }
                }
                else
                {
                    message.Attachments.AddFileAttachment(attachments.ToString());
                }
                message.Subject = subject;
                message.Body = new MessageBody(BodyType.HTML, body);
                message.SendAndSaveCopy();
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }
        public static ExchangeService CreateConnection(EmailExchangeInfo info, string user, string password)
        {
            // Hook up the cert callback to prevent error if Microsoft.NET doesn't trust the server
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
            ExchangeService service = new ExchangeService();
            service.Url = new Uri(info.Url);
            if (string.IsNullOrEmpty(user))
            {
                user = info.Email;
                password = info.Password;
            }
            service.Credentials = new WebCredentials(user, password, info.Domain);
            return service;
        }

        public static EmailExchangeInfo GetEmailExchangeInfo()
        {
            var emailExchangeInfo = new EmailExchangeInfo();
            emailExchangeInfo.Url = "https://autodiscover.kenana.sd/EWS/Exchange.asmx";
            emailExchangeInfo.Email = "e.service@kenana.com";
            emailExchangeInfo.Password = "P@ssw0rd";
            emailExchangeInfo.LoginName = "e.service";
            emailExchangeInfo.Domain = "kenana.sd";
            return emailExchangeInfo;
        }

        public class EmailExchangeInfo
        {
            public string Url { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Domain { get; set; }
            public string LoginName { get; set; }
        }
    }
}
