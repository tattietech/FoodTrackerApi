using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using foodTrackerApi.Interfaces;
using foodTrackerApi.Models;
using Newtonsoft.Json;
using System.Net;

namespace foodTrackerApi.Services
{
    public class DynamoService<T> : IDynamoService
    {
        protected string? tableName;
        protected AmazonDynamoDBClient client;
        protected DynamoDBContext context;
        private readonly string identifier;
        private readonly Table table;

        public DynamoService(string identifier)
        {
            tableName = Environment.GetEnvironmentVariable("FOOD_TRACKER_TABLE_NAME");
            client = new AmazonDynamoDBClient();
            context = new DynamoDBContext(client);
            this.identifier = identifier;
            table = Table.LoadTable(client, "food-tracker-db");
        }

        public async Task<DynamoServiceResponse> List(int household)
        {
            try
            {
                var request = new QueryRequest
                {
                    TableName = tableName,
                    KeyConditionExpression = "household = :household AND begins_with(id, :idStart)",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":household", new AttributeValue { N =  household.ToString()}},
                    {":idStart", new AttributeValue { S =  identifier} } }
                };

                var dbResponse = await client.QueryAsync(request);
                List<T> items = new();

                foreach (var item in dbResponse.Items)
                {
                    var doc = Document.FromAttributeMap(item);
                    var mappedItem = context.FromDocument<T>(doc);

                    items.Add(mappedItem);
                }

                return DynamoServiceResponse.ReturnSuccess(JsonConvert.SerializeObject(items));
            }
            catch(Exception ex)
            {
                return DynamoServiceResponse.ReturnFailure(ex.Message);
            }
        }

        public async Task<DynamoServiceResponse> Put(string item)
        {
            try
            {
                var itemObject = JsonConvert.DeserializeObject<T>(item);
                var doc = context.ToDocument<T>(itemObject);

                await table.PutItemAsync(doc);

                return DynamoServiceResponse.ReturnSuccess(item);
            }
            catch(Exception ex)
            {
                return DynamoServiceResponse.ReturnFailure(ex.Message);
            }
        }

        public async Task<DynamoServiceResponse> Delete(int household, string id)
        {
            try
            {
                await table.DeleteItemAsync(household, id);
                return DynamoServiceResponse.ReturnSuccess("Deleted");
            }
            catch(Exception ex)
            {
                return DynamoServiceResponse.ReturnFailure(ex.Message);
            }
        }
    }
}

