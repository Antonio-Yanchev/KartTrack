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

app.MapGet("/", () => Results.Ok(new
{
    Application = "KartTrack API",
    Status = "Running",
    Version = "1.0"
}));

app.Run();