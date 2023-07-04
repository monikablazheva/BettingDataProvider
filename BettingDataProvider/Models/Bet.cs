using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BettingDataProvider.Models
{
    public class Bet
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string BetId { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsLive { get; set; }

        public bool? IsActive { get; set; }

        public ICollection<Odd> Odds { get; set; }

        public Match Match { get; set; }
    }
}
