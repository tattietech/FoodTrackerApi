using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using foodTrackerApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foodTrackerApi.Models
{
    public class FoodItem : DynamoBaseModel
    {
        [DynamoDBProperty("name")]
        public string Name { get; set; }

        [DynamoDBProperty("expiry")]
        public string? Expiry { get; set; }

        [DynamoDBProperty("bestbefore")]
        public string? BestBefore { get; set; }

        [DynamoDBProperty("quantity")]
        public int Quantity { get; set; }

        [DynamoDBProperty("storageid")]
        public string StorageId { get; set; }

        public static string Identifier => "food-item";
    }
}
