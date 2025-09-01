using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;

namespace ContractViewerApi;

class AllowAuthPerUserPolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext ctx, CancellationToken _)
    {
        var req = ctx.HttpContext.Request;
        if (!HttpMethods.IsGet(req.Method) && !HttpMethods.IsHead(req.Method))
        {
            ctx.EnableOutputCaching = false;
            return ValueTask.CompletedTask;
        }

        // Allow caching for authenticated requests
        ctx.EnableOutputCaching = true;
        ctx.AllowCacheLookup  = true;
        ctx.AllowCacheStorage = true;
        ctx.AllowLocking      = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext _, CancellationToken __)
        => ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext ctx, CancellationToken _)
    {
        var res = ctx.HttpContext.Response;

        // Keep the safe defaults
        if (!StringValues.IsNullOrEmpty(res.Headers.SetCookie) ||
            res.StatusCode != StatusCodes.Status200OK)
        {
            ctx.AllowCacheStorage = false;
        }
        return ValueTask.CompletedTask;
    }
}