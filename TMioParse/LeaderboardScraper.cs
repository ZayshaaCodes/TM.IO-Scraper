using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TMioParse.Utilities;

namespace TMioParse
{
    public class Leaderboard : IEnumerable<LeaderboardPlayerData>
    {
        private const int LeaderboardPageSize = 50;

        private readonly string _dataPath = Path.Combine(AppContext.BaseDirectory, "LeaderboardData.json");
        private List<LeaderboardPlayerData> _leaderboardData;

        private readonly WebDriverWait _wait;

        // indexers
        public LeaderboardPlayerData this[int i] => _leaderboardData[i];
        public LeaderboardPlayerData this[string name] => _leaderboardData.First(data => string.Equals(data.name, name, StringComparison.CurrentCultureIgnoreCase));

        public Leaderboard(WebDriverWait wait)
        {
            _wait            = wait;
            _leaderboardData = new();
            LoadLeaderboardJson();
        }

        public IEnumerator<LeaderboardPlayerData> GetEnumerator() => _leaderboardData.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        ~Leaderboard()
        {
            SaveLeaderboardJson();
        }

        public void ScrapeLeaderboardData(IWebDriver driver, int count = LeaderboardPageSize)
        {
            _leaderboardData.Clear();

            var pages = (int) MathF.Ceiling((float) count / LeaderboardPageSize);

            driver.Navigate().GoToUrl("https://trackmania.io/#/top/royal");

            for (var i = 0; i < pages - 1; i++)
                _wait.ForLoadMoreButton().Click();
            
            //waiting for last page to Load
            _wait.ForLoadMoreButton();
            
            var content = driver.FindElement(By.Id("content"));
            var table   = content.FindElement(By.XPath("//tbody"));
            var rows    = table.FindElements(By.XPath("//tr"));

            foreach (IWebElement row in rows)
            {
                int    pos  = int.Parse(row.FindElement(By.XPath("./td[@class='pos']")).Text);
                string name = row.FindElement(By.XPath("./td/p/span/a/span")).Text;
                string url  = row.FindElement(By.XPath("./td/p/span/a")).GetAttribute("href");

                _leaderboardData.Add(new LeaderboardPlayerData(pos, name, url));
            }

            SaveLeaderboardJson();
        }


        private void LoadLeaderboardJson()
        {
            if (!File.Exists(_dataPath))
            {
                Console.WriteLine(_dataPath + " does not exist.");
            }
            else
            {
                try
                {
                    _leaderboardData = JsonConvert.DeserializeObject<List<LeaderboardPlayerData>>(File.ReadAllText(_dataPath));
                    Console.WriteLine($"Leaderboard Data Loaded from Json: {_leaderboardData.Count}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        void SaveLeaderboardJson()
        {
            var playerDataStr = JsonConvert.SerializeObject(_leaderboardData, Formatting.Indented);
            File.WriteAllText(_dataPath, playerDataStr);
        }
    }
}