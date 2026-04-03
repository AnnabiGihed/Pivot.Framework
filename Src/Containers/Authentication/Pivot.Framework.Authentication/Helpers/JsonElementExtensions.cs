using System.Text.Json;

namespace Pivot.Framework.Authentication.Helpers;

internal static class JsonElementExtensions
{
	public static string? GetStringOrNull(this JsonElement element, string propertyName)
	{
		return element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
			? property.GetString()
			: null;
	}

	public static bool GetBooleanOrDefault(this JsonElement element, string propertyName, bool defaultValue = false)
	{
		return element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
			? property.GetBoolean()
			: defaultValue;
	}
}
