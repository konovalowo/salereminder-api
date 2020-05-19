using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public interface INotificationService
    {
        public Task SendNotification(string token, string notificationBody, string notificationTitle);

        public Task SendMulticastNotification(List<string> tokens, string notificationBody, string notificationTitle);
    }
}
