namespace KartTrack.Api.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Location { get; set; } = ""; 
        public bool IsIndoor { get; set; }
    }
}
