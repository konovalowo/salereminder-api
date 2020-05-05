using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly FirebaseMessaging _messaging;

        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            var app = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.GetApplicationDefault().CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
            });
            _messaging = FirebaseMessaging.GetMessaging(app);

            _logger = logger;
        }

        private Message CreateMessage(string token, string notificationBody, 
            string notificationTitle, string imageUrl = null)
        {
            return new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Body = notificationBody,
                    Title = notificationTitle,
                    ImageUrl = imageUrl
                }
            };
        }

        private MulticastMessage CreateMulticastMessage(List<string> tokens, string notificationBody, 
            string notificationTitle, string imageUrl = null)
        {
            return new MulticastMessage()
            {
                Tokens = tokens.ToList(),
                Notification = new Notification()
                {
                    Body = notificationBody,
                    Title = notificationTitle,
                    ImageUrl = imageUrl
                }
            };
        }

        public async Task SendNotification(string token, string notificationBody, 
            string notificationTitle, string imageUrl = null)
        {
            var result = await _messaging.SendAsync(CreateMessage(token, notificationBody, notificationTitle, imageUrl));
            _logger.LogInformation($"Sent notification result: {result}");
        }

        public async Task SendMulticastNotification(List<string> tokens, string notificationBody, 
            string notificationTitle, string imageUrl = null)
        {
            var result = await _messaging.SendMulticastAsync(CreateMulticastMessage(tokens, notificationBody, notificationTitle, imageUrl));
            _logger.LogInformation($"Sent multicast notification result: {result}");
        }
    }
}
