namespace TMioParse
{
    [System.Serializable]
    public struct LeaderboardPlayerData
    {
        public int position;
        public string name;
        public string uri;

        public LeaderboardPlayerData(int position, string name, string uri)
        {
            this.position = position;
            this.name     = name;
            this.uri      = uri;
        }

        public override string ToString()
        {
            return $"{position,5} | {name,30} | {uri}";
        }
    }
}