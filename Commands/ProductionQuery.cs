using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging; 

namespace Storefront.Cli.Commands;

public class ProductionQuery
{
    #region Fields

    private readonly ILogger<ProductionQuery> _logger;
    private readonly IGraphQLClient _client;

    #endregion

    #region Constructors

    public ProductionQuery(IGraphQLClient client, ILogger<ProductionQuery> logger)
    {
        _client = client;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task Execute()
    {
        var productConnection = new ProductConnectionQueryBuilder()
            .WithEdges(new ProductEdgeQueryBuilder()
                .WithNode(new ProductQueryBuilder()
                    .WithId()
                    .WithTitle()
                    .WithTotalInventory()
                    // .WithVariants(
                    //     new ProductVariantConnectionQueryBuilder()
                    //         .WithEdges(new ProductVariantEdgeQueryBuilder()
                    //             .WithNode(
                    //                 new ProductVariantQueryBuilder()
                    //                     .WithId()
                    //                     .WithPrice(new MoneyV2QueryBuilder().WithAmount().WithCurrencyCode())
                    //                     .WithTitle()
                    //                     .WithWeight()
                    //                     .WithWeightUnit()
                    //                     .WithAvailableForSale()
                    //             )
                    //         )
                    // )
                )
            );

        var query = new QueryRootQueryBuilder()
            .WithProducts(productConnection, 100)
            .Build();

        // Console.WriteLine(productConnection.Build());
        _logger.LogInformation(query);

        var request = new GraphQLRequest(query);
        
        var response = await _client.SendQueryAsync<ProductConnection>(request);

        Console.WriteLine(response.Data);
    }

    #endregion
}