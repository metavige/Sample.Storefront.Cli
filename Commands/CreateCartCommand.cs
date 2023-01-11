using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;

namespace Storefront.Cli.Commands;

public class CreateCartCommand
{
    #region Fields

    private readonly ILogger<CreateCartCommand> _logger;
    private readonly IGraphQLClient _client;

    #endregion

    #region Properties

    #endregion

    #region Constructors

    public CreateCartCommand(ILogger<CreateCartCommand> logger, IGraphQLClient client)
    {
        _logger = logger;
        _client = client;
    }

    #endregion

    #region Methods

    public async Task<Cart> Execute()
    {
        var moneyBuilder = new MoneyV2QueryBuilder().WithAmount().WithCurrencyCode();

        var cartPayloadBuilder = new CartCreatePayloadQueryBuilder()
            .WithCart(
                new CartQueryBuilder()
                    .WithId()
                    .WithCheckoutUrl()
                    .WithCreatedAt()
                    .WithUpdatedAt()
                    .WithAttributes(new AttributeQueryBuilder().WithKey().WithValue())
                    .WithCost(
                        new CartCostQueryBuilder()
                            .WithSubtotalAmount(moneyBuilder)
                            .WithTotalAmount(moneyBuilder)
                            .WithCheckoutChargeAmount(moneyBuilder)
                            .WithTotalTaxAmount(moneyBuilder)
                    )
                    .WithLines(
                        new CartLineConnectionQueryBuilder()
                            .WithEdges(
                                new CartLineEdgeQueryBuilder()
                                    .WithNode(
                                        new CartLineQueryBuilder()
                                            .WithMerchandise(
                                                new MerchandiseQueryBuilder()
                                                    .WithProductVariantFragment(
                                                        new ProductVariantQueryBuilder()
                                                            .WithId()
                                                    )
                                            )
                                    )
                            )
                    )
            );

        var cartInput = new CartInput();
        var lineInput = new CartLineInput { Quantity = 1, MerchandiseId = "gid://shopify/ProductVariant/171319787544" };

        cartInput.Note = "This is a note";
        cartInput.Lines = new List<CartLineInput> { lineInput };

        var builder = new MutationQueryBuilder()
            .WithCartCreate(cartPayloadBuilder, cartInput);

        // _logger.LogInformation(builder.Build(Formatting.Indented));

        var request = new GraphQLRequest(builder.Build());
        var response = await _client.SendMutationAsync<Mutation>(request);

        return response.Data.CartCreate.Cart;
        // return null;
    }

    #endregion
}