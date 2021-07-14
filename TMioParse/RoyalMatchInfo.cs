using System;

namespace TMioParse
{
    [Serializable]
    public struct RoyalMatchInfo
    {
        public int PointChange;
        public bool DidWin;
        public bool Leaver;
        public string InfoUrl;

        public RoyalMatchInfo(int pointChange, bool didWin, bool leaver, string infoUrl)
        {
            PointChange = pointChange;
            DidWin      = didWin;
            InfoUrl     = infoUrl;

            Leaver = leaver;
        }

        public override string ToString()
        {
            return $"{PointChange,5} | {DidWin,8} | {Leaver,8} | {InfoUrl}";
        }
    }
}