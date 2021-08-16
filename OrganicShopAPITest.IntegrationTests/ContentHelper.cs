using Newtonsoft.Json;
using System.Collections.Generic;

namespace OrganicShopAPITest.IntegrationTests
{
    public static class ContentHelper
    {
        public static IEnumerable<T> GetDeserializedObject<T>(this string jsonString)
            => JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);
    }
}
