using NSSLServer.Core.Extension;

namespace NSSLServer.Plugin.Recipes;

/// <inheritdoc/>
public class RecipePlugin : IPlugin
{
    /// <inheritdoc/>
    public string Name { get; } = nameof(RecipePlugin);
}