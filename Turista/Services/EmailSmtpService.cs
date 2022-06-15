using Turista.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Collections;

namespace Turista.Services
{
    public static class EmailSmtpService
    {

        public static bool SendEmail(object receivers,object copyto, string body, string subject,List<Attachment> Attachments)
        {
            try
            {
                var emailSmtpInfo = GetEmailSmtpInfo();
                var smtp = new SmtpClient(emailSmtpInfo.Host, emailSmtpInfo.Port);
                var basicAuthenticationInfo = new NetworkCredential(emailSmtpInfo.Email, emailSmtpInfo.Password);
                smtp.UseDefaultCredentials = false;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = basicAuthenticationInfo;
                smtp.EnableSsl = false;
                var mail = new MailMessage
                {
                    From = new MailAddress(emailSmtpInfo.Email)
                };
                if (receivers is IList&& receivers.GetType().IsGenericType)
                {
                    foreach (string to in (List<string>)receivers)
                    {
                        mail.To.Add(to);
                    }
                }
                if (copyto is IList && copyto.GetType().IsGenericType)
                {
                    if (copyto != null && ((List<string>)copyto).Count > 0)
                    {
                        foreach (string copy in (List<string>)copyto)
                        {
                            mail.CC.Add(copy);
                        }
                    }
                }

                if (Attachments != null)
                {
                    foreach (Attachment attachment in Attachments)
                    {
                        mail.Attachments.Add(attachment);
                    }
                }
                
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                smtp.Send(mail);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static bool SendEmail(object receivers, string body, string subject)
        {
            try
            {
                var emailSmtpInfo = GetEmailSmtpInfo();
                var smtp = new SmtpClient(emailSmtpInfo.Host, emailSmtpInfo.Port);
                var basicAuthenticationInfo = new NetworkCredential(emailSmtpInfo.Email, emailSmtpInfo.Password);
                smtp.UseDefaultCredentials = false;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = basicAuthenticationInfo;
                smtp.EnableSsl = false;
                var mail = new MailMessage
                {
                    From = new MailAddress(emailSmtpInfo.Email)
                };
                if (receivers is IList && receivers.GetType().IsGenericType)
                {
                    foreach (string to in (List<string>)receivers)
                    {
                        mail.To.Add(to);
                    }
                }

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SendEmail(object receivers, string body, string subject, List<Attachment> Attachments)
        {
            try
            {
                var emailSmtpInfo = GetEmailSmtpInfo();
                var smtp = new SmtpClient(emailSmtpInfo.Host, emailSmtpInfo.Port);
                var basicAuthenticationInfo = new NetworkCredential(emailSmtpInfo.Email, emailSmtpInfo.Password);
                smtp.UseDefaultCredentials = false;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = basicAuthenticationInfo;
                smtp.EnableSsl = false;
                var mail = new MailMessage
                {
                    From = new MailAddress(emailSmtpInfo.Email)
                };
                if (receivers is IList && receivers.GetType().IsGenericType)
                {
                    foreach (string to in (List<string>)receivers)
                    {
                        mail.To.Add(to);
                    }
                }

                if (Attachments != null)
                {
                    foreach (Attachment attachment in Attachments)
                    {
                        mail.Attachments.Add(attachment);
                    }
                }

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SendEmail(object receivers, object copyto, string body, string subject)
        {
            try
            {
                var emailSmtpInfo = GetEmailSmtpInfo();
                var smtp = new SmtpClient(emailSmtpInfo.Host, emailSmtpInfo.Port);
                var basicAuthenticationInfo = new NetworkCredential(emailSmtpInfo.Email, emailSmtpInfo.Password);
                smtp.UseDefaultCredentials = false;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = basicAuthenticationInfo;
                smtp.EnableSsl = false;
                var mail = new MailMessage
                {
                    From = new MailAddress(emailSmtpInfo.Email)
                };
                if (receivers is IList && receivers.GetType().IsGenericType)
                {
                    foreach (string to in (List<string>)receivers)
                    {
                        mail.To.Add(to);
                    }
                }
                if (copyto is IList && copyto.GetType().IsGenericType)
                {
                    if (copyto != null && ((List<string>)copyto).Count > 0)
                    {
                        foreach (string copy in (List<string>)copyto)
                        {
                            mail.CC.Add(copy);
                        }
                    }
                }

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static EmailInfo GetEmailSmtpInfo()
        {
            var emailSmtpInfo = new EmailInfo();
            //emailSmtpInfo.Port = appSettings.EmailSmtpInfo.Port;
            //emailSmtpInfo.Host = appSettings.EmailSmtpInfo.Host;
            //emailSmtpInfo.Email = appSettings.EmailSmtpInfo.Email;
            //emailSmtpInfo.Password = appSettings.EmailSmtpInfo.Password;
            return emailSmtpInfo;
        }

    }

}
