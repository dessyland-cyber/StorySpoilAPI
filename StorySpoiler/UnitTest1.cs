using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoiler
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient client;
        private static string createdStoryId="";
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("dessyland", "123123");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            this.client = new RestClient(options);
        }


        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });
            var response= loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Test, Order(1)]
        public void CreateCorrectNewStorySpoiler_ShouldReturnSuccessfullyCreated()
        {
            var story = new
            {
                Title = "New Title",
                Description = "New Description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create",Method.Post);
            request.AddJsonBody(story);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = json.GetProperty("storyId").GetString();
            Assert.That(createdStoryId, Is.Not.Null.And.Not.Empty, "storyId");
            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Test, Order(2)]

        public void EditStorySpoiler_ShouldReturnOK()
        {
            var changes = new StoryDTO
            {
                Title = "Edited Title",
                Description = "Edited Description",
                Url = ""

            };
            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
   
            request.AddJsonBody(changes);
            var response = client.Execute(request);

   
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));


        }
        [Test, Order(3)]
        public void GetAllStorySpoilers_ShouldReturnArray()
        {
            var request = new RestRequest("/api/Story/All/", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var spoilers = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(spoilers,Is.Not.Empty);

        }

        [Test, Order(4)]
        public void DeleteSpoiler_ShouldReturnOK()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}",Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode,Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));

        }

        [Test, Order(5)]
        public void CreateSpoilerWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var spoiler = new
            {
                Title = "",
                Description = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(spoiler);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingSpoiler()
        {
            string fakeId = "123";
            var editSpoiler = new StoryDTO
            {
                Title = "Non existing spoiler",
                Description = "Non existing description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddQueryParameter("id", fakeId);
            request.AddJsonBody(editSpoiler);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));


        }

        [Test, Order(7)]

        public void DeleteNonExistingSpoiler_ShouldReturnBadRequest()
        {
            string fakeId = "123";
            
            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
            request.AddQueryParameter("id", fakeId);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

       


        
        
        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}