using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Turista.Data;
using Turista.Resources;
using Turista.Models;
using Turista.Data.Reservation;
using QRCoder;
using Microsoft.AspNetCore.Mvc.Routing;
//
namespace Domain
{
    public class Payment
    {
        public static bool IsPaid(ApplicationDbContext _context, Guid ReservationId)
        {
            var reservation = _context.Reservations.Include(c => c.Property).FirstOrDefault(c => c.Id == ReservationId);
            var trans = _context.PaymentTransactions.Where(c => c.ReservationId == ReservationId).Sum(c => c.Amount);
            if (trans>=reservation.TotalPrice)
            {
                return true;
            }
            return false;
        }
        public static string PayByCard(ApplicationDbContext _context, double amount,string ccv,string exDate,string cardNo, Guid reservationId,Guid UserId)
        {
            var reservation = _context.Reservations.Find(reservationId);
            PaymentTransaction paymentTransaction = new PaymentTransaction
            {
                UserId=UserId,
                ReservationId=reservationId,
                Amount=amount,
                PaymentDateTime=DateTime.Now,
                PaymentType=(int)enums.PaymentMethod.CreditCard,
                InvoiceNumber = getInvoiceNumber(_context, reservation.PropertyId)
            };
            _context.PaymentTransactions.Add(paymentTransaction);
            _context.SaveChanges();
            return null;
        }
        public static string PayCash(ApplicationDbContext _context,HttpContext httpContext, double amount, Guid reservationId, Guid UserId)
        {
            var reservation = _context.Reservations.Find(reservationId);
            PaymentTransaction paymentTransaction = new PaymentTransaction
            {
                UserId = UserId,
                ReservationId = reservationId,
                Amount = amount,
                PaymentDateTime = DateTime.Now,
                PaymentType = (int)enums.PaymentMethod.Cash,
                RefNo= DateTime.Now.ToString("yyMMddhhmmss")+ reservation.ReservationNumber,
                InvoiceNumber=getInvoiceNumber(_context,reservation.PropertyId)
            };
            _context.PaymentTransactions.Add(paymentTransaction);
            _context.SaveChanges();
            SaveInvoice(_context, httpContext, reservationId, paymentTransaction.Id);
            return null;
        }

