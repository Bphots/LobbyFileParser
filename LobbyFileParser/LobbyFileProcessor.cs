using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LobbyFileParser
{
    public class LobbyFileProcessor
    {
        public LobbyFileProcessor(string lobbyFile)
        {
            var tempPath = Path.GetTempFileName();
            File.Copy(lobbyFile, tempPath, true);
            m_lobbyBytes = File.ReadAllBytes(tempPath);
        }

        public Region GetRegion()
        {
            m_game = new TagAndRegionParser(m_lobbyBytes).Parse();
            return m_game.Region;
        }

        public List<LobbyParameter> LookForMatches(List<string> expectedHeroes, int startingOffset, List<string> heroeNames)
        {
            return new SelectedHeroParser(m_lobbyBytes, heroeNames).LookForMatches(expectedHeroes, startingOffset);
        }

        public Game ParseLobbyInfo(LobbyParameter lobbyParameter, List<string> heroeNames, List<string> maps)
        {
            InitializeMapAttributes(maps);

            if (m_game == null)
                m_game = new TagAndRegionParser(m_lobbyBytes).Parse();
            var heroes = new SelectedHeroParser(m_lobbyBytes, heroeNames).ParseHeroesInfo(lobbyParameter);
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

        private readonly byte[] m_lobbyBytes;
        private Game m_game;
    }
}