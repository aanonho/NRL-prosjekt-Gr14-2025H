namespace WebApplication1.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // typ høyspentledning, kran osv
        public string Organization { get; set; } = string.Empty; // NLA, Luftforsvaret o.l
        public string ReporterName { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public string Status { get; set; } = string.Empty; // pending, approved, rejected
    }
}
