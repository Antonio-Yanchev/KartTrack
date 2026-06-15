using KartTrack.Api.Data;
using KartTrack.Api.Requests;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddSingleton<KartTrackRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/tracks", (KartTrackRepository repository) =>
{
    return Results.Ok(repository.GetTracks());
});

app.MapGet("/tracks/{id}", (int id, KartTrackRepository repository) =>
{
    var track = repository.GetTrackById(id);

    if (track is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(track);
});

app.MapGet("/sessions", (KartTrackRepository repository) =>
{
    return Results.Ok(repository.GetSessions());
});

app.MapGet("/sessions/{id}", (int id, KartTrackRepository repository) =>
{
    var session = repository.GetSessionById(id);

    if (session is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(session);
});

app.MapPost("/sessions", (CreateSessionRequest request, KartTrackRepository repository) =>
{
    var session = repository.CreateSession(request);

    if (session is null)
    {
        return Results.BadRequest("Track does not exist.");
    }

    return Results.Created($"/sessions/{session.Id}", session);
});

app.MapGet("/tracks/{trackId}/personal-best", (int trackId, KartTrackRepository repository) =>
{
    var bestSession = repository.GetPersonalBestByTrackId(trackId);

    if (bestSession is null)
    {
        return Results.NotFound("No sessions found for this track.");
    }

    return Results.Ok(bestSession);
});

app.MapPost("/sessions", (CreateSessionRequest request, KartTrackRepository repository) =>
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

    var session = repository.CreateSession(request);

    if (session is null)
    {
        return Results.BadRequest("Track does not exist.");
    }

    return Results.Created($"/sessions/{session.Id}", session);
});

app.Run();