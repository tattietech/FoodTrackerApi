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
        Task<DynamoServiceResponse> List(string household, string? storageId = null);

        Task<DynamoServiceResponse> Put(string household, string item);

        Task<DynamoServiceResponse> Delete(string household, string id);

        Task<DynamoServiceResponse> DeleteStorage(string household, string storageId);
    }
}
