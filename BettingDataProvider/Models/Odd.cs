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

        [Range(0, 100, ErrorMessage = "Value must be between 0 and 100.")]
        [Column(TypeName = "decimal(5, 2)")]
        public double Value { get; set; }

        [Range(-100, 100, ErrorMessage = "Value must be between -100 and 100.")]
        [Column(TypeName = "decimal(5, 2)")]
        public double? SpecialBetValue { get; set; }

        public Bet Bet { get; set; }
    }
}
