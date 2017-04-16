using Common;
using Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace Web.Service
{
    [ServiceContract]
    interface IWebFrontend
    {
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "config")]
        Task<string> GetAllConfig();

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "config/{product}")]
        Task<string> GetProductConfig(string product);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, UriTemplate = "config/{product}")]
        Task SetProductConfig(string product, AppDocument newDoc);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "monitor")]
        Task<string> GetAllMonitorData();

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "monitor/{product}")]
        Task<string> GetProductMonitorData(string product);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, UriTemplate = "monitor/{product}")]
        Task SetProductMonitorData(string product, AppDocument newDoc);
    }
}
