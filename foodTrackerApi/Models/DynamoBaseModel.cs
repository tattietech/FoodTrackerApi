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
    [DynamoDBTable("food-tracker-db")]
    public class DynamoBaseModel
    {
        [DynamoDBHashKey("household")]
        public int Household { get; set; }

        [DynamoDBProperty("id")]
        public string Id { get; set; }
    }
}
