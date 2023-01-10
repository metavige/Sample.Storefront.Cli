using System.Globalization;
using System.Text.Json;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Storefront.Cli.Commands;

var apiToken = Environment.GetEnvironmentVariable("STOREFRONT_API_TOKEN");

if (string.IsNullOrEmpty(apiToken))
{
    AnsiConsole.WriteLine("[yello]Please set the STOREFRONT_API_TOKEN environment variable.[/]");
    // 離開系統
    return;
}

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

var email = AnsiConsole.Ask<string>("[green]Your email address?[/]");
var password = AnsiConsole.Ask<string>("[green]Password?[/]");

var command = provider.GetRequiredService<CustomerAccessTokenCreateCommand>();
var token = await command.Execute(email, password);

var query = provider.GetRequiredService<CustomerQuery>();
var customer = await query.Execute(token.AccessToken);

Console.WriteLine($"Customer: {customer.FirstName} {customer.LastName}");
