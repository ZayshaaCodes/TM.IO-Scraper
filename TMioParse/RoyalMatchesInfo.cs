using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Newtonsoft.Json;

namespace TMioParse
{
    [System.Serializable]
    public class RoyalMatchesInfo
    {
        public DateTime Updated;
        
        public int Wins;
        public int BestWinStreak;

        public int AbandonedGames;
        public int WorstLoseStreak;

        public List<RoyalMatchInfo> matchdata;

        [JsonProperty]
        public float WinPercentage => (float)Wins / matchdata.Count *100f;
        public float WinPercentageLast50 => (float)Wins / matchdata.Count *100f;

        public RoyalMatchesInfo()
        {
            
        }

        public RoyalMatchesInfo(IEnumerable<RoyalMatchInfo> list)
        {
            Updated   = DateTime.Now;
            
            matchdata = list.ToList();

            AbandonedGames = matchdata.FindAll(i => i.Leaver).Count;
            Wins           = matchdata.FindAll(i => i.DidWin).Count;

            BestWinStreak = 0;

            int c = 0;
            foreach (var data in matchdata)
            {
                if (data.DidWin) c++;
                else
                {
                    if (c > BestWinStreak) BestWinStreak = c;
                    c = 0;
                }
            }
            
            
            WinsInLastNumberOfGames(20);
        }

        private void WinsInLastNumberOfGames(int n = 20)
        {
            var last20 = matchdata.Take(n);

            var x = 0;
            foreach (var info in last20)
            {
                if (info.DidWin)
                {
                    x++;
                }
            }
        }

        public override string ToString()
        {

            var last50 = matchdata.Take(20);
            
            var c      = 0;
            foreach (var info in last50)
            {
                if (info.DidWin)
                {
                    c++;
                }
            }
            
            return $"{Updated, 10:M/d/yy hh:mm} | {matchdata.Count,-4} | {Wins,-4} | {BestWinStreak,-3} | {AbandonedGames,-3} | {WinPercentage,6:F2}% | {c / 20f * 100:F2}%";
        }
    }
}