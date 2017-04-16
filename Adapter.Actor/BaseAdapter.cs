using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Adapter.Interfaces;
using Common;
using Common.Types;
using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;

namespace Adapter
{
    internal abstract class BaseAdapter : Actor, IAdapter
    {
        protected ISubject _subjectService = null;
        protected IStore _storeService = null;

        /// <summary>
        /// Initializes a new instance of Adapter
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public BaseAdapter(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        public string ProductName { get; set; }

        public async Task RegisterEventsAsync()
        {
            await SubjectServiceProxy?.AddObserverAsync(StoreNotificationType.Configuration, this.ServiceUri);
        }

        protected ISubject SubjectServiceProxy
        {
            get
            {
                try
                {
                    if (this._subjectService == null)
                    {
                        var conf = new ServiceConfig(this.ActorService.Context) { SectionName = "StoreService" };
                        var storeServiceUri = conf.GetStringValue("StoreServiceUri");
                        var storeServicePartitionKey = conf.GetLongValue("StoreServicePartitionKey");
                        this._subjectService = ServiceProxy.Create<ISubject>(new Uri(storeServiceUri),
                            new ServicePartitionKey(storeServicePartitionKey));
                    }
                }
                catch (Exception ex)
                {
                    ActorEventSource.Current.ActorMessage(this, ex.Message);
                    this._subjectService = null;
                }
                return this._subjectService;
            }
        }

        protected IStore StoreServiceProxy
        {
            get
            {
                try
                {
                    if (this._storeService == null)
                    {
                        var conf = new ServiceConfig(this.ActorService.Context) { SectionName = "StoreService" };
                        var storeServiceUri = conf.GetStringValue("StoreServiceUri");
                        var storeServicePartitionKey = conf.GetLongValue("StoreServicePartitionKey");
                        this._storeService = ServiceProxy.Create<IStore>(new Uri(storeServiceUri),
                            new ServicePartitionKey(storeServicePartitionKey));
                    }
                }
                catch (Exception ex)
                {
                    ActorEventSource.Current.ActorMessage(this, ex.Message);
                    this._storeService = null;
                }
                return this._storeService;
            }
        }

        public async Task NewAppConfigAvailable(AppDocument newDoc)
        {
            //TODO: Call proper ServiceProxy or ActorProxy for start/init application
        }

        public async Task UpdateAppMonitoringData(AppDocument newDoc)
        {
            await StoreServiceProxy?.SetProductMonitorDataAsync(this.ProductName, newDoc);
        }
    }
}
