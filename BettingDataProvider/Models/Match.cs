using System.ComponentModel.DataAnnotations;

namespace BettingDataProvider.Models
{
    public class Match
    {
        [Key]
        public int Id { get; set; }

        public int MatchId { get; set; }

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
