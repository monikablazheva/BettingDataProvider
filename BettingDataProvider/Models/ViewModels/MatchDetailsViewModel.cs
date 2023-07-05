namespace BettingDataProvider.Models.ViewModels
{
    public class MatchDetailsViewModel
    {
        public Match Match { get; set; }

        public List<Bet> ActiveMarkets { get; set; }

        public List<Bet> InactiveMarkets { get; set; }
    }
}
