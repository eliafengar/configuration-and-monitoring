using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Common;
using Common.Types;

namespace Adapter.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IAdapter : IActor
    {
        Task RegisterEventsAsync();

        Task NewAppConfigAvailable(AppDocument newDoc);

        Task UpdateAppMonitoringData(AppDocument newDoc);
    }
}
