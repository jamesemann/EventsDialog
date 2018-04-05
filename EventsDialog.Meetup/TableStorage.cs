using System;
using System.Configuration;
using EventsDialog.Meetup.Controllers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace EventsDialog.Meetup
{
    public class TableStorage
    {
        private static string _storageConnectionString;

        static TableStorage()
        {
            _storageConnectionString = ConfigurationManager.AppSettings["meetupTableStorageConnectionString"];

            if (string.IsNullOrEmpty(_storageConnectionString))
            {
                throw new Exception(@"populate appsetting <add key=""meetupTableStorageConnectionString"" value=""""/>");
            }
        }

        public static CloudTable GetTableReference()
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference("tokens");
            table.CreateIfNotExists();
            return table;
        }

        public static void InsertOrUpdate(AccessResponse accessResponse)
        {
            var table = GetTableReference();
            var insertOperation = TableOperation.InsertOrReplace(accessResponse);
            table.Execute(insertOperation);
        }

        public static AccessResponse RetrieveByUserId(string userId)
        {
            var table = GetTableReference();
            var retrieveOperation = TableOperation.Retrieve<AccessResponse>("all", userId);
            return table.Execute(retrieveOperation).Result as AccessResponse;
        }
    }
}
