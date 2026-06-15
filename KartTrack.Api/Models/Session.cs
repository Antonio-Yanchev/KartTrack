namespace KartTrack.Api.Models
{
    public class Session
    {
        public int Id { get; set; }
        public int TrackId { get; set; }

        public string DriverName { get; set; } = "";

        public DateTime Date { get; set; }

        public decimal FastestLap { get; set; }

        public decimal AverageLap { get; set; }

        public int TotalLaps { get; set; }

        public int KartNumber { get; set; }

        public int Position { get; set; }

        public string Notes { get; set; } = "";
    }

}
