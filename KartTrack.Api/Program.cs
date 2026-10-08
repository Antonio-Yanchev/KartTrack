using KartTrack.Api.Data;
using KartTrack.Api.Requests;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddDbContext<KartTrackDbContext>(options =>
    options.UseSqlite("Data Source=karttrack.db"));

builder.Services.AddScoped<KartTrackRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

    app.MapGet("/", () => Results.Ok(new
    {
        Application = "KartTrack API",
        Status = "Running",
        Version = "1.0"
    }));

    app.MapGet("/tracks",async (KartTrackRepository repository) =>
    {
        return Results.Ok(await repository.GetTracksAsync());
    });

    app.MapGet("/tracks/{id}", async (int id, KartTrackRepository repository) =>
    {
        var track = await repository.GetTrackByIdAsync(id);

        if (track is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(track);
    });

    app.MapGet("/sessions", async (KartTrackRepository repository) =>
    {
        return Results.Ok(await repository.GetSessionsAsync());
    });

    app.MapGet("/sessions/{id}", async (int id, KartTrackRepository repository) =>
    {
        var session = await repository.GetSessionByIdAsync(id);

        if (session is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(session);
    });

    app.MapGet("/tracks/{trackId}/personal-best", async (int trackId, KartTrackRepository repository) =>
    {
        var bestSession = await repository.GetPersonalBestByTrackIdAsync(trackId);

        if (bestSession is null)
        {
            return Results.NotFound("No sessions found for this track.");
        }

        return Results.Ok(bestSession);
    });

    app.MapPost("/sessions", async (CreateSessionRequest request, KartTrackRepository repository) =>
    {
        if (string.IsNullOrWhiteSpace(request.DriverName))
        {
            return Results.BadRequest("Driver name is required.");
        }

        if (request.TrackId <= 0)
        {
            return Results.BadRequest("Track ID must be valid.");
        }

        if (request.FastestLap <= 0)
        {
            return Results.BadRequest("Fastest lap must be greater than 0.");
        }

        if (request.AverageLap <= 0)
        {
            return Results.BadRequest("Average lap must be greater than 0.");
        }

        if (request.AverageLap < request.FastestLap)
        {
            return Results.BadRequest("Average lap cannot be faster than fastest lap.");
        }

        if (request.TotalLaps <= 0)
        {
            return Results.BadRequest("Total laps must be greater than 0.");
        }

        if (request.KartNumber <= 0)
        {
            return Results.BadRequest("Kart number must be greater than 0.");
        }

        if (request.Position <= 0)
        {
            return Results.BadRequest("Position must be greater than 0.");
        }

        var session = await repository.CreateSessionAsync(request);

        if (session is null)
        {
            return Results.BadRequest("Track does not exist.");
        }

        return Results.Created($"/sessions/{session.Id}", session);
    });


    app.MapPut("/sessions/{id}", async (
        int id,
        UpdateSessionRequest request,
        KartTrackRepository repository) =>
    {
        // Validate the request.
        if (string.IsNullOrWhiteSpace(request.DriverName))
            return Results.BadRequest("Driver name is required.");

        if (request.TrackId <= 0)
            return Results.BadRequest("Track ID must be valid.");

        if (request.FastestLap <= 0)
            return Results.BadRequest("Fastest lap must be greater than 0.");

        if (request.AverageLap <= 0)
            return Results.BadRequest("Average lap must be greater than 0.");

        if (request.AverageLap < request.FastestLap)
            return Results.BadRequest(
                "Average lap cannot be faster than fastest lap.");

        if (request.TotalLaps <= 0)
            return Results.BadRequest("Total laps must be greater than 0.");

        if (request.KartNumber <= 0)
            return Results.BadRequest("Kart number must be greater than 0.");

        if (request.Position <= 0)
            return Results.BadRequest("Position must be greater than 0.");

        // Check that the session exists.
        var existingSession = await repository.GetSessionByIdAsync(id);

        if (existingSession is null)
            return Results.NotFound("Session not found.");

        // Check that the referenced track exists.
        var track = await repository.GetTrackByIdAsync(request.TrackId);

        if (track is null)
            return Results.BadRequest("Track does not exist.");

        // Save changes to SQLite.
        var updatedSession = await repository.UpdateSessionAsync(id, request);

        if (updatedSession is null)
            return Results.NotFound("Session not found.");

        return Results.Ok(updatedSession);
    });


    app.MapDelete("/sessions/{id}", async (
        int id,
        KartTrackRepository repository) =>
    {
        var deleted = await repository.DeleteSessionAsync(id);

        if (!deleted)
            return Results.NotFound("Session not found.");

        return Results.NoContent();
    });

    // Get the personal best session for a specific driver at a specific track.
    app.MapGet("/tracks/{trackId}/personal-best/{driverName}",
        async (
            int trackId,
            string driverName,
            KartTrackRepository repository) =>
    {
        if (string.IsNullOrWhiteSpace(driverName))
            return Results.BadRequest("Driver name is required.");

        var track = await repository.GetTrackByIdAsync(trackId);

        if (track is null)
            return Results.NotFound("Track not found.");

        var personalBest = await repository
            .GetDriverPersonalBestAsync(trackId, driverName);

        if (personalBest is null)
            return Results.NotFound(
                "No sessions found for this driver at this track.");

        return Results.Ok(personalBest);
    });

    //returns a clean leaderboard json response rather than exposing every property of every session
    app.MapGet("/tracks/{trackId}/leaderboard",
        async (
            int trackId,
            KartTrackRepository repository) =>
    {
        var track = await repository.GetTrackByIdAsync(trackId);

        if (track is null)
            return Results.NotFound("Track not found.");

        var sessions = await repository
            .GetTrackLeaderboardAsync(trackId);

        var leaderboard = sessions
            .Select((session, index) => new
            {
                Rank = index + 1,
                DriverName = session.DriverName,
                FastestLap = session.FastestLap,
                SessionId = session.Id,
                KartNumber = session.KartNumber,
                Date = session.Date
            })
            .ToList();

        return Results.Ok(leaderboard);
    });

    // Get the overall stats for a specific driver across all tracks. (needs to be changed later to return best overall stats for a driver for a specific track, not all tracks).
    app.MapGet("/drivers/{driverName}/stats", async (
        string driverName,
        KartTrackRepository repository) =>
    {
        if (string.IsNullOrWhiteSpace(driverName))
            return Results.BadRequest("Driver name is required.");

        var stats = await repository.GetDriverStatsAsync(driverName);

        if (stats is null)
            return Results.NotFound(
                "No sessions found for this driver.");

        return Results.Ok(stats);
    });


    app.MapGet("/drivers/{driverName}/history", async (
        string driverName,
        KartTrackRepository repository) =>
    {
        if (string.IsNullOrWhiteSpace(driverName))
            return Results.BadRequest("Driver name is required.");

        var history = await repository.GetDriverHistoryAsync(
            driverName);

        if (history.Count == 0)
            return Results.NotFound(
                "No sessions found for this driver.");

        return Results.Ok(history);
    });



app.Run();

public partial class Program { }