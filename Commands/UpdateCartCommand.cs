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

    public async Task ExecuteAsync(
        string cartId,
        string customerAccessToken,
        string country = "TW",
        string address = "",
        string firstName = ""
    )
    {
        var queryBuilder = new CartBuyerIdentityUpdatePayloadQueryBuilder()
            .WithCart(
                new CartQueryBuilder()
                    .WithBuyerIdentity(
                        new CartBuyerIdentityQueryBuilder()
                            .WithDeliveryAddressPreferences(
                                new DeliveryAddressQueryBuilder()
                                    .WithMailingAddressFragment(
                                        new MailingAddressQueryBuilder()
                                            .WithId()
                                            .WithAddress1()
                                            .WithName()
                                    )
                            )
                    )
            );

        var input = new CartBuyerIdentityInput
        {
            DeliveryAddressPreferences = new List<DeliveryAddressInput>
            {
                new()
                {
                    DeliveryAddress = new MailingAddressInput
                    { 
                        Country = country,
                        Address1 = address,
                        FirstName = firstName
                    }
                }
            }
        };
        
        var mutation = new MutationQueryBuilder()
            .WithCartBuyerIdentityUpdate(queryBuilder, cartId, input);

        var query = mutation.Build(Formatting.Indented);
        var response = await _client.SendMutationAsync<Mutation>(query);

        Console.WriteLine(response.Data);
    }

    #endregion
}