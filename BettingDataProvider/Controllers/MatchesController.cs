using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BettingDataProvider.Data;
using BettingDataProvider.Models;
using BettingDataProvider.Models.ViewModels;
using BettingDataProvider.Services;

namespace BettingDataProvider.Controllers
{
    public class MatchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Matches
        public async Task<IActionResult> Index()
        {
            if(_context.Matches == null)
            {
                return NotFound("Entity set 'ApplicationDbContext.Matches' is null.");
            }


            var matchesFromDb = await _context.Matches.Include(m => m.Bets).ThenInclude(b => b.Odds).AsSplitQuery().ToListAsync();

            DateTime now = DateTime.Now;
            DateTime next24Hours = now.AddHours(24);

            var matchesInNext24Hours = matchesFromDb.Where(match => match.StartDate >= now && match.StartDate <= next24Hours).ToList();

            List<MatchViewModel> matchViewModels = new List<MatchViewModel>();
            
            foreach (Match match in matchesInNext24Hours)
            {
                List<Odd> odds = new List<Odd>();    
                Dictionary<long, List<Odd>> marketsOdds = new Dictionary<long, List<Odd>>();
                
                var previewMarkets = match.Bets.Where(b => b.IsActive==true && b.Name == "Match Winner" || b.Name == "Map Advantage" || b.Name == "Total Maps Played").ToList();
                
                foreach(var bet in previewMarkets)
                {
                    odds = bet.Odds.Where(o => o.IsActive == true).ToList(); 
                    foreach(var odd in odds)
                    {
                        if(odd.SpecialBetValue != null)
                        {
                            odds = odds.GroupBy(o => o.SpecialBetValue).First().ToList();
                        }
                    }
                    marketsOdds.Add(bet.Id, odds);
                }
                MatchViewModel matchViewModel = new MatchViewModel()
                {
                    Match = match,
                    PreviewMarkets = previewMarkets,
                    MarketsOdds = marketsOdds
                };
                matchViewModels.Add(matchViewModel);
            }
            return View(matchViewModels);
        }

        // GET: Matches/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Matches == null)
            {
                return NotFound();
            }

            var match = await _context.Matches.Include(m => m.Bets).ThenInclude(b => b.Odds).AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound();
            }
            MatchDetailsViewModel matchDetailsViewModel = new MatchDetailsViewModel()
            {
                Match = match,
                ActiveMarkets = match.Bets.Where(b => b.IsActive == true).ToList(),
                InactiveMarkets = match.Bets.Where(b => b.IsActive == false).ToList()
            };

            return View(matchDetailsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> HideMatch(long matchId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match != null)
            {
                QueueManager.AddItem($"Match {match.Name} was hidden by an user");
            }

            return Json(new { HideId = matchId, Entity = "match" });
        }

        [HttpPost]
        public async Task<IActionResult> HideBet(long betId)
        {
            var bet = await _context.Bets.FindAsync(betId);
            if (bet != null)
            {
                QueueManager.AddItem($"Bet {bet.Name} hidden by an user");
            }

            return Json(new { HideId = betId, Entity = "bet" });
        }

        [HttpPost]
        public async Task<IActionResult> HideOdd(long oddId)
        {
            var odd = await _context.Odds.FindAsync(oddId);
            if (odd != null)
            {
                QueueManager.AddItem($"Odd {odd.Name}was hidden by an user");
            }

            return Json(new { HideId = oddId, Entity = "odd" });
        }
    }
}
