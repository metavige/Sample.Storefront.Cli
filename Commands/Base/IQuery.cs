namespace Storefront.Cli.Commands;

public interface IQuery<TRequest, TResponse>
{
    Task<TResponse> QueryAsync(TRequest request);
}

public abstract class BaseQuery<TRequest, TResponse> : IQuery<TRequest, TResponse>
{
    protected abstract Task<TResponse> DoQueryAsync(TRequest request);

    public async Task<TResponse> QueryAsync(TRequest request) { return await DoQueryAsync(request); }
}