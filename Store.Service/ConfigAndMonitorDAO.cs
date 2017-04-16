using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Service
{
    public class ConfigAndMonitorDAO
    {
        protected const string DATABASE_NAME = "Animals";
        //private const string MONGODB_HOST_NAME = "localhost";
        //private const string MONGODB_PORT = "27017";
        //private const string MONGODB_URL_TEMPLATE = "mongodb://{0}:{1}/";
        //private static MongoClient mongoClient = new MongoClient(string.Format(MONGODB_URL_TEMPLATE, MONGODB_HOST_NAME, MONGODB_PORT));

        protected Dictionary<string, AppDAO> appDAOList = null;

        protected MongoClient _mongoClient;

        public ConfigAndMonitorDAO(MongoClient mongoClient)
        {
            this._mongoClient = mongoClient;
            InitAppDAOList();
        }

        private void InitAppDAOList()
        {
            this.appDAOList = new Dictionary<string, AppDAO>();
            var database = this._mongoClient.GetDatabase(DATABASE_NAME);
            foreach (var collection in database.ListCollectionsAsync().Result.ToListAsync<BsonDocument>().Result)
            {
                var appName = collection.GetValue("name").ToString().Replace("configuration.", "").Replace("monitoring.", "").Replace(DATABASE_NAME + ".", "");
                if (!appDAOList.ContainsKey(appName))
                    appDAOList.Add(appName, new AppDAO(appName, database));
            }
        }

        public AppDAO GetAppDAO(string appName)
        {
            if (!appDAOList.ContainsKey(appName))
                appDAOList.Add(appName, new AppDAO(appName, this._mongoClient.GetDatabase(DATABASE_NAME)));
            return appDAOList[appName];
        }

        public List<string> AppDAOListNames
        {
            get
            {
                return appDAOList.Keys.ToList<string>();
            }
        }
    }
}
