
using System.Net;
using System.Net.Http.Json;
using KartTrack.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KartTrack.Api.Tests;

public class SessionApiTests
{
   // Creates a fresh application and database for each test.
   private static (
      KartTrackWebApplicationFactory Factory,
      HttpClient Client) CreateTestApp()
   {
      var factory = new KartTrackWebApplicationFactory();
      var client = factory.CreateClient();

      using var scope = factory.Services.CreateScope();

      var db = scope.ServiceProvider
         .GetRequiredService<KartTrackDbContext>();

      db.Database.EnsureCreated();

      return (factory, client);
   }

   [Fact]
   public async Task GetHomepage_ReturnsOk()
   {
      var (factory, client) = CreateTestApp();
      using (factory)
      using (client)
      {
         var response = await client.GetAsync("/");

         Assert.Equal(
               HttpStatusCode.OK,
               response.StatusCode);
      }
   }

   [Fact]
   public async Task GetTracks_ReturnsSeededTracks()
   {
      var (factory, client) = CreateTestApp();
      using (factory)
      using (client)
      {
         var response = await client.GetAsync("/tracks");

         Assert.Equal(
               HttpStatusCode.OK,
               response.StatusCode);

         var tracks = await response.Content
               .ReadFromJsonAsync<List<
                  KartTrack.Api.Models.Track>>();

         Assert.NotNull(tracks);
         Assert.Equal(2, tracks.Count);
      }
   }

   [Fact]
   public async Task CreateSession_WithValidData_ReturnsCreated()
   {
      var (factory, client) = CreateTestApp();
      using (factory)
      using (client)
      {
         var request = new
         {
               driverName = "Test Driver",
               trackId = 1,
               fastestLap = 42.5m,
               averageLap = 44.8m,
               totalLaps = 15,
               kartNumber = 7,
               position = 2
         };

         var response = await client.PostAsJsonAsync(
               "/sessions", request);

         Assert.Equal(
               HttpStatusCode.Created,
               response.StatusCode);

         // Verify that EF Core persisted the new session.
         using var scope = factory.Services.CreateScope();

         var db = scope.ServiceProvider
               .GetRequiredService<KartTrackDbContext>();

         Assert.True(await db.Sessions.AnyAsync(
               s => s.DriverName == "Test Driver"));
      }
   }

   [Fact]
   public async Task CreateSession_WithInvalidLap_ReturnsBadRequest()
   {
      var (factory, client) = CreateTestApp();
      using (factory)
      using (client)
      {
         var request = new
         {
               driverName = "Test Driver",
               trackId = 1,
               fastestLap = -5m,
               averageLap = 44.8m,
               totalLaps = 15,
               kartNumber = 7,
               position = 2
         };

         var response = await client.PostAsJsonAsync(
               "/sessions", request);

         Assert.Equal(
               HttpStatusCode.BadRequest,
               response.StatusCode);
      }
   }

   [Fact]
   public async Task DeleteNonexistentSession_ReturnsNotFound()
   {
      var (factory, client) = CreateTestApp();
      using (factory)
      using (client)
      {
         var response = await client.DeleteAsync(
               "/sessions/99999");

         Assert.Equal(
               HttpStatusCode.NotFound,
               response.StatusCode);
      }
   }
}
