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
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("GraphQL", LogEventLevel.Information)
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
            EndPoint = new Uri("https://eat-your-own-dog-food.myshopify.com/api/2023-01/graphql.json")
        };

        return new GraphQLHttpClient(options, serializer, httpClient);
    }
);

services.AddSingleton<CustomerAccessTokenCreateCommand>();
services.AddSingleton<CustomerQuery>();
services.AddSingleton<ProductionQuery>();
services.AddSingleton<CreateCartCommand>();
services.AddSingleton<UpdateCartCommand>();

var provider = services.BuildServiceProvider();

// // 1. 使用者先登入 (使用帳號密碼，取得 Access Token)
var username = AnsiConsole.Ask<string>("登入帳號:");
var password = AnsiConsole.Prompt(new TextPrompt<string>("密碼:").PromptStyle("red").Secret());

var accessTokenCommand = provider.GetRequiredService<CustomerAccessTokenCreateCommand>();
var token = await accessTokenCommand.Execute(username, password);

// 2. 使用者查詢產品 (查詢產品資訊)
var first = 10;
var prodQuery = provider.GetRequiredService<ProductionQuery>();
var prods = await prodQuery.Execute(first);

var choiceProds = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("要購買的項目?")
        .PageSize(first)
        .MoreChoicesText("[grey](上下移動選擇)[/]")
        .AddChoices(prods.Edges.Select(p => p.Node.Title)));

var productId = prods.Edges
    .Where(p => p.Node.Title == choiceProds)
    .Select(p => p.Node.Variants.Edges.First().Node.Id)
    .FirstOrDefault();
 
// 3. 使用者加入購物車 (加入購物車)
var cartCreateCommand = provider.GetRequiredService<CreateCartCommand>();
var cart = await cartCreateCommand.Execute(new [] { productId }, token.AccessToken);

// 4. TODO: 設定使用者資訊 (取貨人名稱，取貨地址)
// var updateCartCommand = provider.GetRequiredService<UpdateCartCommand>();
// await updateCartCommand.ExecuteAsync("gid://shopify/Cart/d7f5f32aaa5fb525332536d9e67676a5");

// 5. TODO: 設定付款方式 (信用卡)

// 6. 建立購物車，取得 Checkout Url
AnsiConsole.WriteLine("請點選以下連結進行付款: ");
AnsiConsole.WriteLine(cart.CheckoutUrl.ToString());

// 7. TODO: 使用者前往 Checkout Url 付款 (等待 hook 回傳付款結果)
// while (true)
// {   
//     // 等待處理   
// }

// 8. TODO: 付款成功，取得訂單網址

//
// var m = new MutationQueryBuilder()
//     .WithCartSelectedDeliveryOptionsUpdate(
//         new CartSelectedDeliveryOptionsUpdatePayloadQueryBuilder()
//             .WithCart(new CartQueryBuilder()
//                 .WithId()
//             ),
//         "gid://shopify/Cart/1db6f5a15344435f17d27639480a860c",
//         new List<CartSelectedDeliveryOptionInput>
//         {
//             new() { DeliveryOptionHandle = "" }
//         });
//         
// Console.WriteLine(m.Build(Formatting.Indented));