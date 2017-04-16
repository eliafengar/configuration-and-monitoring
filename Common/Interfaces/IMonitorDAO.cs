using Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IMonitorDAO
    {
        Task<string> GetAppMonitorDataAsync();

        Task SetAppMonitorDataAsync(AppDocument newDoc);

        Task DeleteAppMonitorDataAsync(string id);

        event EventHandler<AppMonitorDataEventArgs> AppMonitorDataChanged;
    }

    public class AppMonitorDataEventArgs : EventArgs
    {
        public string NewAppMonitorData { get; set; }
    }
}
