using System.Text.Json;

namespace AutomationFramework
{
    public static class JsonSerializerExtensions
    {
        public static T FromJson<T>(this string input, JsonSerializerOptions options = default) where T : class =>
            JsonSerializer.Deserialize<T>(input, options);

        public static string ToJson<T>(this T input, JsonSerializerOptions options = default) where T : class =>
            JsonSerializer.Serialize<T>(input, options);

        public static T ToAnonymousType<T>(string json, T anonymousTypeObject, JsonSerializerOptions options = default)
            => JsonSerializer.Deserialize<T>(json, options);
    }
}
