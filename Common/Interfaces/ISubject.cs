using Common.Types;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISubject: IService
    {
        Task AddObserverAsync(StoreNotificationType notificationType, Uri observer);

        Task RemoveObserverAsync(StoreNotificationType notificationType, Uri observer);

        Task RaiseNotificationAsync(StoreNotificationType notificationType, string product, AppDocument newDoc); 
    }
}
