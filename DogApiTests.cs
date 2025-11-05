using NUnit.Framework;
using RestSharp;
using FluentAssertions;
using System.Threading.Tasks;
using System.Net;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using System.Text.Json;
using Allure.Net.Commons;

namespace DogApiTests.Net
{
    [AllureNUnit]
    [AllureEpic("Dog API Tests")]
    public class DogApiTests
    {
        private RestClient _client;

        [OneTimeSetUp]
        public void Setup()
        {
            var options = new RestClientOptions("https://dog.ceo/api");
            
            _client = new RestClient(options);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }

        [Test]
        [AllureFeature("List All Breeds")]
        [AllureSeverity(SeverityLevel.blocker)]
        [AllureName("GET /breeds/list/all - Should return all dog breeds")]
        public async Task TestGetAllBreedsList_Success()
        {
            var request = new RestRequest("/breeds/list/all");

            var response = await _client.ExecuteGetAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (JsonDocument doc = JsonDocument.Parse(response.Content!))
            {
                JsonElement root = doc.RootElement;

                root.GetProperty("status").GetString().Should().Be("success");
                
                JsonElement message = root.GetProperty("message");
                message.ValueKind.Should().Be(JsonValueKind.Object, "message should be an object containing breeds");
                
                var breedsCount = message.EnumerateObject().Count();
                breedsCount.Should().BeGreaterThan(50, "API should return a large list of dog breeds");

                message.TryGetProperty("bulldog", out _).Should().BeTrue("bulldog should be in the breeds list");
                message.TryGetProperty("hound", out _).Should().BeTrue("hound should be in the breeds list");
                message.TryGetProperty("retriever", out _).Should().BeTrue("retriever should be in the breeds list");

                JsonElement australian = message.GetProperty("australian");
                australian.ValueKind.Should().Be(JsonValueKind.Array, "breeds with sub-breeds should have array values");
                australian.EnumerateArray().Should().Contain(x => x.GetString() == "shepherd", "australian should have shepherd sub-breed");

                JsonElement affenpinscher = message.GetProperty("affenpinscher");
                affenpinscher.ValueKind.Should().Be(JsonValueKind.Array);
                affenpinscher.EnumerateArray().Should().BeEmpty("affenpinscher has no sub-breeds");
            }
        }

        [Test]
        [AllureFeature("List All Breeds")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureName("GET /breeds/list/all - Invalid route scenario (404)")]
        public async Task TestGetAllBreedsList_Failure_NotFound()
        {
            var request = new RestRequest("/breeds/list/all/invalid-path");

            var response = await _client.ExecuteGetAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            using (JsonDocument doc = JsonDocument.Parse(response.Content!))
            {
                JsonElement root = doc.RootElement;
                root.GetProperty("status").GetString().Should().Be("error");
                root.GetProperty("code").GetInt32().Should().Be(404);
                root.GetProperty("message").GetString().Should().Be("No route found for \"GET http://dog.ceo/api/breeds/list/all/invalid-path\" with code: 0");
            }
        }

        [Test]
        [AllureFeature("Images by Breed")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureName("GET /breed/{breed}/images - Should return images for a breed")]
        public async Task TestGetImagesForBreed_Success()
        {
            string breed = "hound";

            var request = new RestRequest("/breed/{breed}/images").AddUrlSegment("breed", breed);

            var response = await _client.ExecuteGetAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (JsonDocument doc = JsonDocument.Parse(response.Content!))
            {
                JsonElement root = doc.RootElement;

                root.GetProperty("status").GetString().Should().Be("success");

                JsonElement message = root.GetProperty("message");
                message.ValueKind.Should().Be(JsonValueKind.Array, "message should be an array of image URLs");

                var imagesCount = message.EnumerateArray().Count();
                imagesCount.Should().BeGreaterThan(0, $"{breed} should have at least one image");

                var imageUrls = message.EnumerateArray().Select(x => x.GetString()).ToList();
                imageUrls.Should().OnlyContain(url => url!.StartsWith("https://images.dog.ceo/breeds/"), 
                    "all image URLs should start with the correct domain");

                imageUrls.Should().OnlyContain(url => url!.Contains($"breeds/{breed}"), 
                    $"all URLs should contain the breed '{breed}' in the path");

                imageUrls.Should().OnlyContain(url => url!.EndsWith(".jpg") || url!.EndsWith(".jpeg") || url!.EndsWith(".png"), 
                    "all URLs should point to valid image files");

                imageUrls.Should().OnlyHaveUniqueItems("image URLs should not contain duplicates");
            }
        }

        [Test]
        [AllureFeature("Images by Breed")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureName("GET /breed/{breed}/images - Invalid breed scenario")]
        public async Task TestGetImagesForBreed_Failure_BreedNotFound()
        {
            string breed = "nonexistentbreed";

            var request = new RestRequest("/breed/{breed}/images").AddUrlSegment("breed", breed);

            var response = await _client.ExecuteGetAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            using (JsonDocument doc = JsonDocument.Parse(response.Content!))
            {
                JsonElement root = doc.RootElement;
                root.GetProperty("status").GetString().Should().Be("error");
                root.GetProperty("code").GetInt32().Should().Be(404);
                root.GetProperty("message").GetString().Should().Be("Breed not found (main breed does not exist)");
            }
        }

        [Test]
        [AllureFeature("Random Image")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureName("GET /breeds/image/random - Should return a random image")]
        public async Task TestGetRandomImage_Success()
        {
            var request = new RestRequest("/breeds/image/random");

            var response = await _client.ExecuteGetAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (JsonDocument doc = JsonDocument.Parse(response.Content!))
            {
                JsonElement root = doc.RootElement;
                
                root.GetProperty("status").GetString().Should().Be("success");

                JsonElement message = root.GetProperty("message");
                message.ValueKind.Should().Be(JsonValueKind.String, "message should be a string containing the image URL");

                string imageUrl = message.GetString()!;

                imageUrl.Should().NotBeNullOrEmpty("image URL should not be empty");

                imageUrl.Should().StartWith("https://images.dog.ceo/breeds/", 
                    "image URL should start with the correct domain");

                (imageUrl.EndsWith(".jpg") || imageUrl.EndsWith(".jpeg") || imageUrl.EndsWith(".png"))
                    .Should().BeTrue("image URL should point to a valid image file (.jpg, .jpeg, or .png)");

                imageUrl.Should().Contain("breeds/", "image URL should contain the breeds path");
            }
        }

        [Test]
        [AllureFeature("Random Image")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureName("GET /breeds/image/random - Invalid path scenario")]
        public async Task TestGetRandomImage_Failure_NotFound()
        {
            var request = new RestRequest("/breeds/image/randomm");

            var response = await _client.ExecuteGetAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            
            using (JsonDocument doc = JsonDocument.Parse(response.Content!))
            {
                JsonElement root = doc.RootElement;
                root.GetProperty("status").GetString().Should().Be("error");
                root.GetProperty("code").GetInt32().Should().Be(404);
                root.GetProperty("message").GetString().Should().Be("No route found for \"GET http://dog.ceo/api/breeds/image/randomm\" with code: 0");
            }
        }
    }
}