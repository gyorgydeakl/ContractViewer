using Common;
using ContractListApi;
using StackExchange.Redis;

namespace ContractViewerApi;

public class UserService(IHttpClientFactory clientFactory, IConnectionMultiplexer redis)
{
    public async Task<string> GetUserIdAsync(string username) =>
        await redis
            .GetDatabase()
            .TryGetDeserialized($"username/{username}", async () =>
            {
                return await clientFactory
                    .GetClient(Apis.User)
                    .GetAsync<string>($"user/{username}/userId");
            });
}

public class ContractListService(IHttpClientFactory clientFactory, IConnectionMultiplexer redis, UserService userService)
{
    public async Task<List<ContractSummary>> GetContractSummariesAsync(string username) =>
        await redis.GetDatabase()
            .TryGetDeserialized($"user/{username}/contracts", async () =>
            {
                var userId = await userService.GetUserIdAsync(username);
                return await clientFactory
                    .GetClient(Apis.ContractList)
                    .GetAsync<List<ContractSummary>>($"users/{userId}/contracts");
            });
}

public class ContractDetailsService(IHttpClientFactory clientFactory, IConnectionMultiplexer redis)
{
    public async Task<ContractDetails> GetContractDetailsAsync(string contractId) =>
        await redis.GetDatabase()
            .TryGetDeserialized($"contract/{contractId}", async () =>
            {
                return await clientFactory
                    .GetClient(Apis.ContractDetails)
                    .GetAsync<ContractDetails>($"contract/{contractId}");
            });
}