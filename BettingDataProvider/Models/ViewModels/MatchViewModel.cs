namespace BettingDataProvider.Models.ViewModels
{
    public class MatchViewModel
    {
        public Match Match { get; set; }

        public List<Bet> PreviewMarkets { get; set; }

        public Dictionary<long, List<Odd>> MarketsOdds { get; set; }
    }
}
