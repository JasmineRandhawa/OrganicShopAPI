using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrganicShopAPI;
using OrganicShopAPI.Constants;
using OrganicShopAPI.DataAccess;
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
        public async Task AddNewCategory_ExpectedCategoriesCountToBeOne()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var optionsBuilder = new DbContextOptionsBuilder<OrganicShopDbContext>();
            optionsBuilder.UseSqlServer(configurationBuilder["ConnectionStrings:DefaultConnection"]);

            var context = new OrganicShopDbContext(optionsBuilder.Options);

            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            //await context.Database.MigrateAsync();

            // Arrange
            var request = TestRoutes.AddCategory;
            Category category = new Category();
            category.Name = "Bakery";

            // Act
            var response = await Client.PostAsync(request, category.GetStringContent());

            var content = await response.Content.ReadAsStringAsync();
            var dbCategory = content.GetDeserializedObject<Category>();

            Assert.NotNull(response);
            Assert.Equal(response.StatusCode, (HttpStatusCode)StatusCodes.Status201Created);
            Assert.True(dbCategory.Id == 1);
            Assert.True(dbCategory.Name.Equals("Bakery"));
        }

        [Fact]
        public async Task GetAllCategories_WithoutFilter_ReturnsListOfAllCategories()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var optionsBuilder = new DbContextOptionsBuilder<OrganicShopDbContext>();
            optionsBuilder.UseSqlServer(configurationBuilder["ConnectionStrings:DefaultConnection"]);

            var context = new OrganicShopDbContext(optionsBuilder.Options);

            //await context.Database.EnsureDeletedAsync();
            //await context.Database.EnsureCreatedAsync();
            //await context.Database.MigrateAsync();

            // Arrange
            var request = TestRoutes.AllCategories;

            // Act
            var response = await Client.GetAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            var categories = content.GetDeserializedList<Category>();

            Assert.NotNull(response);
            Assert.Equal(response.StatusCode,(HttpStatusCode) StatusCodes.Status200OK);
            Assert.NotNull(categories);
            Assert.True(categories.Count() == 0);
        }
    }
}
