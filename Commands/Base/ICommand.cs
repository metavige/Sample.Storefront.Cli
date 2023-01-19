namespace Storefront.Cli.Commands.Base;

public interface ICommand<TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request);
}

public abstract class BaseCommand<TRequest, TResponse> : ICommand<TRequest, TResponse>
{
    protected abstract Task<TResponse> DoExecuteAsync(TRequest request);

    public virtual async Task<TResponse> ExecuteAsync(TRequest request)
    {
        return await DoExecuteAsync(request);
    }
}