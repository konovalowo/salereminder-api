using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductApi.Models;
using ProductApi.Parser;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public class UpdateDbHostedService : IHostedService
    {
        private readonly ILogger<UpdateDbHostedService> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private Timer _timer;

        // Notifications
        const string notificationBody = "{0}";
        const string notificationTitle = "Sale!";

        private readonly DateTime schedule;

        public UpdateDbHostedService(IServiceScopeFactory scopeFactory, ILogger<UpdateDbHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            schedule = DateTime.Today.AddHours(12).AddMinutes(0).AddDays(1);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Update Database Hosted Service running.");

            try
            {
                _timer = new Timer(DoWork, null,
                    schedule.Subtract(DateTime.Now), TimeSpan.FromMinutes(schedule.Minute + schedule.Hour * 60));
            }
            catch (ArgumentOutOfRangeException)
            {
                var scheduleNextDay = schedule.AddDays(1);
                _timer = new Timer(DoWork, null,
                    scheduleNextDay.Subtract(DateTime.Now), TimeSpan.FromMinutes(scheduleNextDay.Minute + scheduleNextDay.Hour * 60));
            }

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Update Database Hosted Service is working.");
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var parserService = scope.ServiceProvider.GetRequiredService<IParserService>();

                var products = await context.Products.Include(p => p.UserProducts)
                                                     .ThenInclude(u => u.UserProfile).ToListAsync();

                foreach (var item in products)
                {
                    Product parsedProduct;
                    try
                    {
                        parsedProduct = await parserService.Parse(item.Url);
                    }
                    catch (ParserException e)
                    {
                        _logger.LogError("ParserException while updating database " + e.Message);
                        continue;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Exception in parser while updating database " + e.Message);
                        continue;
                    }


                    if (parsedProduct.Price < item.Price)
                    {
                        try
                        {
                            item.IsOnSale = true;
                            var tokens = item.UserProducts.Select(u => u.UserProfile.FirebaseToken);
                            if (tokens.Count() != 0)
                            {
                                await notificationService.SendMulticastNotification(tokens.ToList(), string.Format(notificationBody, item.Name), notificationTitle);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Failed to send multicast notification: " + e.Message);
                        }
                    }
                    else if (parsedProduct.Price >= item.Price)
                    {
                        item.IsOnSale = false;
                    }

                    item.Price = parsedProduct.Price;
                    await context.SaveChangesAsync();
                }
                _logger.LogInformation("Finished updating database");
                await context.SaveChangesAsync();
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Update Database Hosted Service is stopping.");
      
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