        private static int getInvoiceNumber(ApplicationDbContext _context, Guid propertyId)
        {
            try
            {
                var isInvoice = _context.PaymentTransactions.Any(c => c.Reservation.PropertyId == propertyId);
                if (isInvoice)
                {
                    return _context.PaymentTransactions.Where(c => c.Reservation.PropertyId == propertyId).Sum(c=>c.InvoiceNumber)+1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void SaveInvoice(ApplicationDbContext _context, HttpContext httpContext, Guid ReservationId,Guid PaymentId)
        {
            var subPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices", ReservationId + "");
            var pdfpath = Path.Combine(subPath, PaymentId + ".pdf");
            if (!System.IO.Directory.Exists(subPath))
            {
                System.IO.Directory.CreateDirectory(subPath);
            }
            if (System.IO.File.Exists(pdfpath))
            {
                System.IO.File.Delete(pdfpath);
            }


            var reservation = _context.Reservations.Include(c=>c.Invoices).ThenInclude(c=>c.User).Include(c => c.Property.City).Include(c=>c.User).FirstOrDefault(c => c.Id == ReservationId);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoiceTmp.html");
            string html = "";
            var invoice = reservation.Invoices.FirstOrDefault(c => c.Id == PaymentId);
            var TotalPrice = reservation.TotalDays * reservation.DayPrice;
            var UnPaidAmount = TotalPrice - reservation.Invoices.Sum(c => c.Amount);
            var discountFromOffers = Reservation.getPropertyOffer(_context, reservation.Property, reservation.DateFrom, reservation.DateTo);
            var subTotalPrice = TotalPrice - discountFromOffers.Sum(c=>c.Amount);

            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    html += reader.ReadLine();
                }
            }
            var LogoUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");

            html = html.Replace("#LogoUrl#", LogoUrl);
            html = html.Replace("#TotalPaid#", reservation.Invoices.Sum(c => c.Amount) + " "+lang.Currency);
            html = html.Replace("#AmountText#", new ToWord((decimal)invoice.Amount).ConvertToArabic());
            html = html.Replace("#PaidAmount#", invoice.Amount + " " +lang.Currency);

            html = html.Replace("#InvoiceNo#", invoice.InvoiceNumber+"");
            html = html.Replace("#Invoice#", lang.Invoice);
            html = html.Replace("#InvoiceNolbl#", lang.InvoiceNo);
            html = html.Replace("#OurDetails#", lang.OurDetails);
            html = html.Replace("#AppName#", lang.AppName);
            html = html.Replace("#Clientlbl#", lang.ClientInfo);
            html = html.Replace("#PropertyName#", reservation.Property.PropertyName);
            html = html.Replace("#ClientName#", reservation.User.FirstName+" "+ reservation.User.LastName);
            html = html.Replace("#PhoneNumber#", reservation.User.PhoneNumber);
            var payment = "";
            if (invoice.PaymentType==(int)enums.PaymentMethod.Cash)
            {
                payment = lang.Cash;
            }else if (invoice.PaymentType == (int)enums.PaymentMethod.Cash)
            {
                payment = lang.CCard;
            }
            html = html.Replace("#PaymentMethodlbl#", lang.PaymentMethod);
            html = html.Replace("#PaymentMethod#", payment);
            html = html.Replace("#PaymentDatelbl#", lang.Date);
            html = html.Replace("#PaymentDate#", invoice.CreatedDate.ToString("dd-MM-yyyy"));
            html = html.Replace("#ReservationNumberlbl#", lang.ReservationNumber);
            html = html.Replace("#ReservationNumber#", reservation.ReservationNumber+ "");
            html = html.Replace("#ItemDescriptionlbl#", lang.ReservationDetails);
            html = html.Replace("#ItemDescription#", lang.ReservationDetails);

            html = html.Replace("#DayPricelbl#", lang.DayPrice);
            html = html.Replace("#DaysCountlbl#", lang.DaysCount);
            html = html.Replace("#TotalAmountlbl#", lang.Total);
            html = html.Replace("#DiscountFromOfferslbl#", lang.DiscountFromOffers);
            html = html.Replace("#Totallbl#", lang.Total);
            html = html.Replace("#ViewThisInvoiceOnlinelbl#", lang.ViewThisInvoiceOnlinelbl);
            html = html.Replace("#Link#", Domain.Settings.Get(_context).AppLink);
            html = html.Replace("#CheckInDateTimelbl#", lang.CheckIn);
            html = html.Replace("#CheckOutDateTimelbl#", lang.CheckOut);

            html = html.Replace("#CheckInDateTime#", reservation.DateFrom.ToString("dd-MM-yyyy")+" الساعة "+reservation.Property.CheckInTime);
            html = html.Replace("#CheckOutDateTime#", reservation.DateTo.ToString("dd-MM-yyyy") + " الساعة " + reservation.Property.CheckOutTime);
            html = html.Replace("#Notice#", String.IsNullOrEmpty(Settings.Get(_context).InvoiceNotice)?"N/A": Settings.Get(_context).InvoiceNotice);
            html = html.Replace("#PropertyLoclbl#", lang.City);
            html = html.Replace("#PropertyLoc#", reservation.Property.City.CityName);
            html = html.Replace("#DaysCount#", reservation.TotalDays+"");
            html = html.Replace("#DayPrice#", reservation.DayPrice+ " "+lang.Currency);

            var paidBy = "";
            if (invoice.User.UserType==(int)enums.UserType.Admin)
            {
                paidBy = " تم الدفع بواسطة توريستا ";
                paidBy += invoice.User.FirstName + " " + invoice.User.LastName;
            }
            else if(invoice.User.UserType == (int)enums.UserType.BookAdmin || invoice.User.UserType == (int)enums.UserType.BookUser)
            {
                paidBy = " تم الدفع بواسطة مالك العقار ";
                paidBy += invoice.User.FirstName + " " + invoice.User.LastName;
            }
             html = html.Replace("#PaidTo#", paidBy);
            html = html.Replace("#SubTotal#", subTotalPrice + " "+lang.Currency);
            html = html.Replace("#SubTotal#", subTotalPrice + " "+lang.Currency);
            html = html.Replace("#Total#", TotalPrice + " "+lang.Currency);
            html = html.Replace("#DiscountFromOffers#", discountFromOffers.Sum(c=>c.Amount) + " " + lang.Currency);
            var requestContext = $"{Settings.Get(_context).AppLink}/UserAccount/Invoice/{invoice.Id}";

            html = html.Replace("#QRCODE#", getQRCode(requestContext));
            var doc = HtmlToPDF.HtmlToPdf(html);
            System.IO.File.WriteAllBytes(pdfpath, doc);
        }


        private static string getQRCode(string QRCodeText)
        {
            QRCodeGenerator QrGenerator = new QRCodeGenerator();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(QRCodeText, QRCodeGenerator.ECCLevel.Q);
            QRCode QrCode = new QRCode(QrCodeInfo);
            Bitmap QrBitmap = QrCode.GetGraphic(60);
            ImageConverter converter = new ImageConverter();
            var BitmapArray = (byte[])converter.ConvertTo(QrBitmap, typeof(byte[]));
            return string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
        }

        private static void SendWhatsApp(HttpContext httpContext, string whatsAppNumber, string pdfpath)
        {
            throw new NotImplementedException();
        }
    }
}
