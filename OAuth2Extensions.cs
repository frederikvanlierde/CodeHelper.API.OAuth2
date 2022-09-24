using System.Text.Json;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace CodeHelper.Core.OAuth2
{
    public static  class OAuth2Extensions
    {
        public static HttpContent GetJsonString(this object data)
        {
            var _j = JsonSerializer.Serialize<object>(data, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            return new StringContent(_j, System.Text.Encoding.UTF8, "application/json");
        }
    }
}
