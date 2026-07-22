using Microsoft.Extensions.DependencyInjection;
using Pva.Core;

namespace Pva.TextExpansion;

/// <summary>ثبت گسترش متن در DI.</summary>
public static class TextExpansionServiceCollectionExtensions
{
    public static IServiceCollection AddTextExpansion(this IServiceCollection services, string? storePath = null)
    {
        var store = new ExpansionStore(storePath);
        services.AddSingleton(store);
        services.AddSingleton<ITextExpander>(_ => new TextExpander(store.Load()));
        return services;
    }
}
