using Common.Types;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IStore: IService
    { 
        Task<string> GetConfigAsync();

        Task<string> GetProductConfigAsync(string product);

        Task SetProductConfigAsync(string product, AppDocument newDoc);

        Task<string> GetMonitorDataAsync();

        Task<string> GetProductMonitorDataAsync(string product);

        Task SetProductMonitorDataAsync(string product, AppDocument newDoc);
    }
}
