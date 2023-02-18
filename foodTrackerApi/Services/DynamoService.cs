using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using foodTrackerApi.Interfaces;
using foodTrackerApi.Models;
using Newtonsoft.Json;

namespace foodTrackerApi.Services
{
    public class DynamoService<T> : IDynamoService
    {
        protected string? tableName;
        protected AmazonDynamoDBClient client;
        protected DynamoDBContext context;
        private string identifier;
        private readonly Table table;

        public DynamoService(string identifier)
        {
            tableName = Environment.GetEnvironmentVariable("FOOD_TRACKER_TABLE_NAME");
            client = new AmazonDynamoDBClient();
            context = new DynamoDBContext(client);
            this.identifier = identifier;
            table = Table.LoadTable(client, "food-tracker-db");
        }

        private async Task<List<T>> ListItems<T>(string household, string? storageId = null)
        {
            QueryRequest request = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "householdId = :householdId AND begins_with(id, :idStart)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":householdId", new AttributeValue { S =  household.ToString()}},
                        {":idStart", new AttributeValue { S =  identifier} }
                    }
            };

            if (!string.IsNullOrEmpty(storageId))
            {
                request.FilterExpression = "storageid = :storageId";
                request.ExpressionAttributeValues.Add(":storageId", new AttributeValue { S = storageId });
            }

            var dbResponse = await client.QueryAsync(request);
            List<T> items = new();

            foreach (var item in dbResponse.Items)
            {
                var doc = Document.FromAttributeMap(item);
                var mappedItem = context.FromDocument<T>(doc);

                items.Add(mappedItem);
            }

            return items;
        }

        public async Task<DynamoServiceResponse> List(string household, string? storageId = null)
        {
            try
            {
                var items = await ListItems<T>(household, storageId);
                return DynamoServiceResponse.ReturnSuccess(JsonConvert.SerializeObject(items));
            }
            catch(Exception ex)
            {
                return DynamoServiceResponse.ReturnFailure(ex.Message);
            }
        }

        public async Task<DynamoServiceResponse> Put(string household, string item)
        {
            try
            {
                var itemObject = JsonConvert.DeserializeObject<T>(item);

                if (itemObject == null)
                    return DynamoServiceResponse.ReturnFailure("Item not formatted correctly");

                (itemObject as DynamoBaseModel).HouseholdId = household;

                if(string.IsNullOrEmpty((itemObject as DynamoBaseModel).Id))
                {
                    (itemObject as DynamoBaseModel).Id = $"{identifier}-{household}-{Guid.NewGuid()}";
                }

                var doc = context.ToDocument<T>(itemObject);

                await table.PutItemAsync(doc);

                return DynamoServiceResponse.ReturnSuccess(JsonConvert.SerializeObject(itemObject));
            }
            catch(Exception ex)
            {
                return DynamoServiceResponse.ReturnFailure(ex.Message);
            }
        }

        public async Task<DynamoServiceResponse> Delete(string household, string id)
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

        public async Task<DynamoServiceResponse> DeleteStorage(string household, string storageId)
        {
            try
            {
                var storageBatchDelete = context.CreateBatchWrite<FoodStorage>();

                identifier = "food";
                var foodItemsInStorage = await ListItems<FoodItem>(household, storageId);

                if(foodItemsInStorage != null && foodItemsInStorage.Count > 0)
                {
                    foreach (var item in foodItemsInStorage)
                    {
                        storageBatchDelete.AddDeleteKey(item.HouseholdId, item.Id);
                    }
                }

                await table.DeleteItemAsync(household, storageId);
                await storageBatchDelete.ExecuteAsync();

                return DynamoServiceResponse.ReturnSuccess("Deleted");
            }
            catch (Exception ex)
            {
                return DynamoServiceResponse.ReturnFailure(ex.Message);
            }
        }
    }
}

