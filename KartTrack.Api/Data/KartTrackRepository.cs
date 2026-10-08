
using KartTrack.Api.Models;
using KartTrack.Api.Requests;
using Microsoft.EntityFrameworkCore;

namespace KartTrack.Api.Data;

public class KartTrackRepository
{
    private readonly KartTrackDbContext _context;

    public KartTrackRepository(KartTrackDbContext context)
    {
        _context = context;
    }

    public async Task<List<Track>> GetTracksAsync()
    {
        return await _context.Tracks
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Track?> GetTrackByIdAsync(int id)
    {
        return await _context.Tracks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Session>> GetSessionsAsync()
    {
        return await _context.Sessions
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Session?> GetSessionByIdAsync(int id)
    {
        return await _context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Session?> CreateSessionAsync(
        CreateSessionRequest request)
    {
        bool trackExists = await _context.Tracks
            .AnyAsync(t => t.Id == request.TrackId);

        if (!trackExists)
            return null;

        var session = new Session
        {
            TrackId = request.TrackId,
            DriverName = request.DriverName,
            Date = DateTime.UtcNow,
            FastestLap = request.FastestLap,
            AverageLap = request.AverageLap,
            TotalLaps = request.TotalLaps,
            KartNumber = request.KartNumber,
            Position = request.Position,
            Notes = request.Notes ?? ""
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<Session?> GetPersonalBestByTrackIdAsync(
        int trackId)
    {
        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.TrackId == trackId)
            .OrderBy(s => s.FastestLap)
            .FirstOrDefaultAsync();
    }


public async Task<Session?> UpdateSessionAsync(
    int id, UpdateSessionRequest request)
{
    var session = await _context.Sessions.FindAsync(id);

    if (session is null)
        return null;

    session.TrackId = request.TrackId;
    session.DriverName = request.DriverName;
    session.FastestLap = request.FastestLap;
    session.AverageLap = request.AverageLap;
    session.TotalLaps = request.TotalLaps;
    session.KartNumber = request.KartNumber;
    session.Position = request.Position;
    session.Notes = request.Notes ?? "";

    await _context.SaveChangesAsync();

    return session;
}


public async Task<bool> DeleteSessionAsync(int id)
{
    var session = await _context.Sessions.FindAsync(id);

    if (session is null)
        return false;

    _context.Sessions.Remove(session);
    await _context.SaveChangesAsync();

    return true;
}

}
