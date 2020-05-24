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
            _logger = logger;

            FirebaseApp app;
            try
            {
                app = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.GetApplicationDefault().CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
                });
                _messaging = FirebaseMessaging.GetMessaging(app);
            }
            catch (Exception)
            {
                logger.LogError("No FCM application credentials found. " +
                    "\nDefine environment variable GOOGLE_APPLICATION_CREDENTIALS.");
            }
        }

        private Message CreateMessage(string token, string notificationBody, 
            string notificationTitle)
        {
            return new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Body = notificationBody,
                    Title = notificationTitle,
                }
            };
        }

        private MulticastMessage CreateMulticastMessage(List<string> tokens, string notificationBody, 
            string notificationTitle)
        {
            return new MulticastMessage()
            {
                Tokens = tokens.ToList(),
                Notification = new Notification()
                {
                    Body = notificationBody,
                    Title = notificationTitle,
                }
            };
        }

        public async Task SendNotification(string token, string notificationBody, 
            string notificationTitle)
        {
            if (_messaging != null)
            {
                var result = await _messaging.SendAsync(CreateMessage(token, notificationBody, notificationTitle));
                _logger.LogInformation($"Sent notification result.");
            }
            else
            {
                _logger.LogError("No FCM application credentials found.");
            }
        }

        public async Task SendMulticastNotification(List<string> tokens, string notificationBody, 
            string notificationTitle)
        {
            if (_messaging != null)
            {
                var result = await _messaging.SendMulticastAsync(CreateMulticastMessage(tokens, notificationBody, notificationTitle));
                _logger.LogInformation($"Sent multicast notification.");
            }
            else
            {
                _logger.LogError("No FCM application credentials found.");
            }
        }
    }
}
