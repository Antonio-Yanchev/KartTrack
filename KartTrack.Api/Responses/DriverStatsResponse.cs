namespace KartTrack.Api.Responses;

public class DriverStatsResponse
{
   public string DriverName { get; set; } = "";
   public int TotalSessions { get; set; }
   public int TotalLaps { get; set; }
   public int TracksVisited { get; set; }
   public decimal BestLap { get; set; }
   public decimal AverageFastestLap { get; set; }
   public decimal AveragePosition { get; set; }
   public int BestPosition { get; set; }
}
