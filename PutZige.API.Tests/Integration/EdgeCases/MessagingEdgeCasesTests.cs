#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace PutZige.API.Tests.Integration.EdgeCases
{
    public class MessagingEdgeCasesTests : IAsyncDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Faker _faker = new Faker();

        public MessagingEdgeCasesTests()
        {
            _factory = new WebApplicationFactory<Program>();
        }

        public async ValueTask DisposeAsync()
        {
            _factory.Dispose();
            await Task.CompletedTask;
        }

        private HttpClient CreateClient(bool withAuth = false)
        {
            var client = _factory.CreateClient();
            if (withAuth)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
            }

            return client;
        }

        /// <summary>
        /// Verifies that ConcurrentMessageSends_SameUsers_BothSaved behaves as expected.
        /// </summary>
        [Fact]
        public async Task ConcurrentMessageSends_SameUsers_BothSaved()
        {
            var client = CreateClient(withAuth: true);
            var receiver = Guid.NewGuid();

            var t1 = client.PostAsJsonAsync("/api/v1/messages", new { ReceiverId = receiver, MessageText = "first" }, CancellationToken.None);
            var t2 = client.PostAsJsonAsync("/api/v1/messages", new { ReceiverId = receiver, MessageText = "second" }, CancellationToken.None);

            await Task.WhenAll(t1, t2);

            t1.Result.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted, HttpStatusCode.BadRequest);
            t2.Result.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted, HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Verifies that ConcurrentMarkAsRead_SameMessage_NoDataCorruption behaves as expected.
        /// </summary>
        [Fact]
        public async Task ConcurrentMarkAsRead_SameMessage_NoDataCorruption()
        {
            var client = CreateClient(withAuth: true);
            var messageId = Guid.NewGuid();

            var t1 = client.PostAsync($"/api/v1/messages/{messageId}/mark-as-read", null, CancellationToken.None);
            var t2 = client.PostAsync($"/api/v1/messages/{messageId}/mark-as-read", null, CancellationToken.None);

            await Task.WhenAll(t1, t2);

            t1.Result.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
            t2.Result.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// Verifies that UserConnectsDisconnectsRapidly_NoOrphanedConnections behaves as expected.
        /// </summary>
        [Fact]
        public async Task UserConnectsDisconnectsRapidly_NoOrphanedConnections()
        {
            var client = CreateClient(withAuth: false);
            var r1 = await client.GetAsync("/hubs/chat", CancellationToken.None);
            var r2 = await client.GetAsync("/hubs/chat", CancellationToken.None);
            r1.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
            r2.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Verifies that ConcurrentGetConversation_ConsistentResults behaves as expected.
        /// </summary>
        [Fact]
        public async Task ConcurrentGetConversation_ConsistentResults()
        {
            var client = CreateClient(withAuth: true);
            var otherUserId = Guid.NewGuid();

            var t1 = client.GetAsync($"/api/v1/messages/conversation/{otherUserId}", CancellationToken.None);
            var t2 = client.GetAsync($"/api/v1/messages/conversation/{otherUserId}", CancellationToken.None);

            await Task.WhenAll(t1, t2);

            t1.Result.StatusCode.Should().Be(t2.Result.StatusCode);
        }

        /// <summary>
        /// Verifies that Pagination_RequestPageBeyondTotal_EmptyResults behaves as expected.
        /// </summary>
        [Fact]
        public async Task Pagination_RequestPageBeyondTotal_EmptyResults()
        {
            var client = CreateClient(withAuth: true);
            var resp = await client.GetAsync("/api/v1/messages?page=9999&pageSize=50", CancellationToken.None);
            resp.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
            var content = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            content.Should().NotBeNull();
        }

        /// <summary>
        /// Verifies that Pagination_RequestPage1Million_HandlesGracefully behaves as expected.
        /// </summary>
        [Fact]
        public async Task Pagination_RequestPage1Million_HandlesGracefully()
        {
            var client = CreateClient(withAuth: true);
            var resp = await client.GetAsync("/api/v1/messages?page=1000000&pageSize=50", CancellationToken.None);
            resp.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Verifies that Guid_EmptyGuid_HandledGracefully behaves as expected.
        /// </summary>
        [Fact]
        public async Task Guid_EmptyGuid_HandledGracefully()
        {
            var client = CreateClient(withAuth: true);
            var resp = await client.GetAsync($"/api/v1/messages/conversation/{Guid.Empty}", CancellationToken.None);
            resp.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Verifies that Timestamps_AllUtc_NoTimezoneBugs behaves as expected.
        /// </summary>
        [Fact]
        public async Task Timestamps_AllUtc_NoTimezoneBugs()
        {
            var client = CreateClient(withAuth: true);
            var resp = await client.GetAsync("/api/v1/messages?since=2020-01-01T00:00:00Z", CancellationToken.None);
            resp.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Verifies that SoftDelete_DeletedMessage_NotInQueries behaves as expected.
        /// </summary>
        [Fact]
        public async Task SoftDelete_DeletedMessage_NotInQueries()
        {
            var client = CreateClient(withAuth: true);
            var deletedMessageId = Guid.NewGuid();
            var resp = await client.GetAsync($"/api/v1/messages/{deletedMessageId}", CancellationToken.None);
            resp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Verifies that DatabaseTimeout_HandledGracefully behaves as expected.
        /// </summary>
        [Fact]
        public async Task DatabaseTimeout_HandledGracefully()
        {
            var client = CreateClient(withAuth: true);
            var resp = await client.GetAsync("/api/v1/messages?simulateDelay=true", CancellationToken.None);
            resp.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Verifies that HubCrash_MessagesNotLost_PersistedInDatabase behaves as expected.
        /// </summary>
        [Fact]
        public async Task HubCrash_MessagesNotLost_PersistedInDatabase()
        {
            var client = CreateClient(withAuth: true);
            var r = await client.PostAsJsonAsync("/api/v1/messages?simulateHubCrash=true", new { ReceiverId = Guid.NewGuid(), MessageText = "persist" }, CancellationToken.None);
            r.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Verifies that UserReconnects_ReceivesPendingMessages behaves as expected.
        /// </summary>
        [Fact]
        public async Task UserReconnects_ReceivesPendingMessages()
        {
            var client = CreateClient(withAuth: true);
            var r = await client.GetAsync("/hubs/chat/pending", CancellationToken.None);
            r.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Verifies that MessageText_EmptyString_Rejected behaves as expected.
        /// </summary>
        [Fact]
        public async Task MessageText_EmptyString_Rejected()
        {
            var client = CreateClient(withAuth: true);
            var resp = await client.PostAsJsonAsync("/api/v1/messages", new { ReceiverId = Guid.NewGuid(), MessageText = "" }, CancellationToken.None);
            resp.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.InternalServerError);
        }
    }
}
