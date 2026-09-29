using Conectando.Api.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Conectando.Api.DTOs.Posts;

public class PatchPostRequest
{
    // El nombre se fija aca para que el DTO no dependa de como este
    // configurado el serializador: el contrato del API es camelCase.
    [JsonPropertyName("content")]
    [JsonConverter(typeof(OptionalStringConverter))]
    public Optional<string?> Content { get; set; } = new();

    [JsonPropertyName("privacy")]
    [JsonConverter(typeof(OptionalPrivacyConverter))]
    public Optional<PostPrivacy> Privacy { get; set; } = new();

    [JsonPropertyName("mediaIds")]
    [JsonConverter(typeof(OptionalGuidListConverter))]
    public Optional<List<Guid>> MediaIds { get; set; } = new();
}

/// <summary>
/// Lectura compartida de los <see cref="Optional{T}"/>.
/// Un null explicito tambien cuenta como "campo provisto": es la unica
/// forma de limpiar un campo con un PATCH.
/// </summary>
internal static class OptionalRead
{
    public static Optional<T> Read<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var result = new Optional<T> { HasValue = true, Value = default };

        if (reader.TokenType != JsonTokenType.Null)
        {
            result.Value = JsonSerializer.Deserialize<T>(ref reader, options);
        }

        return result;
    }
}

public sealed class OptionalStringConverter : JsonConverter<Optional<string?>>
{
    /// <summary>
    /// Imprescindible: sin esto System.Text.Json asigna null directo a la
    /// propiedad y nunca entra al conversor, y se pierde el PATCH de limpieza.
    /// </summary>
    public override bool HandleNull => true;

    public override Optional<string?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => OptionalRead.Read<string?>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, Optional<string?> value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Value, options);
}

public sealed class OptionalPrivacyConverter : JsonConverter<Optional<PostPrivacy>>
{
    public override bool HandleNull => true;

    public override Optional<PostPrivacy> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => OptionalRead.Read<PostPrivacy>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, Optional<PostPrivacy> value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Value, options);
}

public sealed class OptionalGuidListConverter : JsonConverter<Optional<List<Guid>>>
{
    public override bool HandleNull => true;

    public override Optional<List<Guid>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => OptionalRead.Read<List<Guid>>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, Optional<List<Guid>> value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Value, options);
}