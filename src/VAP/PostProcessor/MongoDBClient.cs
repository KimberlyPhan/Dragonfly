// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using MongoDB.Bson;
using MongoDB.Driver;
using System.Configuration;

namespace PostProcessor
{
    public class MongoDBClient
    {
        private static string connectionString = ConfigurationManager.AppSettings["MongoDBConnectionString"] + "?retryWrites=true&w=majority";
        private static readonly MongoClient client = new MongoClient(connectionString);

        public static void CreateDocument(string cltName, string content)
        {
            var database = client.GetDatabase("Dragonfly");
            var collection = database.GetCollection<BsonDocument>("test");
            var document = BsonDocument.Parse(content);
            collection.InsertOne(document);
        }
    }
}
