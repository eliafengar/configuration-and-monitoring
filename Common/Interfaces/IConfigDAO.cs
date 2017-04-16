using Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IConfigDAO
    {
        Task<string> GetAppConfigAsync();

        Task SetAppConfigAsync(AppDocument newDoc);

        Task DeleteAppConfigAsync(string id);

        event EventHandler<AppConfigEventArgs> AppConfigChanged;
    }

    public class AppConfigEventArgs : EventArgs
    {
        public string NewAppConfig { get; set; }
    }
}
