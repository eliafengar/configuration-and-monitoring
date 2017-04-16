using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.Interfaces;
using Common.Types;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Adapter.Interfaces;
using MongoDB.Driver;

namespace Store.Service
{
    /// <summary>
    /// An instance of this class is created for each service instance by the StoreService Fabric runtime.
    /// </summary>
    internal sealed class StoreService : StatefulService, ISubject, IStore
    {
        private ConfigAndMonitorDAO _dao;

        public StoreService(StatefulServiceContext context, ConfigAndMonitorDAO dao)
            : base(context)
        {
            this._dao = dao;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            ServiceEventSource.Current.Message("Open RPC Communication");
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }

        public async Task AddObserverAsync(StoreNotificationType notificationType, Uri observer)
        {
            try
            {
                var listenersDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, List<Uri>>>("ListenersDictionary");
                using (ITransaction tx = this.StateManager.CreateTransaction())
                {
                    List<Uri> newList = null;
                    var result1 = await listenersDict.TryGetValueAsync(tx, notificationType.ToString());
                    if (result1.HasValue)
                        newList = new List<Uri>(result1.Value);
                    else
                        newList = new List<Uri>();

                    if (!newList.Contains(observer))
                        newList.Add(observer);
                    await listenersDict.SetAsync(tx, notificationType.ToString(), newList);
                    await tx.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
            }
        }

        public async Task RemoveObserverAsync(StoreNotificationType notificationType, Uri observer)
        {
            try
            {
                var listenersDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, List<Uri>>>("ListenersDictionary");
                using (ITransaction tx = this.StateManager.CreateTransaction())
                {
                    List<Uri> newList = null;
                    var result1 = await listenersDict.TryGetValueAsync(tx, notificationType.ToString());
                    if (result1.HasValue)
                        newList = new List<Uri>(result1.Value);
                    else
                        newList = new List<Uri>();

                    if (newList.Contains(observer))
                        newList.Remove(observer);
                    await listenersDict.SetAsync(tx, notificationType.ToString(), newList);
                    await tx.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
            }
        }

        public async Task RaiseNotificationAsync(StoreNotificationType notificationType, string product, AppDocument newDoc)
        {
            try
            {
                var listenersDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, List<Uri>>>("ListenersDictionary");
                using (ITransaction tx = this.StateManager.CreateTransaction())
                {
                    List<Uri> newList = null;
                    var result1 = await listenersDict.TryGetValueAsync(tx, notificationType.ToString());
                    if (result1.HasValue)
                        newList = new List<Uri>(result1.Value);
                    else
                        newList = new List<Uri>();

                    foreach (var listener in newList)
                    {

                        // This only creates a proxy object, it does not activate an actor or invoke any methods yet.
                        var myActor = ActorProxy.Create<IAdapter>(new ActorId(product), listener);

                        // This will invoke a method on the actor. If an actor with the given ID does not exist, it will be activated by this method call.
                        await myActor.NewAppConfigAvailable(newDoc);
                    }

                    await tx.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
            }
        }

        public async Task<string> GetConfigAsync()
        {
            string result = string.Empty;
            foreach (var appName in this._dao.AppDAOListNames)
            {
                var app = this._dao.GetAppDAO(appName);
                result += await app.GetAppConfigAsync();
            }
            return result;
        }

        public async Task<string> GetProductConfigAsync(string product)
        {
            return await this._dao.GetAppDAO(product).GetAppConfigAsync();
        }

        public async Task SetProductConfigAsync(string product, AppDocument newDoc)
        {
            await this._dao.GetAppDAO(product).SetAppConfigAsync(newDoc);
            await RaiseNotificationAsync(StoreNotificationType.Configuration, product, newDoc);
        }
        public async Task<string> GetMonitorDataAsync()
        {
            string result = string.Empty;
            foreach (var appName in this._dao.AppDAOListNames)
            {
                var app = this._dao.GetAppDAO(appName);
                result += await app.GetAppMonitorDataAsync();
            }
            return result;
        }

        public async Task<string> GetProductMonitorDataAsync(string product)
        {
            return await this._dao.GetAppDAO(product).GetAppMonitorDataAsync();
        }

        public async Task SetProductMonitorDataAsync(string product, AppDocument newDoc)
        {
            await this._dao.GetAppDAO(product).SetAppMonitorDataAsync(newDoc);
            await RaiseNotificationAsync(StoreNotificationType.Monitoring, product, newDoc);
        }
    }
}
