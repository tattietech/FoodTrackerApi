using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using foodTrackerApi.Interfaces;
using foodTrackerApi.Models;
using foodTrackerApi.Services;
using Newtonsoft.Json;
using System.Net;

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
            "household" => new DynamoService<Household>(Household.Identifier),
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

                if (string.IsNullOrEmpty(user.HouseholdId))
                    break;

                if(path == "food")
                {
                    bool gotStorage = request.QueryStringParameters.TryGetValue("storageId", out var storageId);
                    if (!gotStorage)
                        break;

                    serviceResponse = await _dynamoService.List(user.HouseholdId, storageId);
                    break;
                }

                serviceResponse = await _dynamoService.List(user.HouseholdId);
                break;
            case "PUT":
                user = await GetUserInfo(key);

                if (string.IsNullOrEmpty(user.HouseholdId))
                    break;

                serviceResponse = await _dynamoService.Put(user.HouseholdId, request.Body);
                break;
            case "DELETE":
                if(request.QueryStringParameters != null && request.QueryStringParameters.TryGetValue("id", out var id))
                {
                    user = await GetUserInfo(key);

                    if (string.IsNullOrEmpty(user.HouseholdId))
                        break;

                    if(path == "storage")
                    {
                        serviceResponse = await _dynamoService.DeleteStorage(user.HouseholdId, id);
                        break;
                    }

                    serviceResponse = await _dynamoService.Delete(user.HouseholdId, id);
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
        var user = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());

        if (string.IsNullOrEmpty(user.HouseholdId))
        {
            var household = await AddHousehold(user);
            user.HouseholdId = household;
        }

        return user;
    }

    private async Task<string> AddHousehold(User user)
    {
        var id = $"household-{Guid.NewGuid()}";

        user.IsHouseholdAdmin= true;
        var household = new Household
        {
            Id = id,
            HouseholdId = id,
            Name = $"{user.GivenName}'s house",
            Users = new List<User>() { user }
        };

        AmazonCognitoIdentityProviderClient cognitoClient = new();
        AdminUpdateUserAttributesRequest request = new()
        {
            UserPoolId = "eu-west-2_xRpVWcjO2",
            Username = user.Username,
            UserAttributes = new List<AttributeType>() { new AttributeType() { Name = "custom:householdId", Value = household.Id } }
        };

        AdminUpdateUserAttributesResponse response = new();
        try
        {
            response = await cognitoClient.AdminUpdateUserAttributesAsync(request);
        }
        catch(Exception ex)
        {
            LambdaLogger.Log("ADDHOUSEHOLD EXCEPTION: " + ex.Message);
            return string.Empty;
        }

        IDynamoService houseService = new DynamoService<Household>(Household.Identifier);
        await houseService.Put(household.Id, JsonConvert.SerializeObject(household).ToString());

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            return household.Id;
        }
        else
        {
            return string.Empty;
        }
    }
}

