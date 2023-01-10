using System.Text.Json;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Storefront.Cli.Commands;

var apiToken = Environment.GetEnvironmentVariable("STOREFRONT_API_TOKEN");

var services = new ServiceCollection();
services.AddHttpClient();
services.AddSingleton<IGraphQLClient>(sp =>
    {
        var httpClient = sp.GetRequiredService<HttpClient>();

        httpClient.DefaultRequestHeaders.Add("X-Shopify-Storefront-Access-Token", apiToken);
        var serializer = new SystemTextJsonSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri("https://eat-your-own-dog-food.myshopify.com/api/2022-10/graphql.json")
        };

        return new GraphQLHttpClient(options, serializer, httpClient);
    }
);

services.AddSingleton<CustomerAccessTokenCreateCommand>();
services.AddSingleton<CustomerQuery>();

var provider = services.BuildServiceProvider();

var command = provider.GetRequiredService<CustomerAccessTokenCreateCommand>();
var token = await command.Execute("rickychiang+demo20221229@91app.com", "p@ssw0rd");

var query = provider.GetRequiredService<CustomerQuery>();
var customer = await query.Execute(token.AccessToken);

Console.WriteLine($"Customer: {customer.FirstName} {customer.LastName}");

// Console.WriteLine("Hello, World!");

// var mutation = new MutationQueryBuilder()
//     .WithCustomerAccessTokenCreate(
//         new CustomerAccessTokenCreatePayloadQueryBuilder()
//             .ExceptCustomerAccessToken()
//             .ExceptCustomerUserErrors(),
//         new CustomerAccessTokenCreateInput { Email = "rickychiang+demo20221229@91app.com", Password = "p@ssw0rd" }
//     ).Build(Formatting.Indented, 2);
//
// Console.WriteLine(mutation);

// var query = new CustomerQueryBuilder()
//     .ExceptId()
//     .ExceptEmail()
//     .ExceptFirstName()
//     .ExceptLastName()