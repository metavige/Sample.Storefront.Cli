using System.Diagnostics;
using System.Text.Json;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using Storefront.Cli.Commands;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
    .MinimumLevel.Override("GraphQL", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var apiToken = Environment.GetEnvironmentVariable("STOREFRONT_API_TOKEN");

if (string.IsNullOrEmpty(apiToken))
{
    AnsiConsole.WriteLine("[yello]Please set the STOREFRONT_API_TOKEN environment variable.[/]");
    // 離開系統
    return;
}

var services = new ServiceCollection();
services.AddLogging(logger => logger.AddSerilog());
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
services.AddSingleton<ProductionQuery>();
services.AddSingleton<CreateCartCommand>();

var provider = services.BuildServiceProvider();

// var email = AnsiConsole.Ask<string>("[green]Your email address?[/]");
// var password = AnsiConsole.Ask<string>("[green]Password?[/]");
// //
// var accessTokenCommand = provider.GetRequiredService<CustomerAccessTokenCreateCommand>();
// var token = await accessTokenCommand.Execute(email, password);
//
// Log.Logger.Debug("Token: {Token}", token.AccessToken);

// var query = provider.GetRequiredService<CustomerQuery>();
// var customer = await query.Execute(token.AccessToken);
//
// Console.WriteLine($"Customer: {customer.FirstName} {customer.LastName}");

// TODO: Not working
var prodQuery = provider.GetRequiredService<ProductionQuery>();
prodQuery.Execute();

// TODO: Not working
var cartCreateCommand = provider.GetRequiredService<CreateCartCommand>();
cartCreateCommand.Execute();