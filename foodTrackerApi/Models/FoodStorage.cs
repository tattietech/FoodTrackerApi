using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foodTrackerApi.Models
{
    public class FoodStorage : DynamoBaseModel
    {
        [DynamoDBProperty("name")]
        public string Name { get; set; }

        public static string Identifier => "food-storage";
    }
}
