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

        var request = new GraphQLRequest
        {
            Query = queryString,
            Variables = new { customerAccessToken = accessToken }
        };
        var response = await _client.SendQueryAsync<CustomerQueryResponse>(request);

        return response.Data.Customer;
    }

    #endregion
}

class CustomerQueryResponse
{
    public Customer Customer { get; init; }
}