using System.Text.Json;

namespace Pivot.Framework.Authentication.Helpers;

internal static class JsonElementExtensions
{
    /// <summary>
    /// Tries to get the string value of the specified property from the <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static string? GetStringOrNull(this JsonElement element, string propertyName)
	{
		return element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
			? property.GetString()
			: null;
	}

    /// <summary>
    /// Tries to get the boolean value of the specified property from the <see cref="JsonElement"/>. If the property is not found or is not a boolean, returns the specified default value.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="propertyName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static bool GetBooleanOrDefault(this JsonElement element, string propertyName, bool defaultValue = false)
	{
		return element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
			? property.GetBoolean()
			: defaultValue;
	}
}