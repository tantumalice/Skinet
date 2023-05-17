using API.DTO;
using API.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace IntegrationTests
{
    public class GetProductsTest : IClassFixture<TestFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GetProductsTest(TestFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async void GetByDefault()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("api/products");

            response.EnsureSuccessStatusCode();

            var result = (Pagination<ProductToReturnDto>)await response.Content.ReadFromJsonAsync(typeof(Pagination<ProductToReturnDto>));

            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }
    }
}