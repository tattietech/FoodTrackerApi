using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal.Transform;
using foodTrackerApi.Interfaces;
using foodTrackerApi.Models;
using foodTrackerApi.Services;
using foodTrackerAuth.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace foodTrackerApi;

public class Function
{
    private IDynamoService? _dynamoService;
    private HttpClient _client;
    
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request)
    {
        _client = new();
        var gotKey = request.Headers.TryGetValue("authorization", out var key);

        if (!gotKey || key == null)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Body = "Incorrect Key Provided"
            };
        }

        var path = request.RequestContext?.Http?.Path?.Split("/")?.LastOrDefault(string.Empty);
        var method = request.RequestContext?.Http?.Method;

        _dynamoService = path switch
        {
            "food" => new DynamoService<FoodItem>(FoodItem.Identifier),
            "storage" => new DynamoService<FoodStorage>(FoodStorage.Identifier),
            _ => null,
        };

        APIGatewayHttpApiV2ProxyResponse response = new()
        {
            Headers = new Dictionary<string, string>() { { "Content-Type", "application/json" }},
            Body = "Not Found",
            StatusCode = 404
        };

        if (_dynamoService is null)
            return response;

        DynamoServiceResponse serviceResponse = new();
        User user;
        switch (method)
        {
            case "GET":
                user = await GetUserInfo(key);

                if (user.Household == 0)
                    break;

                serviceResponse = await _dynamoService.List(user.Household);
                break;
            case "PUT":
                serviceResponse = await _dynamoService.Put(request.Body);
                break;
            case "DELETE":
                if(request.QueryStringParameters != null && request.QueryStringParameters.TryGetValue("id", out var id))
                {
                    user = await GetUserInfo(key);

                    if (user.Household == 0)
                        break;

                    serviceResponse = await _dynamoService.Delete(user.Household, id);
                }
                break;
        }

        if (serviceResponse != null)
        {
            response.Body = serviceResponse.Message;
            response.StatusCode = serviceResponse.Success ? 200 : 400;
        }

        return response; 
    }

    private async Task<User> GetUserInfo(string key)
    {
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://foodtracker.auth.eu-west-2.amazoncognito.com/oauth2/userInfo");
        userInfoRequest.Headers.Add("Authorization", key);

        var response = await _client.SendAsync(userInfoRequest);
        response.EnsureSuccessStatusCode();
        return JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
    }
}

