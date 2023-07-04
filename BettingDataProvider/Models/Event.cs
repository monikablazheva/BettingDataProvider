using System.ComponentModel.DataAnnotations;

namespace BettingDataProvider.Models
{
    public class Event
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string EventId { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsLive { get; set; }

        public int CategoryID { get; set; }

        public ICollection<Match> Matches { get; set; }

        public Sport Sport { get; set; }
    }
}
