namespace OrchardCoreContrib.PoExtractor.DotNet;

/// <summary>
/// Represents a class that contains the names of the localized string types and their factory methods.
/// </summary>
public static class LocalizedStringTypes
{
    /// <summary>
    /// Gets the name of the type that represents a localized string.
    /// </summary>
    public static readonly string LocalizedStringTypeName = "LocalizedString";

    /// <summary>
    /// Gets the name of the type that represents a localized HTML string.
    /// </summary>
    public static readonly string LocalizedHtmlStringTypeName = "LocalizedHtmlString";

    /// <summary>
    /// Gets the name of the class that contains the LocalizedString factory method.
    /// </summary>
    public static readonly string LocalizedStringExtensionsTypeName = "LocalizedStringExtensions";

    /// <summary>
    /// Gets the name of the class that contains the LocalizedHtmlString factory method.
    /// </summary>
    public static readonly string LocalizedHtmlStringExtensionsTypeName = "LocalizedHtmlStringExtensions";

    /// <summary>
    /// Gets the name of the factory method that creates a localized string from its name.
    /// </summary>
    public static readonly string FactoryMethodName = "Create";

    /// <summary>
    /// Gets the names of the localized string types that can be created with a constructor.
    /// </summary>
    public static readonly string[] TypeNames =
    [
        LocalizedStringTypeName,
        LocalizedHtmlStringTypeName
    ];

    /// <summary>
    /// Gets the names of the types that contain the factory method.
    /// </summary>
    public static readonly string[] FactoryTypeNames =
    [
        LocalizedStringTypeName,
        LocalizedHtmlStringTypeName,
        LocalizedStringExtensionsTypeName,
        LocalizedHtmlStringExtensionsTypeName
    ];
}
