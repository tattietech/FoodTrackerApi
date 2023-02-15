using Amazon.DynamoDBv2.DocumentModel;
using foodTrackerApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foodTrackerApi.Interfaces
{
    public interface IDynamoService
    {
        Task<DynamoServiceResponse> List(int household);

        Task<DynamoServiceResponse> Put(string item);

        Task<DynamoServiceResponse> Delete(int household, string id);
    }
}
