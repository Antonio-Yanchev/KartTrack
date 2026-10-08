
using System.Data.Common;
using KartTrack.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KartTrack.Api.Tests;

public class KartTrackWebApplicationFactory
   : WebApplicationFactory<Program>
{
   protected override void ConfigureWebHost(
      IWebHostBuilder builder)
   {
      builder.UseEnvironment("Development");

      builder.ConfigureServices(services =>
      {
            // Remove the application's SQLite configuration.
            services.RemoveAll<
               DbContextOptions<KartTrackDbContext>>();

            services.RemoveAll<
               IDbContextOptionsConfiguration<KartTrackDbContext>>();

            // Keep an in-memory SQLite connection open.
            services.AddSingleton<DbConnection>(_ =>
            {
               var connection = new SqliteConnection(
                  "Data Source=:memory:");

               connection.Open();
               return connection;
            });

            // Register the test database context.
            services.AddDbContext<KartTrackDbContext>(
               (serviceProvider, options) =>
               {
                  var connection = serviceProvider
                        .GetRequiredService<DbConnection>();

                  options.UseSqlite(connection);
               });
      });
   }
}
