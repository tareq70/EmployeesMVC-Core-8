using Azure.Core;
using EmployeesMVC_Core_8.DTOs;
using EmployeesMVC_Core_8.Interfaces;
using EmployeesMVC_Core_8.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EmployeesMVC_Core_8.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public PaymentService(AppDbContext context, HttpClient http, IConfiguration config)
        {
            _context = context;
            _http = http;
            _config = config;
        }

        public async Task<FawryPaymentResult> CreateFawryPayment(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");

            var merchantRef = $"ORD-{orderId}-{DateTime.UtcNow.Ticks}";
            order.MerchantRef = merchantRef;

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = order.TotalAmount,
                MerchantRef = merchantRef,
                Status = "Pending"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var merchantCode = _config["Fawry:MerchantCode"]?.Trim();
            var secret = _config["Fawry:Secret"]?.Trim();
            var customerId = "777777";
            string cardNumber = "4242424242424242";
            string cardExpiryYear = "29";
            string cardExpiryMonth = "05";
            string cvv = "123";
            string paymentMethod = "PayUsingCC";
            var amountStr = payment.Amount.ToString("0.00", CultureInfo.InvariantCulture);

            var returnUrl = $"https://abcd-41-33-22-11.ngrok-free.app/payment/status?orderId={order.Id}";

            var signature = GenerateSignature(
                merchantCode, merchantRef, customerId,
                paymentMethod, amountStr,
                cardNumber, cardExpiryYear, cardExpiryMonth, cvv,
                returnUrl, secret
            );

            var request = new
            {
                merchantCode,
                merchantRefNum = merchantRef,
                customerProfileId = customerId,
                cardNumber,
                cardExpiryYear,
                cardExpiryMonth,
                cvv,
                amount = amountStr,
                currencyCode = "EGP",
                paymentMethod,
                customerName = order.CustomerName,
                customerMobile = "201000000000",
                customerEmail = "test@test.com",
                paymentExpiry = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeMilliseconds(),
                returnUrl,
                webhookUrl = "https://abcd-41-33-22-11.ngrok-free.app/payment/webhook",
                chargeItems = new[]
                {
                  new
                  {
                   itemId = $"ITEM-{orderId}",
                   description = $"Order #{orderId}",
                   price = amountStr,
                   quantity = 1
                  }
                },
                enable3DS = true,
                authCaptureModePayment = false,
                description = "example description",
                signature
            };



            var response = await _http.PostAsJsonAsync(
                "https://atfawry.fawrystaging.com/ECommerceWeb/Fawry/payments/charge",
                request);
            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonRequest);


            //var result = await response.Content.ReadFromJsonAsync<FawryApiResponse>();

            //if (result == null || string.IsNullOrEmpty(result.referenceNumber))
            //    throw new Exception("Fawry error");

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Fawry HTTP Error: {raw}");

            var result = JsonSerializer.Deserialize<FawryApiResponse>(raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
                throw new Exception($"Fawry invalid response: {raw}");

            if (string.IsNullOrEmpty(result.referenceNumber))
                throw new Exception($"Fawry rejected request: {raw}");


            payment.ReferenceNumber = result.referenceNumber;
            payment.PaymentUrl = result.paymentUrl;
            payment.Status = "Created";

            await _context.SaveChangesAsync();

            return new FawryPaymentResult
            {
                Success = true,
                PaymentUrl = result.paymentUrl,
                ReferenceNumber = result.referenceNumber,
                MerchantRef = merchantRef
            };
        }
        private string GenerateSignature(string merchantCode, string merchantRef,
             string customerId, string paymentMethod, string amount, string cardNumber,
             string cardExpiryYear, string cardExpiryMonth, string cvv, string returnUrl, string secret)
        {
            var raw = string.Concat(merchantCode, merchantRef, customerId,
                paymentMethod, amount, cardNumber, cardExpiryYear, cardExpiryMonth,
                cvv, returnUrl, secret
            );

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
/*
 
 public async Task<IActionResult> GenrateLinkFawryAsync(int AppointmentId)
        {
            var appointment = _context.Appointments.Include(z=>z.Patient).FirstOrDefault(z => z.Id == AppointmentId);
            var chargeItem = new ChargeItemVM();
            chargeItem.ItemId = appointment.Id.ToString();
            chargeItem.Price = appointment.AppointmentPrice.ToString();
            chargeItem.Description = "حجز";
            chargeItem.Quantity = "1";
            var url = $"{HttpContext.Request.Scheme}{Uri.SchemeDelimiter}{Request.Host}/Fawry/AddBookingPayment?AppointmentId="
                + appointment.Id+"&"
               ;
            

            string merchantCode = "1tSa6uxz2nTwlaAmt38enA==";
            string merchantRefNum = "12143754405";
            string customerProfileId = "777777";
            string paymentMethod = "PayUsingCC";
            string amount = "505"; /*appointment.AppointmentPrice.ToString();
string cardNumber = "4242424242424242";
string cardExpiryYear = "29";
string cardExpiryMonth = "05";
string cvv = "123";
string returnUrl = "https://www.google.com/";
string merchantSecretKey = "259af31fc2f74453b3a55739b21ae9ef";
var Patient = appointment.Patient;
//// Build signature
string signatureBody = string.Concat(
    merchantCode,
    merchantRefNum,
    customerProfileId,
    paymentMethod,
    amount,
    cardNumber,
    cardExpiryYear,
    cardExpiryMonth,
    cvv,
    url,
    merchantSecretKey
);
string signature = GetSHA256Hash(signatureBody);

var payload = new FawryPaymentRequeVm
{
    MerchantCode = merchantCode,
    MerchantRefNum = merchantRefNum,
    CustomerName = Patient.Name,
    CustomerMobile = "01234567891",
    CustomerEmail = "example@gmail.com",
    CustomerProfileId = customerProfileId,
    CardNumber = cardNumber,
    CardExpiryYear = cardExpiryYear,
    CardExpiryMonth = cardExpiryMonth,
    CVV = cvv,
    Amount = amount,
    CurrencyCode = "EGP",
    Language = "en-gb",
    ChargeItems = new List<ChargeItemVM>
                {
                    new ChargeItemVM
                    {
                        ItemId=appointment.Id.ToString(),
                        Description="mmmmmmmmm",
                        Quantity="1",
                        Price=appointment.AppointmentPrice.ToString()
                    }

                },
    Enable3DS = true,
    AuthCaptureModePayment = false,
    ReturnUrl = url,

    PaymentMethod = paymentMethod,
    Description = "example description",
    sign = signature
};
var responseObject = await _faweryRequest.SendPaymentAsync(payload);
return Json(new { link = responseObject.nextAction.redirectUrl });
                }
 */