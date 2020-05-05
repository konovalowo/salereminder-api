using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductApi.Models;
using System;
using System.Collections.Generic;
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
        const string notificationBody = "{0} is on sale!";
        const string notificationTitle = "Sale!";

        public UpdateDbHostedService(IServiceScopeFactory scopeFactory, ILogger<UpdateDbHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Update Database Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

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
                    var parsedProduct = await parserService.Parse(item.Url);

                    //test
                    if (parsedProduct.Price < item.Price)
                    {
                        item.IsOnSale = true;
                        var tokens = item.UserProducts.Select(u => u.UserProfile.FirebaseToken);
                        await notificationService.SendMulticastNotification(tokens.ToList(), string.Format(notificationBody, item.Name), notificationTitle, item.Image);
                    }

                    item.Price = parsedProduct.Price;
                }

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
