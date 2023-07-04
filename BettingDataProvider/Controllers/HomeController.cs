using BettingDataProvider.Data;
using BettingDataProvider.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace BettingDataProvider.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        //private static readonly object _lockObject = new object();

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
                    string apiUrl = "https://sports.ultraplay.net/sportsxml?clientKey=9C5E796D-4D54-42FD-A535-D7E77906541A&sportId=2357&days=7";

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] apiResponseBytes = await response.Content.ReadAsByteArrayAsync();

                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.Async = true;
                        settings.IgnoreWhitespace = true;

                        // Specify the encoding explicitly as UTF8
                        Encoding encoding = Encoding.UTF8;

                        using (MemoryStream memoryStream = new MemoryStream(apiResponseBytes))
                        using (StreamReader streamReader = new StreamReader(memoryStream, encoding))
                        using (XmlReader reader = XmlReader.Create(streamReader, settings))
                        {
                            /*bool lockTaken = false;
                            try
                            {
                                Monitor.Enter(_lockObject, ref lockTaken);
                                await ReadSportsAsync(reader);
                            }
                            finally
                            {
                                if (lockTaken)
                                {
                                    Monitor.Exit(_lockObject);
                                }
                            }          */           
                            await ReadSportsAsync(reader);
                        }
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }

        private async Task ReadSportsAsync(XmlReader reader)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {

                try
                {
                    while (await reader.ReadAsync())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "Sport")
                        {
                            string sportId = reader.GetAttribute("ID");

                            if (!_context.Sports.Any(s => s.SportId == sportId))
                            {
                                Sport currentSport = new Sport
                                {
                                    Name = reader.GetAttribute("Name"),
                                    SportId = sportId
                                };

                                _context.Sports.Add(currentSport);

                                await ReadEventsAsync(reader, currentSport);
                            }
                            else
                            {
                                Sport sportFromDb = await _context.Sports.FirstOrDefaultAsync(s => s.SportId == sportId);
                                await ReadEventsAsync(reader, sportFromDb);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }

        private async Task ReadEventsAsync(XmlReader reader, Sport currentSport)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Event")
                {
                    string eventId = reader.GetAttribute("ID");

                    if (!_context.Events.Any(e => e.EventId == eventId))
                    {
                        Event currentEvent = new Event()
                        {
                            Name = reader.GetAttribute("Name"),
                            EventId = eventId,
                            IsLive = bool.Parse(reader.GetAttribute("IsLive")),
                            CategoryID = int.Parse(reader.GetAttribute("CategoryID")),
                            Sport = currentSport
                        };
                        _context.Events.Add(currentEvent);
                        await ReadMatchesAsync(reader, currentEvent);
                    }
                    else
                    {
                        Event eventFromDb = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
                        await ReadMatchesAsync(reader, eventFromDb);
                    }

                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Sport") //avoid recursion
                {
                    break;
                }
            }
        }

        private async Task ReadMatchesAsync(XmlReader reader, Event currentEvent)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Match")
                {
                    string matchName = reader.GetAttribute("Name");
                    string matchID = reader.GetAttribute("ID");
                    DateTime matchStartDate = DateTime.Parse(reader.GetAttribute("StartDate"));

                    var matchType = reader.GetAttribute("MatchType");
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

                    if (_context.Matches.Any(m => m.MatchId == matchID))
                    {
                        Match matchFromDb = await _context.Matches.FirstOrDefaultAsync(m => m.MatchId == matchID);
                        
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

                        await ReadBetsAsync(reader, matchFromDb);
                    }
                    else
                    {
                        Match currentMatch = new Match()
                        {
                            Name = matchName,
                            MatchId = matchID,
                            StartDate = matchStartDate,
                            Type = matchMatchType,
                            IsActive = true,
                            Event = currentEvent
                        };
                        _context.Matches.Add(currentMatch);
                        await ReadBetsAsync(reader, currentMatch);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Event") //avoid recursion
                {
                    break;
                }
            }
        }

        private async Task ReadBetsAsync(XmlReader reader, Match currentMatch)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Bet")
                {
                    string betId = reader.GetAttribute("ID");
                    
                    if (!_context.Bets.Any(b => b.BetId == betId))
                    {
                        Bet currentBet = new Bet()
                        {
                            Name = reader.GetAttribute("Name"),
                            BetId = betId,
                            IsLive = bool.Parse(reader.GetAttribute("IsLive")),
                            Match = currentMatch,
                            IsActive = true
                        };

                        _context.Bets.Add(currentBet);
                        await ReadOddsAsync(reader, currentBet);
                    }
                    else
                    {
                        Bet betFromDb = await _context.Bets.FirstOrDefaultAsync(b => b.BetId == betId);
                        await ReadOddsAsync(reader, betFromDb);
                    }
                    
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Match") //avoid recursion
                {
                    break;
                }
            }
        }

        private async Task ReadOddsAsync(XmlReader reader, Bet currentBet)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Odd")
                {
                    string oddId = reader.GetAttribute("ID");
                    double oddValue = double.Parse(reader.GetAttribute("Value"), CultureInfo.InvariantCulture);

                    if (_context.Odds.Any(o => o.OddId == oddId))
                    {
                        Odd oddFromDb = await _context.Odds.FirstOrDefaultAsync(o => o.OddId == oddId);

                        if (oddFromDb.Value != oddValue)
                        {
                            oddFromDb.Value = oddValue;
                            _context.Odds.Update(oddFromDb);
                        }
                    }
                    else
                    {
                        double? specialBetValue = null;
                        if (reader.GetAttribute("SpecialBetValue") != null)
                        {
                            specialBetValue = double.Parse(reader.GetAttribute("SpecialBetValue"), CultureInfo.InvariantCulture);
                        }

                        Odd currentOdd = new Odd()
                        {
                            Name = reader.GetAttribute("Name"),
                            OddId = oddId,
                            Value = oddValue,
                            SpecialBetValue = specialBetValue,
                            IsActive = true,
                            Bet = currentBet
                        };

                        _context.Odds.Add(currentOdd);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Bet") //avoid recursion
                {
                    break;
                }
            }
        }
    }
}