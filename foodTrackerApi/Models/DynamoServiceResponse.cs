using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foodTrackerApi.Models
{
    public class DynamoServiceResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public int StatusCode { get; set; }

        public static DynamoServiceResponse ReturnSuccess(string message)
        {
            return new DynamoServiceResponse
            {
                Success = true,
                Message = message
            };
        }

        public static DynamoServiceResponse ReturnFailure(string message)
        {
            return new DynamoServiceResponse
            {
                Success = false,
                Message = message
            };
        }
    }
}
