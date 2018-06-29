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

            InitializeHeroes(heroes);
            InitializeMapAttributes(maps);
        }

        public Game ParseLobbyInfo()
        {
            var game = new TagAndRegionParser(m_lobbyBytes).Parse();
            var heroes = new SelectedHeroParser(m_lobbyBytes, m_heroElements).ParseHeroesInfo();
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

        private void InitializeHeroes(IEnumerable<string> heroes)
        {
            byte oddByte1 = 1;
            byte oddByte2 = 0;
            byte evenByte1 = 0;
            byte evenByte2 = 2;

            foreach (var hero in heroes)
            {
                var heroElement = new HeroElement
                {
                    EvenByte1 = evenByte1,
                    EvenByte2 = evenByte2,
                    OddByte1 = oddByte1,
                    OddByte2 = oddByte2,
                    Name = hero
                };
                m_heroElements.Add(heroElement);

                evenByte2++;
                if (evenByte2 > 0x1F)
                {
                    evenByte2 = 0;
                    evenByte1 += 4;
                }

                oddByte2++;
                if (oddByte2 > 0x01)
                {
                    oddByte2 = 0;
                    oddByte1++;
                }
            }
        }

        private void InitializeMapAttributes(List<string> maps)
        {
            m_maps = maps;
        }

        private readonly List<HeroElement> m_heroElements = new List<HeroElement>();

        private List<string> m_maps;

        private readonly byte[] m_lobbyBytes;
    }
}