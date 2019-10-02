namespace Replica.App.Models
{
    public class ChatRoute : Model
    {
        public int ChatId { get; set; }
        public virtual Chat Chat { get; set; }

        public int RouteId { get; set; }
        public virtual Route Route { get; set; }
    }
}