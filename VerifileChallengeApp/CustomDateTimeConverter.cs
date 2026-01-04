using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VerifileChallengeApp;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd HH:mm:ss";  // Matches your format

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? dateStr = reader.GetString();
        if (dateStr == null) return default;

        return DateTime.ParseExact(dateStr, Format, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
