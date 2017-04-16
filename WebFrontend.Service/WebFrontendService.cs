using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Globalization;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System.ServiceModel;
using System.ServiceModel.Description;
using Common.Types;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Web.Service
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WebFrontendService : StatelessService, IWebFrontend
    {
        private IStore _storeService = null;

        public WebFrontendService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            ServiceEventSource.Current.Message("Open WCF Web Frontend");
            yield return new ServiceInstanceListener(context =>
            {
                string host = context.NodeContext.IPAddressOrFQDN;
                var endpointConfig = context.CodePackageActivationContext.GetEndpoint("WebFrontendServiceEndpoint");
                int port = endpointConfig.Port;
                string scheme = endpointConfig.Protocol.ToString();
                string uri = string.Format(CultureInfo.InvariantCulture, "{0}://{1}:{2}/", scheme, host, port);
                var listener = new WcfCommunicationListener<IWebFrontend>(
                    serviceContext: context,
                    wcfServiceObject: new WebFrontendService(context),
                    listenerBinding: new WebHttpBinding(WebHttpSecurityMode.None),
                    address: new EndpointAddress(uri)
                );
                var ep = listener.ServiceHost.Description.Endpoints.Last();
                ep.Behaviors.Add(new WebHttpBehavior());
                return listener;
            });
        }

        public async Task<string> GetAllConfig()
        {
            return await StoreServiceProxy?.GetConfigAsync();
        }

        public async Task<string> GetProductConfig(string product)
        {
            return await StoreServiceProxy?.GetProductConfigAsync(product);
        }

        public async Task SetProductConfig(string product, AppDocument newDoc)
        {
            await StoreServiceProxy?.SetProductConfigAsync(product, newDoc);
        }
        public async Task<string> GetAllMonitorData()
        {
            return await StoreServiceProxy?.GetMonitorDataAsync();
        }

        public async Task<string> GetProductMonitorData(string product)
        {
            return await StoreServiceProxy?.GetProductMonitorDataAsync(product);
        }

        public async Task SetProductMonitorData(string product, AppDocument newDoc)
        {
            await StoreServiceProxy?.SetProductMonitorDataAsync(product, newDoc);
        }

        private IStore StoreServiceProxy
        {
            get
            {
                try
                {
                    if (this._storeService == null)
                    {
                        var conf = new ServiceConfig(this.Context) { SectionName = "StoreService" };
                        var storeServiceUri = conf.GetStringValue("StoreServiceUri");
                        var storeServicePartitionKey = conf.GetLongValue("StoreServicePartitionKey");
                        this._storeService = ServiceProxy.Create<IStore>(new Uri(storeServiceUri),
                            new ServicePartitionKey(storeServicePartitionKey));
                    }
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.Message("Error while trying to get ServiceProxy to Stor Service");
                    this._storeService = null;
                }
                return this._storeService;
            }
        }
    }
}
