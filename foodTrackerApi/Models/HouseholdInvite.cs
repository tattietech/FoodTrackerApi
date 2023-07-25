using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foodTrackerApi.Models
{
    public class HouseholdInvite : DynamoBaseModel
    {
        [DynamoDBProperty("email")]
        public string Email { get; set; }

        [DynamoDBProperty("accepted")]
        public bool Accepted { get; set; }

        [DynamoDBProperty("from")]
        public User From { get; set; }

        public static string Identifier => "household-invite";
    }
}
