using System.ComponentModel.DataAnnotations;

namespace BettingDataProvider.Models
{
    public class Match
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string MatchId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public bool? IsActive { get; set; }

        [Required]
        public MatchType Type { get; set; }

        public ICollection<Bet> Bets { get; set; }

        public Event Event { get; set; }
    }

    public enum MatchType { Prematch, Live, Outright }
}
