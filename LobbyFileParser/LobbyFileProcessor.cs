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

        public Game ParseLobbyInfo()
        {
            var game = new TagAndRegionParser(m_lobbyBytes).Parse();
            var heroes = new SelectedHeroParser(m_lobbyBytes, m_heroes).ParseHeroesInfo();
            var map = new MapParser(m_lobbyBytes, m_maps).Parse();

            if (game.Players.Count == 5 && heroes.GetRange(0, 5).All(h => h == SelectedHeroParser.Random))
            {
                heroes.RemoveRange(0, 5);
            }

            for (var i = 0; i < game.Players.Count; i++)
            {
                if (!heroes.Contains(SelectedHeroParser.Fail))
                    game.Players[i].SelectedHero = heroes[i];
                else
                    game.Players[i].SelectedHero = SelectedHeroParser.Fail;
            }

            game.Map = map;

            return game;
        }


        private void InitializeMapAttributes(List<string> maps)
        {
            m_maps = maps;
        }

        private List<string> m_maps;
        private List<string> m_heroes;

        private readonly byte[] m_lobbyBytes;
    }
}