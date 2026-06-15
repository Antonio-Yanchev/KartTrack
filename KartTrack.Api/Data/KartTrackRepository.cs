using KartTrack.Api.Models;
using KartTrack.Api.Requests;

namespace KartTrack.Api.Data
{
    public class KartTrackRepository
    {
        private readonly List<Track> _tracks = new()
        {
            new Track
            {
                Id = 1,
                Name = "TeamSport Edmonton",
                Location = "London",
                IsIndoor = true
            },

            new Track
            {
                Id = 2,
                Name = "Daytona Milton Keynes",
                Location = "Milton Keyenes",
                IsIndoor = false
            }

        };

        private readonly List<Session> _sessions = new()
        {
            new Session {
                Id = 1,
                TrackId = 1,
                DriverName = "Antonio",
                Date = DateTime.UtcNow,
                FastestLap = 58.45m,
                AverageLap = 61.20m,
                TotalLaps = 12,
                KartNumber = 7,
                Position = 3,
                Notes = "First test session at TeamSport Edmonton"
            },

            new Session{
                Id = 2,
                TrackId = 2,
                DriverName = "Adam",
                Date = DateTime.UtcNow,
                FastestLap = 62.10m,
                AverageLap = 65.40m,
                TotalLaps = 10,
                KartNumber = 14,
                Position = 6,
                Notes = "Outdoor session at Daytona Milton Keynes"

            }

        };

        public List<Track> GetTracks()
            {
            return _tracks; 
        }

        public Track? GetTrackById(int id)
        {
            return _tracks.FirstOrDefault(track => track.Id == id);

        }

        public List<Session> GetSessions()
        {
            return _sessions;
        }

        public Session? GetSessionById(int id)
        {
            return _sessions.FirstOrDefault(session => session.Id == id);
        }

        public Session? CreateSession(CreateSessionRequest request)
        {
            var trackExists = _tracks.Any(track => track.Id == request.TrackId);

            if (!trackExists)
            {
                return null;
            }

            var nextId = _sessions.Any()
                ? _sessions.Max(sessions => sessions.Id) + 1 : 1;

            var session = new Session
            {
                Id = nextId,
                TrackId = request.TrackId,
                DriverName = request.DriverName,
                Date = DateTime.UtcNow,
                FastestLap = request.FastestLap,
                AverageLap = request.AverageLap,
                TotalLaps = request.TotalLaps,
                KartNumber = request.KartNumber,
                Position = request.Position,
                Notes = request.Notes
            };

            _sessions.Add(session);

            return session;
        }

        public Session? GetPersonalBestByTrackId(int trackId)
        {
            var trackExists = _tracks.Any(track => track.Id == trackId);

            if (!trackExists)
            {
                return null;
            }

            return _sessions
                .Where(session => session.TrackId == trackId)
                .OrderBy(session => session.FastestLap)
                .FirstOrDefault();
        }
    }
}
