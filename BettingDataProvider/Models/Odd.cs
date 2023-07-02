using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BettingDataProvider.Models
{
    public class Odd
    {
        [Key]
        public int Id { get; set; }

        public int OddId { get; set; }

        [Required]
        public string Name { get; set; }

        public double Value { get; set; }

        public double? SpecialBetValue { get; set; }

        public bool? IsActive { get; set; }

        public Bet Bet { get; set; }
    }
}
