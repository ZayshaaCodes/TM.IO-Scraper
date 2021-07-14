using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TMioParse.Utilities;

namespace TMioParse
{
    public class PlayerStats : IEnumerable<KeyValuePair<string, RoyalMatchesInfo>>
    {
        private readonly Dictionary<string, RoyalMatchesInfo> _royalStatsDict = new();
        
        private readonly string _dataPath = Path.Combine(AppContext.BaseDirectory, "Players/{0}.json");
        private readonly WebDriverWait _wait;

        public PlayerStats(WebDriverWait wait)
        {
            _wait = wait;

            LoadPlayerStatsCache();
        }

        public void LoadPlayerStatsCache()
        {
            var root = Path.GetDirectoryName(_dataPath);
            Console.WriteLine(root);

            DirectoryInfo directoryInfo = new(root);
            var           files         = directoryInfo.GetFiles("*.json");

            var c = 0;
            foreach (var file in files)
            {
                var txt  = File.ReadAllText(file.FullName);
                var info = JsonConvert.DeserializeObject<RoyalMatchesInfo>(txt);
                var name = Path.GetFileNameWithoutExtension(file.Name);

                if (!_royalStatsDict.TryAdd(name, info))
                    Console.WriteLine($"data for {name} already exists");
                else
                    c++;
            }

            Console.WriteLine($"stats loaded for {c} players(s)");
        }

        void Add(string name, RoyalMatchesInfo royalMatchesInfo)
        {
            if (!_royalStatsDict.TryAdd(name, royalMatchesInfo))
                Console.WriteLine($"data for {name} already exists");
        }
        
        public async Task GetPlayerWinData(IWebDriver driver, LeaderboardPlayerData playerData)
        {
            driver.Navigate().GoToUrl(playerData.uri + "/matches-royal");

            var content = driver.FindElement(By.Id("content"));

            var button = _wait.ForLoadMoreButton();

            int beforeClickCount;
            int afterClickCount = 0;
            
            //clicking "Load More..." until the number of rows doesnt change
            do
            {
                beforeClickCount = afterClickCount;

                button.Click();
                await Task.Delay(500);
                button = _wait.ForLoadMoreButton();

                afterClickCount = content.FindElements(By.XPath("//tbody/tr")).Count;
            } while (beforeClickCount != afterClickCount);

            // making sure the last page is done
            _wait.ForLoadMoreButton();

            var rows = content.FindElements(By.XPath("//tbody/tr"));

            var res = new List<RoyalMatchInfo>();
            foreach (var row in rows)
            {
                var faElements = row.FindElements(By.ClassName("fa"));
                var infoUrl    = row.FindElement(By.XPath(".//a")).GetAttribute("href");

                var won  = false;
                var left = false;

                foreach (var element in faElements)
                {
                    if (element.GetAttribute("class").Contains("ban")) left  = true;
                    if (element.GetAttribute("class").Contains("check")) won = true;
                }

                res.Add(new RoyalMatchInfo(0, won, left, infoUrl));
            }

            var royalMatchesInfo = new RoyalMatchesInfo(res);
            _royalStatsDict.TryAdd(playerData.name, royalMatchesInfo);

            SavePlayerStats(playerData.name, royalMatchesInfo);
        }

        private void SavePlayerStats(string name, RoyalMatchesInfo royalMatchesInfo)
        {
            var path    = string.Format(_dataPath, name);
            var jsonStr =JsonConvert.SerializeObject(royalMatchesInfo);

            File.WriteAllText(path, jsonStr);
        }


        public IEnumerator<KeyValuePair<string, RoyalMatchesInfo>> GetEnumerator()
        {
            return _royalStatsDict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async Task GetPlayerWinDataId(ChromeDriver driver, string playerId)
        {
            //"bf60f37f-87cd-4d7e-87bb-05298f2f9b0e"
            var url = "//https://trackmania.io/#/player/{0}/matches-royal";
                driver.Navigate().GoToUrl(string.Format(url, playerId));

                var content = driver.FindElement(By.Id("content"));

                var button = _wait.ForLoadMoreButton();

                int beforeClickCount;
                int afterClickCount = 0;
            
                //clicking "Load More..." until the number of rows doesnt change
                do
                {
                    beforeClickCount = afterClickCount;

                    button.Click();
                    await Task.Delay(500);
                    button = _wait.ForLoadMoreButton();

                    afterClickCount = content.FindElements(By.XPath("//tbody/tr")).Count;
                } while (beforeClickCount != afterClickCount);

                // making sure the last page is done
                _wait.ForLoadMoreButton();

                var rows = content.FindElements(By.XPath("//tbody/tr"));

                var res = new List<RoyalMatchInfo>();
                foreach (var row in rows)
                {
                    var faElements = row.FindElements(By.ClassName("fa"));
                    var infoUrl    = row.FindElement(By.XPath(".//a")).GetAttribute("href");

                    var won  = false;
                    var left = false;

                    foreach (var element in faElements)
                    {
                        if (element.GetAttribute("class").Contains("ban")) left  = true;
                        if (element.GetAttribute("class").Contains("check")) won = true;
                    }

                    res.Add(new RoyalMatchInfo(0, won, left, infoUrl));
                }

                var royalMatchesInfo = new RoyalMatchesInfo(res);

                var name = driver.FindElement(By.XPath("//h1[@class='title']/span[@class='game-text']")).Text;
                _royalStatsDict.TryAdd(name, royalMatchesInfo);

                SavePlayerStats(name, royalMatchesInfo);
        }
    }
}