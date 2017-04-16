using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Types
{
    public class ServiceConfig
    {
        protected ConfigurationPackage _configurationPackage;
        protected ConfigurationSection _configurationSection;

        public ServiceConfig(ServiceContext context)
        {
            this._configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
        }

        public string SectionName
        {
            get
            {
                return this._configurationSection.Name;
            }
            set
            {
                this._configurationSection = this._configurationPackage.Settings.Sections[value];
            }
        }

        public string GetStringValue(string paramName)
        {
            return this._configurationSection.Parameters[paramName].Value;
        }

        public int GetIntValue(string paramName)
        {
            int retVal;
            var result = Int32.TryParse(this._configurationSection.Parameters[paramName].Value, out retVal);
            return result ? retVal : -1;
        }

        public Int64 GetLongValue(string paramName)
        {
            Int64 retVal;
            var result = Int64.TryParse(this._configurationSection.Parameters[paramName].Value, out retVal);
            return result ? retVal : -1;
        }
    }
}
