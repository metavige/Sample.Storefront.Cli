using System.Text.Json;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using Storefront.Cli.Commands;

const string SHOP_ID = "eat-your-own-dog-food";
const string API_VERSION = "2023-01";

#region Initialize

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("System", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("GraphQL", LogEventLevel.Error)
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

#endregion

#region Builder

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
            EndPoint = new Uri($"https://{SHOP_ID}.myshopify.com/api/{API_VERSION}/graphql.json")
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

#endregion

#region 執行步驟
// ===================================
// 以下為執行的程式碼
// ===================================

// // 1. 使用者先登入 (使用帳號密碼，取得 Access Token)
var username = AnsiConsole.Ask<string>("登入帳號:");
var password = AnsiConsole.Prompt(new TextPrompt<string>("密碼:").PromptStyle("red").Secret());

var accessTokenCommand = provider.GetRequiredService<CustomerAccessTokenCreateCommand>();
var token = await accessTokenCommand.ExecuteAsync(new AccessTokenRequest(username, password));

// 2. 使用者查詢產品 (查詢產品資訊)
var first = 10;
var prodQuery = provider.GetRequiredService<ProductionQuery>();
var prods = await prodQuery.ExecuteAsync(first);

var choiceProds = AnsiConsole.Prompt(
    new MultiSelectionPrompt<string>()
        .Title("要購買的項目?")
        .PageSize(first)
        .MoreChoicesText("[grey](上下移動選擇)[/]")
        .AddChoices(prods.Edges.Select(p => p.Node.Title))
);

var productIds = prods.Edges
    .Where(p => choiceProds.Contains(p.Node.Title))
    .Select(p => p.Node.Variants.Edges.First().Node.Id);

// 3. 使用者加入購物車 (加入購物車)
var cartCreateCommand = provider.GetRequiredService<CreateCartCommand>();
var cart = await cartCreateCommand.ExecuteAsync(new CreateCartRequest { ProductIds = productIds, AccessToken = token.AccessToken });

// 4. 設定使用者資訊 (取貨人名稱，取貨地址)
var firstName = AnsiConsole.Ask<string>("取貨人名稱:");
var address = AnsiConsole.Ask<string>("收貨地址:");

var updateCartCommand = provider.GetRequiredService<UpdateCartCommand>();
await updateCartCommand.ExecuteAsync(cart.Id, token.AccessToken, "Taiwan", firstName, address);

// 5. TODO: 設定付款方式 (信用卡)

// 6. 建立購物車，取得 Checkout Url
var customerQuery = provider.GetRequiredService<CustomerQuery>();
var customer = await customerQuery.QueryAsync(token.AccessToken);

AnsiConsole.WriteLine("請點選以下連結進行付款: ");
AnsiConsole.WriteLine(customer.LastIncompleteCheckout.WebUrl.ToString());

AnsiConsole.WriteLine("\n(等待付款完成.....)\n");

// 7. 使用者前往 Checkout Url 付款 (等待 hook 回傳付款結果)
while (true)
{
    Thread.Sleep(TimeSpan.FromSeconds(5));
    // 等待處理
    customer = await customerQuery.QueryAsync(token.AccessToken);

    if (customer.LastIncompleteCheckout.Order != null)
    {
        break;
    }
}

// 8. TODO: 付款成功，取得訂單網址
AnsiConsole.WriteLine("[blue]狀態網址:[/]" + customer.Orders.Nodes.FirstOrDefault()?.StatusUrl.ToString());

#endregion