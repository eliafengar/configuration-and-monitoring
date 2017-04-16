using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Types
{
    public class AppDocument
    {
        [BsonId]
        public ObjectId Id { get; private set; }
        public DateTime Created { get; set; }
        public List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class MetadataDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId LatestID { get; set; }
    }
}
