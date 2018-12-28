using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LobbyFileParser
{
    public class LobbyFileProcessor
    {
        public LobbyFileProcessor(string lobbyFile, List<string> heroes, List<string> maps)
        {
            var tempPath = Path.GetTempFileName();
            File.Copy(lobbyFile, tempPath, true);
            m_lobbyBytes = File.ReadAllBytes(tempPath);
            m_heroes = heroes;
            InitializeMapAttributes(maps);
        }

        public Region GetRegion()
        {
            m_game = new TagAndRegionParser(m_lobbyBytes).Parse();
            return m_game.Region;
        }

        public List<LobbyParameter> LookForMatches(List<string> expectedHeroes, int startingOffset)
        {
            return new SelectedHeroParser(m_lobbyBytes, m_heroes).LookForMatches(expectedHeroes, startingOffset);
        }

        public Game ParseLobbyInfo(LobbyParameter lobbyParameter)
        {
            if (m_game == null)
                m_game = new TagAndRegionParser(m_lobbyBytes).Parse();
            var heroes = new SelectedHeroParser(m_lobbyBytes, m_heroes).ParseHeroesInfo(lobbyParameter);
            var map = new MapParser(m_lobbyBytes, m_maps).Parse();

            if (m_game.Players.Count == 5 && heroes.GetRange(0, 5).All(h => h == SelectedHeroParser.Random))
            {
                heroes.RemoveRange(0, 5);
            }

            for (var i = 0; i < m_game.Players.Count; i++)
            {
                if (!heroes.Contains(SelectedHeroParser.Fail))
                    m_game.Players[i].SelectedHero = heroes[i];
                else
                    m_game.Players[i].SelectedHero = SelectedHeroParser.Fail;
            }

            m_game.Map = map;

            return m_game;
        }


        private void InitializeMapAttributes(List<string> maps)
        {
            m_maps = maps;
        }

        private List<string> m_maps;
        private List<string> m_heroes;

        private readonly byte[] m_lobbyBytes;
        private Game m_game;
    }
}