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

    public async Task<ProductConnection> Execute(int first = 3)
    { 
        
        var productConnection = new ProductConnectionQueryBuilder()
            .WithEdges(new ProductEdgeQueryBuilder()
                .WithNode(new ProductQueryBuilder()
                    .WithId()
                    .WithTitle()
                    .WithHandle()
                    .WithTags()
                    // .WithTotalInventory()
                    .WithVariants(
                        new ProductVariantConnectionQueryBuilder()
                            .WithEdges(new ProductVariantEdgeQueryBuilder()
                                .WithNode(
                                    new ProductVariantQueryBuilder()
                                        .WithId()
                                        .WithPrice(new MoneyV2QueryBuilder().WithAmount())
                                        .WithAvailableForSale()
                                )
                            )
                    , first: 3)
                )
            );

        var query = new QueryRootQueryBuilder()
            .WithProducts(productConnection, first)
            .Build(Formatting.Indented);
 
        _logger.LogInformation(query);

        var request = new GraphQLRequest(query);
        
        var response = await _client.SendQueryAsync<QueryRoot>(request);

        _logger.LogInformation("{data}", response.Data); 
        
        return response.Data.Products;
    }

    #endregion
}