using System.ComponentModel.DataAnnotations;

namespace BettingDataProvider.Models
{
    public class Sport
    {
        [Key]
        public int Id { get; set; }

        public int SportId { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}
