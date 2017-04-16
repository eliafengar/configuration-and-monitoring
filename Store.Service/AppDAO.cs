using Common.Interfaces;
using Common.Types;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Store.Service
{
    public class AppDAO : IConfigDAO, IMonitorDAO
    {
        //TODO: Move to App Config File
        private readonly ObjectId METADATA_ID = new ObjectId("000000000000000000000000");

        private IMongoDatabase _database = null;
        private IMongoCollection<AppDocument> _configurationCollection;
        private IMongoCollection<AppDocument> _monitorCollection;

        private ObjectId _currentConfigId;
        private ObjectId _currentMonitorId;

        public AppDAO(string appName, IMongoDatabase database)
        {
            this.AppName = appName;
            this._database = database;
            if (this._database != null)
            {
                this._configurationCollection = this._database.GetCollection<AppDocument>(string.Format("configuration.{0}", this.AppName));
                this._monitorCollection = this._database.GetCollection<AppDocument>(string.Format("monitoring.{0}", this.AppName));
            }

            this._currentConfigId = this.GetCurrentAsync(this._configurationCollection).Result.Id;
            this._currentMonitorId = this.GetCurrentAsync(this._monitorCollection).Result.Id;   

            //Thread t = new Thread(CheckAppChanges);
            //t.Start();
        }

        public string AppName { get; set; }

        public event EventHandler<AppConfigEventArgs> AppConfigChanged;
        public event EventHandler<AppMonitorDataEventArgs> AppMonitorDataChanged;

        public async Task DeleteAppConfigAsync(string id)
        {
            await this.DeleteDocumentAsync(this._configurationCollection, ObjectId.Parse(id));
        }

        public async Task DeleteAppMonitorDataAsync(string id)
        {
            await this.DeleteDocumentAsync(this._monitorCollection, ObjectId.Parse(id));
        }

        public async Task<string> GetAppConfigAsync()
        {
            var current = await this.GetCurrentAsync(this._configurationCollection);
            if (current.Id != METADATA_ID)
                return current.ToJson();
            return new BsonDocument().ToJson();
        }

        public async Task<string> GetAppMonitorDataAsync()
        {
            var current = await this.GetCurrentAsync(this._monitorCollection);
            if (current.Id != METADATA_ID)
                return current.ToJson();
            return new BsonDocument().ToJson();
        }

        public async Task SetAppConfigAsync(AppDocument newDoc)
        {
            var id = await this.InsertDocumentAsync(this._configurationCollection, newDoc);
            this._currentConfigId = id;
        }

        public async Task SetAppMonitorDataAsync(AppDocument newDoc)
        {
            var id = await this.InsertDocumentAsync(this._monitorCollection, newDoc);
            this._currentMonitorId = id;
        }

        private async Task<AppDocument> GetCurrentAsync(IMongoCollection<AppDocument> collection)
        {
            var metadata = await this.GetMetadataAsync(collection);
            var current = await collection.FindAsync<AppDocument>(item => item.Id == metadata.LatestID);
            try
            {
                return current.Single();
            }
            catch (Exception ex)
            {
                //TODO: Write to Fabric logs
            }
            return new AppDocument();
        }

        private async Task<MetadataDocument> GetMetadataAsync(IMongoCollection<AppDocument> collection)
        {
            var databaseName = collection.Database.DatabaseNamespace.DatabaseName;
            var medatadataColl = this._database.GetCollection<MetadataDocument>(collection.CollectionNamespace.FullName.Replace(databaseName + ".", ""));
            var metadata = await medatadataColl.FindAsync<MetadataDocument>(item => item.Id == METADATA_ID);
            try
            {
                return metadata.Single();
            }
            catch (Exception ex)
            {
                //TODO: Write to Fabric logs
            }
            return new MetadataDocument();
        }

        private async Task<ObjectId> InsertDocumentAsync(IMongoCollection<AppDocument> collection, AppDocument newDoc)
        {
            newDoc.Created = DateTime.Now;
            await collection.InsertOneAsync(newDoc);
            var databaseName = collection.Database.DatabaseNamespace.DatabaseName;
            var medatadataColl = this._database.GetCollection<MetadataDocument>(collection.CollectionNamespace.FullName.Replace(databaseName + ".", ""));
            var medadataDoc = new MetadataDocument { LatestID = newDoc.Id };
            await medatadataColl.ReplaceOneAsync(item => item.Id == METADATA_ID, medadataDoc, new UpdateOptions { IsUpsert = true });
            return newDoc.Id;
        }

        private async Task DeleteDocumentAsync(IMongoCollection<AppDocument> collection, ObjectId id)
        {
            await collection.DeleteOneAsync<AppDocument>(item => item.Id == id);
        }

        private async void CheckAppChanges()
        {
            while (true)
            {
                var metadata = await this.GetMetadataAsync(this._configurationCollection);
                if (metadata.LatestID != this._currentConfigId)
                {
                    var newDoc = await this.GetCurrentAsync(this._configurationCollection);
                    if (this.AppConfigChanged != null)
                    {
                        var args = new AppConfigEventArgs { NewAppConfig = newDoc.ToJson() };
                        AppConfigChanged(this, args);
                    }
                }

                metadata = await this.GetMetadataAsync(this._monitorCollection);
                if (metadata.LatestID != this._currentMonitorId)
                {
                    var newDoc = await this.GetCurrentAsync(this._monitorCollection);
                    if (this.AppMonitorDataChanged != null)
                    {
                        var args = new AppMonitorDataEventArgs { NewAppMonitorData = newDoc.ToJson() };
                        AppMonitorDataChanged(this, args);
                    }
                }

                Thread.Sleep(new TimeSpan(0, 0, 15));
            }
        }
    }
}
