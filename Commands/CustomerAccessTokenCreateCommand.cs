using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using Storefront.Cli.Commands.Base;

namespace Storefront.Cli.Commands;

public class CustomerAccessTokenCreateCommand : BaseCommand<AccessTokenRequest, CustomerAccessToken>
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

    protected override async Task<CustomerAccessToken> DoExecuteAsync(AccessTokenRequest request)
    {
        var builder = new MutationQueryBuilder()
            .WithCustomerAccessTokenCreate(
                new CustomerAccessTokenCreatePayloadQueryBuilder().WithCustomerAccessToken(new CustomerAccessTokenQueryBuilder()
                    .WithAccessToken()
                    .WithExpiresAt()
                ),
                new CustomerAccessTokenCreateInput { Email = request.LoginId, Password = request.Password }
            );

        var mutation = builder.Build(Formatting.Indented);
        _logger.LogDebug(mutation);

        var response = await _client.SendMutationAsync<Mutation>(new GraphQLRequest(mutation));

        return response.Data.CustomerAccessTokenCreate.CustomerAccessToken;
    }

    #endregion
}
public record AccessTokenRequest(string LoginId, string Password); 

