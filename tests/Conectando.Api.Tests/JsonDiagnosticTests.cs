using System.Text.Json;
using System.Text.Json.Serialization;

namespace Conectando.Api.Tests;

public class JsonDiagnosticTests
{
    [Fact]
    public void Converter_OnStructReadonlyInitField_IsApplied()
    {
        // El DTO local se llama "Content", asi que el JSON va en "Content":
        // con las opciones por defecto la comparacion distingue mayusculas.
        var result = JsonSerializer.Deserialize<Wrapper>("{\"Content\":\"nuevo\"}");
        Assert.True(result.Content.HasValue);
        Assert.Equal("nuevo", result.Content.Value);
    }

    private sealed class Wrapper
    {
        [JsonConverter(typeof(OptionalStringConverter))]
        public OptionalTune Content { get; init; }
    }

    private readonly struct OptionalTune
    {
        public OptionalTune(string? value) { Value = value; HasValue = true; }
        public string? Value { get; }
        public bool HasValue { get; }
    }

    private sealed class OptionalStringConverter : JsonConverter<OptionalTune>
    {
        public override OptionalTune Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return new OptionalTune();
            var value = JsonSerializer.Deserialize<string?>(ref reader, options);
            return new OptionalTune(value);
        }

        public override void Write(Utf8JsonWriter writer, OptionalTune value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value.Value, options);
    }
}