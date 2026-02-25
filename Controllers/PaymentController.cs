using EmployeesMVC_Core_8.DTOs;
using EmployeesMVC_Core_8.Interfaces;
using EmployeesMVC_Core_8.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EmployeesMVC_Core_8.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly AppDbContext _context;

        public PaymentController(IPaymentService paymentService, AppDbContext context)
        {
            _paymentService = paymentService;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Pay(int orderId)
        {
            try
            {
                var result = await _paymentService.CreateFawryPayment(orderId);

                if (!result.Success || string.IsNullOrEmpty(result.PaymentUrl))
                    return RedirectToAction("Fail");

                return Redirect(result.PaymentUrl);
            }
            catch (Exception ex)
            {
                return Content($"Payment error: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Fail()
        {
            return View();
        }
        public IActionResult TestPay(int id)
        {
            return View(id);
        }
        public async Task<IActionResult> CreateTestOrder()
        {
            var order = new Order
            {
                CustomerName = "Test User",
                TotalAmount = 100,
                Status = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            //return Content($"Order Created with ID = {order.Id}");

            return RedirectToAction("TestPay", new { id = order.Id });
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] FawryWebhookDto dto)
        {
            var log = new PaymentWebhookLog
            {
                ReferenceNumber = dto.referenceNumber,
                MerchantRefNumber = dto.merchantRefNumber,
                Status = dto.orderStatus,
                RawResponse = JsonSerializer.Serialize(dto)
            };

            _context.PaymentWebhookLogs.Add(log);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(x => x.MerchantRef == dto.merchantRefNumber);

            if (payment != null)
            {
                payment.Status = dto.orderStatus;

                if (dto.orderStatus == "PAID")
                {
                    var order = await _context.Orders.FindAsync(payment.OrderId);
                    if (order != null)
                        order.Status = "Paid";
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> Status(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return Content("Order not found");

            return View(order);
        }


    }
}
