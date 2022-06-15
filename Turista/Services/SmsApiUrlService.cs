using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Turista.Models;

namespace EProcurementSolution.Services
{
    public static class SmsApiUrlService
    {
        public static bool SendSMS(string key, string phoneNumber, string message)
        {
            var smsApiUrlInfo = GetSmsApiUrlInfo();
            try
            {
                if (!string.IsNullOrEmpty(smsApiUrlInfo.Url))
                    return false;


                if (phoneNumber.Length == 9)
                    phoneNumber = key + phoneNumber;
                else if (phoneNumber.Length == 10)
                    phoneNumber = key + phoneNumber.Remove(0, 1);
                else if(phoneNumber.Length < 9)
                    return false;

                if (message.Length > 336)
                {
                    return false;
                }
                var uri = string.Format(smsApiUrlInfo.Url,phoneNumber,message);
                var objUri = new Uri(uri);
                var objWebRequest = WebRequest.Create(objUri);
                var objWebResponse = objWebRequest.GetResponse();
                var objStream = objWebResponse.GetResponseStream();
                var objStreamReader = new StreamReader(objStream);
                var strHTML = objStreamReader.ReadToEnd();

                return strHTML.Contains("1:") || strHTML.Contains("u:");
            }
            catch
            {
                return false;
            }

        }

        public static SmsApiUrlInfo GetSmsApiUrlInfo()
        {
            var smsApiUrlInfo = new SmsApiUrlInfo();
           // smsApiUrlInfo.Url = settings.SmsInfo.Url;
            return smsApiUrlInfo;
        }

    }
}
