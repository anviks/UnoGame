using System.Text.Json;
using System.Text.Json.Serialization;

namespace Helpers;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions Options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        // WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
}