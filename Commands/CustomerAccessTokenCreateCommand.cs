using GraphQL;
using GraphQL.Client.Abstractions;

namespace Storefront.Cli.Commands;

public class CustomerAccessTokenCreateCommand
{
    #region Fields

    private readonly IGraphQLClient _client;

    #endregion

    #region Constructors

    public CustomerAccessTokenCreateCommand(IGraphQLClient client) { _client = client; }

    #endregion

    #region Methods

    public async Task<CustomerAccessToken> Execute(string email, string password)
    {
        var request = new GraphQLRequest
        {
            Query = @"mutation customerAccessTokenCreate($input: CustomerAccessTokenCreateInput!) {
                  customerAccessTokenCreate(input: $input) {
                    customerAccessToken {
                        accessToken
                        expiresAt
                    }
                    customerUserErrors {
                      code
                      message
                    }
                  }
                }",
            OperationName = "customerAccessTokenCreate",
            Variables = new { input = new { email, password } }
        };

        var response = await _client.SendMutationAsync<Mutation>(request);
        
        return response.Data.CustomerAccessTokenCreate.CustomerAccessToken;
    }

    #endregion
}