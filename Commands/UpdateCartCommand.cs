using GraphQL.Client.Abstractions;

namespace Storefront.Cli.Commands;

public class UpdateCartCommand
{
    #region Fields

    private readonly IGraphQLClient _client;

    #endregion


    #region Constructors

    public UpdateCartCommand(IGraphQLClient client) { _client = client; }

    #endregion

    #region Methods

    public async Task ExecuteAsync(string cartId, string customerAccessToken = "79454d3436f8c527cbfb0ecc59528d0c")
    {
        var mutation = new MutationQueryBuilder()
            .WithCartSelectedDeliveryOptionsUpdate(
                new CartSelectedDeliveryOptionsUpdatePayloadQueryBuilder(),
                cartId,
                new List<CartSelectedDeliveryOptionInput>
                {
                    new()
                    {
                        DeliveryOptionHandle = "" 
                    }
                }
            );
        //
        // var mutation = new MutationQueryBuilder()
        //     .WithCartBuyerIdentityUpdate(
        //         new CartBuyerIdentityUpdatePayloadQueryBuilder().WithCart(
        //             new CartQueryBuilder().WithBuyerIdentity(
        //                 new CartBuyerIdentityQueryBuilder().WithDeliveryAddressPreferences(
        //                     new DeliveryAddressQueryBuilder().WithMailingAddressFragment(new MailingAddressQueryBuilder().WithId()
        //                         .WithAddress1()
        //                         .WithName()
        //                     )
        //                 )
        //             )
        //         ),
        //         cartId, new CartBuyerIdentityInput
        //         {
        //             DeliveryAddressPreferences = new List<DeliveryAddressInput>
        //             {
        //                 new()
        //                 {
        //                     DeliveryAddress = new MailingAddressInput
        //                     {
        //                         Country = "TW",
        //                         Address1 = "123 Main St",
        //                         FirstName = "John"
        //                     }
        //                 }
        //             }
        //         }
        //     );

        var query = mutation.Build(Formatting.Indented);

        var response = await _client.SendMutationAsync<Mutation>(query);

        Console.WriteLine(response.Data);
    }

    #endregion
}