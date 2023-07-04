namespace BettingDataProvider.Models.ViewModels
{
    public class MatchViewModel
    {
        public Match Match { get; set; }

        public List<Bet> PreviewMarkets { get; set; }

        public List<Odd> Odds { get; set; }
    }
}
