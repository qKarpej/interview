using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GigaStore
{
    public class UserSession
    {
        public static List<string> UserNames { get; set; } = new List<string>();

        public static void SaveUser(string userName)
        {
            UserNames.Add(userName);
        }

        public static bool IsUserLoggedIn(HttpRequest request)
        {
            request.Cookies.TryGetValue("User", out var userName);
            return UserNames.Contains(userName);
        }
    }

    public class RetailController : ControllerBase
    {
        private DataAccessLayer _dataAccessLayer;
        private BackgroundService _backgroundService;

        public EmailService EmailService;

        public RetailController()
        {
            _dataAccessLayer = new DataAccessLayer();
            _backgroundService = new BackgroundService();
            EmailService = new EmailService();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PlaceOrder/{customerId}")]
        public IActionResult PlaceOrder(int customerId, [FromBody] Order order)
        {
            if (UserSession.IsUserLoggedIn(Request))
            {
                _dataAccessLayer.AddOrder(customerId, order);
                _backgroundService.UpdateInventory(order);

                EmailService.SendOrderConfirmationEmailAsync(customerId, order);
                return Ok();
            }
            return BadRequest();
        }
    }

    public class DataAccessLayer
    {
        public void AddOrder(int customerId, Order order)
        {
            using (var context = new RetailDbContext())
            {
                var customer = context.Customers.FirstOrDefault(c => c.Id == customerId);
                var customerOrders = context.Orders.Where(o => o.CustomerId == customerId).ToList();

                customerOrders.Add(order);

                context.Orders.Add(order);
                context.SaveChanges();
            }
        }
    }

    public class BackgroundService
    {
        public void UpdateInventory(Order order)
        {	
            using (var connection = new SqlConnection("..."))
            {
                connection.Open();

                var currentDate = DateTime.Now;

                var query = $"UPDATE Inventory SET Quantity = Quantity - {order.Quantity}, LastUpdated = '{currentDate}' WHERE ProductId = {order.ProductId}";

                using (var command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public class EmailService
    {
        public async void SendOrderConfirmationEmailAsync(int customerId, Order order)
        {
            try
            {

                var emailContent = $"Order confirmation for order ID: {order.Id}.";
                var email = new Dictionary<string, string>
                        {
                                { "to", $"customer{customerId}@example.com" },
                                { "subject", "Order Confirmation" },
                                { "body", emailContent }
                        };
                var content = new FormUrlEncodedContent(email);
                var httpClient = new HttpClient();
                var response = httpClient.PostAsync("https://email-api.example.com/send", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Failed to send order confirmation email.");
                }
            }
            catch (Exception ex)
            {					
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }

    public class RetailDbContext : DbContext
    {
        public RetailDbContext() :
                base(new DbContextOptionsBuilder().UseSqlite("...").Options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
    }

    public class Product
    {
    }

    public class Inventory
    {
    }

    public class Customer
    {
        public int Id { get; set; }
    }

    public class Order
    {
        public object Quantity { get; set; }
        public object ProductId { get; set; }
        public object Id { get; set; }
        public int CustomerId { get; set; }
    }
}
