using BettingDataProvider.Data;
using BettingDataProvider.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BettingDataProvider.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PullData()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = "https://sports.ultraplay.net/sportsxml?clientKey=9C5E796D-4D54-42FD-A535-D7E77906541A&sportId=2357&days=7"; //API URL

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();

                        // Convert the API response data to an XmlDocument
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(apiResponse);
                        if (xmlDoc == null)
                            return NotFound("XML doc is null.");

                        //XmlSports/Sport/Event/Match/Bet/Odd
                        XmlNodeList sportNodes = xmlDoc.SelectNodes("/XmlSports/Sport"); //sports
                        if (sportNodes == null)
                            return NotFound("Sport nodes are null.");

                        foreach (XmlNode sportNode in sportNodes)
                        {
                            int sportId = int.Parse(sportNode.Attributes["ID"].Value);
                            Sport currentSport = new Sport
                            {
                                Name = sportNode.Attributes["Name"].Value,
                                SportId = int.Parse(sportNode.Attributes["ID"].Value)
                            };
                            
                            if (!_context.Sports.Any(s => s.SportId == sportId))
                            {
                                _context.Sports.Add(currentSport); //add the sport to the database if not already in
                            }

                            XmlNodeList eventNodes = sportNode.SelectNodes("Event"); //events
                            if (eventNodes == null)
                                return NotFound("Event nodes are null.");

                            foreach (XmlNode eventNode in eventNodes)
                            {
                                int eventId = int.Parse(eventNode.Attributes["ID"].Value);
                                Event currentEvent = new Event()
                                {
                                    Name = eventNode.Attributes["Name"].Value,
                                    EventId = int.Parse(eventNode.Attributes["ID"].Value),
                                    IsLive = bool.Parse(eventNode.Attributes["IsLive"].Value),
                                    CategoryID = int.Parse(eventNode.Attributes["CategoryID"].Value),
                                    Sport = currentSport
                                };

                                if (!_context.Events.Any(e => e.EventId == eventId))
                                {
                                    _context.Events.Add(currentEvent); //add the event to the database if not already in
                                }

                                XmlNodeList matchNodes = eventNode.SelectNodes("Match"); //matches
                                if (matchNodes == null)
                                    return NotFound("Match nodes are null.");

                                foreach (XmlNode matchNode in matchNodes)
                                {
                                    string matchName = matchNode.Attributes["Name"].Value;
                                    int matchID = int.Parse(matchNode.Attributes["ID"].Value);
                                    DateTime matchStartDate = DateTime.Parse(matchNode.Attributes["StartDate"].Value);

                                    var matchType = matchNode.Attributes["MatchType"].Value;
                                    Models.MatchType matchMatchType = 0;
                                    if (matchType.ToLower() == Models.MatchType.Prematch.ToString().ToLower())
                                    {
                                        matchMatchType = Models.MatchType.Prematch;
                                    }
                                    else if (matchType.ToLower() == Models.MatchType.Live.ToString().ToLower())
                                    {
                                        matchMatchType = Models.MatchType.Live;
                                    }
                                    else if (matchType.ToLower() == Models.MatchType.Outright.ToString().ToLower())
                                    {
                                        matchMatchType = Models.MatchType.Outright;
                                    }

                                    Match currentMatch = new Match()
                                    {
                                        Name = matchName,
                                        MatchId = matchID,
                                        StartDate = matchStartDate,
                                        Type = matchMatchType,
                                        IsActive = true,
                                        Event = currentEvent
                                    };

                                    if (_context.Matches.Any(m => m.MatchId == matchID))
                                    {
                                        Match matchFromDb = await _context.Matches.Include(m => m.Bets).ThenInclude(b => b.Odds).FirstOrDefaultAsync(m => m.MatchId == matchID);

                                        if (matchFromDb == null)
                                            return NotFound("Match from db is null.");

                                        if (matchFromDb.StartDate != matchStartDate)
                                        {
                                            matchFromDb.StartDate = matchStartDate;
                                            _context.Matches.Update(matchFromDb);
                                        }
                                        if (matchFromDb.Type != matchMatchType)
                                        {
                                            matchFromDb.Type = matchMatchType;
                                            _context.Matches.Update(matchFromDb);
                                        }

                                    }
                                    else
                                    { 
                                        _context.Matches.Add(currentMatch); //add the match to the database 
                                        
                                    }

                                    XmlNodeList betNodes = matchNode.SelectNodes("Bet"); //bets
                                    if (betNodes == null)
                                        return NotFound("Bet nodes are null.");

                                    foreach(XmlNode betNode in betNodes)
                                    {
                                        int betId = int.Parse(betNode.Attributes["ID"].Value);
                                        Bet currentBet = new Bet()
                                        {
                                            Name = betNode.Attributes["Name"].Value,
                                            BetId = int.Parse(betNode.Attributes["ID"].Value),
                                            IsLive = bool.Parse(betNode.Attributes["IsLive"].Value),
                                            Match = currentMatch,
                                            IsActive = true
                                        };

                                        if (!_context.Bets.Any(b => b.BetId == betId))
                                        {
                                            _context.Bets.Add(currentBet); //add the bet to the database if not already in
                                        }

                                        XmlNodeList oddNodes = betNode.SelectNodes("Odd"); //bets
                                        if (oddNodes == null)
                                            return NotFound("Odd nodes are null.");

                                        foreach (XmlNode oddNode in oddNodes)
                                        {
                                            int oddId = int.Parse(oddNode.Attributes["ID"].Value);
                                            double oddValue = double.Parse(oddNode.Attributes["Value"].Value, CultureInfo.InvariantCulture);
                                            
                                            if (_context.Odds.Any(o => o.OddId == oddId))
                                            {
                                                Odd oddFromDb = await _context.Odds.FirstOrDefaultAsync(o => o.OddId == oddId);

                                                if (oddFromDb == null)
                                                    return NotFound("Odd from db is null.");

                                                if (oddFromDb.Value != oddValue)
                                                {
                                                    oddFromDb.Value = oddValue;
                                                    _context.Odds.Update(oddFromDb);
                                                }
                                            }
                                            else
                                            {
                                                if(oddNode.Attributes["SpecialBetValue"] != null)
                                                {
                                                    Odd currentOdd = new Odd()
                                                    {
                                                        Name = oddNode.Attributes["Name"].Value,
                                                        OddId = oddId,
                                                        Value = oddValue,
                                                        SpecialBetValue = double.Parse(oddNode.Attributes["SpecialBetValue"].Value, CultureInfo.InvariantCulture),
                                                        IsActive = true,
                                                        Bet = currentBet
                                                    };
                                                    _context.Odds.Add(currentOdd); //add the odd to the database 
                                                }
                                                else
                                                {
                                                    Odd currentOdd = new Odd()
                                                    {
                                                        Name = oddNode.Attributes["Name"].Value,
                                                        OddId = oddId,
                                                        Value = oddValue,
                                                        IsActive = true,
                                                        Bet = currentBet
                                                    };
                                                    _context.Odds.Add(currentOdd); //add the odd to the database 
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}