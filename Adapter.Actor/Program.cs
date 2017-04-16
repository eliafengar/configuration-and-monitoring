using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Adapter.Interfaces;

namespace Adapter
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // This line registers an Actor Service to host your actor class with the Service Fabric runtime.
                // The contents of your ServiceManifest.xml and ApplicationManifest.xml files
                // are automatically populated when you build this project.
                // For more information, see https://aka.ms/servicefabricactorsplatform

                ActorRuntime.RegisterActorAsync<TigerAdapterActor>(
                   (context, actorType) => new ActorService(context, actorType)).GetAwaiter().GetResult();

                ActorRuntime.RegisterActorAsync<LionAdapterActor>(
                   (context, actorType) => new ActorService(context, actorType)).GetAwaiter().GetResult();

                // This only creates a proxy object, it does not activate an actor or invoke any methods yet.
                var proxy = ActorProxy.Create<IAdapter>(new ActorId("tiger"), new Uri("fabric:/ConfigAndMonitor/TigerAdapterActor"));
                proxy.RegisterEventsAsync();
                proxy = ActorProxy.Create<IAdapter>(new ActorId("lion"), new Uri("fabric:/ConfigAndMonitor/LionAdapterActor"));
                proxy.RegisterEventsAsync();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
