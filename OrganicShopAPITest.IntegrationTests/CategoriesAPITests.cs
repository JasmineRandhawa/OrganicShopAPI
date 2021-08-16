using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using OrganicShopAPI;
using OrganicShopAPI.Constants;
using OrganicShopAPI.Models;

using Xunit;

namespace OrganicShopAPITest.IntegrationTests
{
    public class CategoriesAPITests : IClassFixture<TestFixture<Startup>>
    {
        private HttpClient Client;

        public CategoriesAPITests(TestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        [Fact]
        public async Task GetAllCategories_WithoutFilter_ReturnsListOfAllCategories()
        {
            // Arrange
            var request = TestRoutes.AllCategories;

            // Act
            var response = await Client.GetAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            var categories = content.GetDeserializedObject<Category>();

            Assert.NotNull(response);
            Assert.Equal(response.StatusCode,(HttpStatusCode) StatusCodes.Status200OK);
            Assert.NotNull(categories);
            Assert.True(categories.Count() > 0);
        }
    }
}
