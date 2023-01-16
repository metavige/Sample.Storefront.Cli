using GraphQL;
using GraphQL.Client.Abstractions;

namespace Storefront.Cli.Commands;

public class CustomerQuery
{
    #region Fields

    private readonly IGraphQLClient _client;

    #endregion

    #region Constructors

    public CustomerQuery(IGraphQLClient client) { _client = client; }

    #endregion

    #region Methods

    public async Task<Customer> Execute(string accessToken)
    {
        var builder = new QueryRootQueryBuilder()
            .WithCustomer(
                new CustomerQueryBuilder()
                    .WithId()
                    .WithEmail()
                    .WithDisplayName()
                    .WithFirstName()
                    .WithLastName()
                    .WithNumberOfOrders(),
                accessToken
            );

        var queryString = builder.Build();
        Console.Write(queryString);

        var request = new GraphQLRequest(queryString, new { customerAccessToken = accessToken });
        var response = await _client.SendQueryAsync<QueryRoot>(request);

        return response.Data.Customer;
    }

    #endregion
}
 