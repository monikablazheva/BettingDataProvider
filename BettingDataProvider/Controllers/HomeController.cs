using BettingDataProvider.Data;
using BettingDataProvider.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
                            Sport currentSport = new Sport
                            {
                                Name = sportNode.Attributes["Name"].Value,
                                SportId = int.Parse(sportNode.Attributes["ID"].Value)
                            };
                            _context.Sports.AddIfNotExists(currentSport); //add the sport to the database if not already in
                            await _context.SaveChangesAsync();

                            XmlNodeList eventNodes = sportNode.SelectNodes("Event"); //events
                            if (eventNodes == null)
                                return NotFound("Event nodes are null.");

                            foreach (XmlNode eventNode in eventNodes)
                            {
                                Event currentEvent = new Event()
                                {
                                    Name = eventNode.Attributes["Name"].Value,
                                    EventId = int.Parse(eventNode.Attributes["ID"].Value),
                                    IsLive = bool.Parse(eventNode.Attributes["IsLive"].Value),
                                    CategoryID = int.Parse(eventNode.Attributes["CategoryID"].Value),
                                    Sport = currentSport
                                };
                                _context.Events.AddIfNotExists(currentEvent); //add the event to the database if not already in

                                if (!currentSport.Events.Contains(currentEvent)) //add the event to its sport's list if not already in
                                {
                                    currentSport.Events.Add(currentEvent);
                                    _context.Sports.Update(currentSport);
                                }
                                await _context.SaveChangesAsync();

                                XmlNodeList matchNodes = eventNode.SelectNodes("Match"); //matches
                                if (matchNodes == null)
                                    return NotFound("Match nodes are null.");

                                /*foreach(XmlNode matchNode in matchNodes)
                                {
                                    if
                                }*/
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }
    }
}