namespace WebApplication1.Models
{
    public class ObstacleData
    {
        public string ObstacleName { get; set; }
        public double ObstacleHeight { get; set; }
        public string ObstacleDescription { get; set; }
        public double ObstacleLatitude { get; set; }
        public double ObstacleLongitude { get; set; }

        
        // Nye egenskaper for å støtte ulike former
        public string ObstacleType { get; set; } // "marker", "circle" eller "line"
        public double? ObstacleRadius { get; set; } // bare relevant for sirkel
        public string? ObstacleLineCoords { get; set; } // f.eks. lagret som JSON for polyline

    }
}
