using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TMioParse.Utilities;

namespace TMioParse
{
    class Program
    {
        private static Leaderboard _leaderboard;
        private static PlayerStats _playerStats;
        private static ChromeDriver _driver;
        private static WebDriverWait _wait;

        ~Program()
        {
            _driver.Close();
            _driver.Dispose();
        }

        static async Task Main(string[] args)
        {
            var chromeOptions = new ChromeOptions();
            // chromeOptions.AddArguments("headless");

            _driver = new ChromeDriver(chromeOptions);

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            _wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(ElementNotVisibleException));


            if (args.Length >= 2 && args[0] == "find")
            {
                var p = args[1];
                
                _driver.Navigate().GoToUrl($"https: //trackmania.io/#/player/{0}/matches-royal");
                
                return;
            }

            _leaderboard = new Leaderboard(_wait);
            _playerStats = new PlayerStats(_wait);
            // _leaderboard.ScrapeLeaderboardData(_driver,500);

            if (args.Length > 0)
            {
                if (args[0] == "-log")
                {
                    var stats = _playerStats.ToList();
                    stats.Sort((k, v) => v.Value.Wins - k.Value.Wins);

                    var t = new TablePrinter("Rank", "Player", "Updated", "Games", "Wins", "BWS", "Toxic", "Win %");

                    int i = 1;
                    foreach (var stat in stats)
                    {
                        RoyalMatchesInfo data = stat.Value;
                        string           name = stat.Key;

                        t.AddRow(i, name, data.Updated, data.matchdata.Count, data.Wins, data.BestWinStreak, data.AbandonedGames, $"{data.WinPercentage:F2}%");

                        i++;
                    }

                    t.Print();

                    return;
                }

                var match = Regex.Match(args[0], @"(?'start'\d+)-(?'end'\d+)");

                if (match.Success)
                {
                    var start = int.Parse(match.Groups["start"].Value);
                    var end   = int.Parse(match.Groups["end"].Value);

                    var len = end - start;

                    var range = Enumerable.Range(start, len);

                    foreach (var i in range)
                    {
                        var player = _leaderboard[i];
                        Console.WriteLine($"scraping data for {player.name}");
                        await _playerStats.GetPlayerWinDataId(_driver, player.name);
                    }
                }
                else
                {
                    foreach (var name in args)
                    {
                        Console.WriteLine($"scraping data for {name}");
                        await _playerStats.GetPlayerWinData(_driver, _leaderboard[name]);
                    }
                }
            }
        }
    }
}