using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;

namespace Storefront.Cli.Commands;

public class CustomerQuery : BaseQuery<string, Customer>
{
    #region Fields

    private readonly ILogger<CustomerQuery> _logger;
    private readonly IGraphQLClient _client;

    #endregion

    #region Constructors

    public CustomerQuery(IGraphQLClient client, ILogger<CustomerQuery> logger)
    {
        _client = client;
        _logger = logger;
    }

    #endregion

    #region Methods

    protected override async Task<Customer> DoQueryAsync(string request)
    {
        var orderQueryBuilder = new OrderQueryBuilder()
            .WithId()
            .WithStatusUrl()
            .WithOrderNumber()
            .WithProcessedAt();

        var orderBuilder = new OrderConnectionQueryBuilder()
            .WithEdges(new OrderEdgeQueryBuilder().WithNode(orderQueryBuilder));

        var checkoutQueryBuilder = new CheckoutQueryBuilder()
            .WithId()
            .WithOrder(orderQueryBuilder)
            .WithWebUrl();

        var customerQueryBuilder = new CustomerQueryBuilder()
            .WithId()
            .WithEmail()
            .WithDisplayName()
            .WithFirstName()
            .WithLastName()
            .WithNumberOfOrders()
            .WithOrders(orderBuilder, 10)
            .WithLastIncompleteCheckout(checkoutQueryBuilder);

        var builder = new QueryRootQueryBuilder()
            .WithCustomer(customerQueryBuilder, request);

        var queryString = builder.Build(Formatting.Indented);
        _logger.LogDebug(queryString);

        var response = await _client.SendQueryAsync<QueryRoot>(
            new GraphQLRequest(queryString, new { customerAccessToken = request })
        );

        return response.Data.Customer;
    }

    #endregion
}