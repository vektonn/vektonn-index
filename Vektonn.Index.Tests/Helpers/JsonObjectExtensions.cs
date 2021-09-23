using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vektonn.Index.Tests.Helpers
{
    public static class JsonObjectExtensions
    {
        private static readonly JsonConverter[] Converters =
        {
            new StringEnumConverter()
        };

        public static string ToPrettyJson<T>(this T? o)
        {
            return JsonConvert.SerializeObject(o, Formatting.Indented, Converters);
        }
    }
}
