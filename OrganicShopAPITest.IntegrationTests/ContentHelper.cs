using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace OrganicShopAPITest.IntegrationTests
{
    public static class ContentHelper
    {
        public static IEnumerable<T> GetDeserializedList<T>(this string jsonString)
            => JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);

        public static StringContent GetStringContent(this object o)
       => new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");

        public static T GetDeserializedObject<T>(this string jsonString)
           => JsonConvert.DeserializeObject<T>(jsonString);
    }
}
