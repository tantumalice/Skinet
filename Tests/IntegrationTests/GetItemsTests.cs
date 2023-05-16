using API.DTO;
using API.Helpers;
using Core.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests
{
    public class GetItemsTests : IClassFixture<TestFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GetItemsTests(TestFactory<Program> factory)
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