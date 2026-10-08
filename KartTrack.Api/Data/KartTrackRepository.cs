using KartTrack.Api.Models;
using KartTrack.Api.Requests;
using Microsoft.EntityFrameworkCore;
using KartTrack.Api.Responses;

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

    // Get the personal best session for a specific driver on a specific track.
    public async Task<Session?> GetDriverPersonalBestAsync(
        int trackId, string driverName)
    {
        var name = driverName.Trim().ToLower();

        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.TrackId == trackId &&
                        s.DriverName.ToLower() == name)
            .OrderBy(s => s.FastestLap)
            .ThenBy(s => s.Id)
            .FirstOrDefaultAsync();
}

    // Get the leaderboard for a specific track, showing the best session for each driver.
    public async Task<List<Session>> GetTrackLeaderboardAsync(
        int trackId)
    {
        var sessions = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.TrackId == trackId)
            .ToListAsync();

        return sessions
            .GroupBy(
                s => s.DriverName.Trim(),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderBy(s => s.FastestLap)
                .ThenBy(s => s.Id)
                .First())
            .OrderBy(s => s.FastestLap)
            .ThenBy(s => s.DriverName)
            .ToList();
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

    // Get aggregated statistics for a specific driver across all sessions.
    public async Task<DriverStatsResponse?> GetDriverStatsAsync(
        string driverName)
    {
        var name = driverName.Trim().ToLower();

        var sessions = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.DriverName.ToLower() == name)
            .ToListAsync();

        if (sessions.Count == 0)
            return null;

        return new DriverStatsResponse
        {
            DriverName = sessions[0].DriverName,
            TotalSessions = sessions.Count,
            TotalLaps = sessions.Sum(s => s.TotalLaps),
            TracksVisited = sessions
                .Select(s => s.TrackId)
                .Distinct()
                .Count(),
            BestLap = sessions.Min(s => s.FastestLap),
            AverageFastestLap = Math.Round(
                sessions.Average(s => s.FastestLap), 2),
            AveragePosition = Math.Round(
                sessions.Average(s => (decimal)s.Position), 2),
            BestPosition = sessions.Min(s => s.Position)
        };
    }

    //return a driver's previus sessions, ordered by date and then by session ID.
    public async Task<List<Session>> GetDriverHistoryAsync(
        string driverName)
    {
        var name = driverName.Trim().ToLower();

        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.DriverName.ToLower() == name)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Id)
            .ToListAsync();
    }



}
