using System.ComponentModel.DataAnnotations;

namespace BettingDataProvider.Models
{
    public class Sport
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string SportId { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}
