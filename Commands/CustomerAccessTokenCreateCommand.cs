using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;

namespace Storefront.Cli.Commands;

public class CustomerAccessTokenCreateCommand
{
    #region Fields

    private readonly ILogger<CustomerAccessTokenCreateCommand> _logger;
    private readonly IGraphQLClient _client;

    #endregion

    #region Constructors

    public CustomerAccessTokenCreateCommand(IGraphQLClient client, ILogger<CustomerAccessTokenCreateCommand> logger)
    {
        _client = client;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task<CustomerAccessToken> Execute(string email, string password)
    {
        var builder = new MutationQueryBuilder()
            .WithCustomerAccessTokenCreate(
                new CustomerAccessTokenCreatePayloadQueryBuilder().WithCustomerAccessToken(new CustomerAccessTokenQueryBuilder()
                    .WithAccessToken()
                    .WithExpiresAt()
                ),
                new CustomerAccessTokenCreateInput { Email = email, Password = password }
            );
 
        _logger.LogDebug(builder.Build(Formatting.Indented));

        var response = await _client.SendMutationAsync<Mutation>(new GraphQLRequest(builder.Build()));
        
        return response.Data.CustomerAccessTokenCreate.CustomerAccessToken;
    }

    #endregion
}