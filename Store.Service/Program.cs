using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.Types;
using MongoDB.Driver;

namespace Store.Service
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
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When StoreService Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("StoreServiceType",
                    context => {
                        //TODO: Replace with Modern Dependancy Injection Framework
                        var conf = new ServiceConfig(context) { SectionName = "MongoDB" };
                        var connStr = conf.GetStringValue("ConnectionString");
                        var mongoClient = new MongoClient(connStr);
                        var dao = new ConfigAndMonitorDAO(mongoClient);
                        return new StoreService(context, dao);
                        }).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(StoreService).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
