using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using Storefront.Cli.Commands.Base;

namespace Storefront.Cli.Commands;

public class CreateCartCommand : BaseCommand<CreateCartRequest, Cart> 
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

    protected override async Task<Cart> DoExecuteAsync(CreateCartRequest request)
    { 
        var cartPayloadBuilder = new CartCreatePayloadQueryBuilder()
            .WithCart(
                new CartQueryBuilder()
                    .WithId()
                    .WithCheckoutUrl()
                    .WithCreatedAt()
                    .WithUpdatedAt()
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
                            ),
                        10
                    )
            );

        var cartInput = new CartInput
        {
            Note = "This is a note",
            Lines = request.ProductIds.Select(id => new CartLineInput { Quantity = 1, MerchandiseId = id })
                .ToList(),
            BuyerIdentity = new CartBuyerIdentityInput
            {
                CustomerAccessToken = request.AccessToken
            }
        };

        var builder = new MutationQueryBuilder()
            .WithCartCreate(cartPayloadBuilder, cartInput);

        _logger.LogDebug(builder.Build(Formatting.Indented));
 
        var response = await _client.SendMutationAsync<Mutation>(new GraphQLRequest(builder.Build()));

        return response.Data.CartCreate.Cart;
        // return null;
    }

    #endregion
}

public class CreateCartRequest
{
    public IEnumerable<string> ProductIds { get; init; }
    public string AccessToken { get; init; }
}