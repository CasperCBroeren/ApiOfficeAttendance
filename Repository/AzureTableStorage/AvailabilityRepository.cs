using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;

namespace ApiOfficeAttendance.Repository.AzureTableStorage
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly IConfiguration configuration;
        public string TableName { get; }

        public AvailabilityRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.TableName = "officeavailability";
        } 

        public async IAsyncEnumerable<PersonInOffice> Find(DateTime startDate, string organization, int maxDays)
        {
            var tableQuery = new TableQuery<PersonInOffice> {TakeCount = maxDays};
            tableQuery.Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startDate.Date.Ticks.ToString()));
            var table = await this.CreateTable(organization);
            TableContinuationToken token = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(tableQuery, token);
                foreach (var inOfficeAvailable in result.Results)
                {
                    yield return inOfficeAvailable;
                }

                token = result.ContinuationToken;
            } while (token != null);
        }

        public async Task Add(string organization, PersonInOffice item)
        {
            var mergeOperation = TableOperation.InsertOrMerge(item);
            var table = await CreateTable(organization);
            await table.ExecuteAsync(mergeOperation);
        }

        public async Task Remove(string organization, PersonInOffice item)
        {
            var mergeOperation = TableOperation.Delete(item);
            var table = await CreateTable(organization);
            await table.ExecuteAsync(mergeOperation);
        }

        private async Task<CloudTable> CreateTable(string organization)
        {
            var tableStorageConnectionstring = this.configuration["Azure:TableStorageConnectionstring"];
            var storageAccount = tableStorageConnectionstring.Equals("dev") 
                 ? CloudStorageAccount.DevelopmentStorageAccount
                 : CloudStorageAccount.Parse(tableStorageConnectionstring);
            var client = storageAccount.CreateCloudTableClient();
            var table = client.GetTableReference($"{this.TableName}{organization}");
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}
