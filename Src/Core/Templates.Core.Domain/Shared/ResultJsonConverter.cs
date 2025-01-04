using System.Text.Json.Serialization;
using System.Text.Json;

namespace Templates.Core.Domain.Shared;

public class ResultJsonConverter<TValue> : JsonConverter<Result<TValue>>
{
	public override Result<TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException("Deserialization is not required.");
	}

	public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options)
	{
		if (value.IsSuccess)
		{
			JsonSerializer.Serialize(writer, value.Value, options);
		}
		else
		{
			JsonSerializer.Serialize(writer, new
			{
				value.IsSuccess,
				value.Error
			}, options);
		}
	}
}