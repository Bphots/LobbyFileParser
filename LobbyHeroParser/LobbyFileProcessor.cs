using System.Collections.Generic;
using System.IO;

namespace LobbyFileParser
{
    public class LobbyFileProcessor
    {
        public LobbyFileProcessor(string lobbyFile, List<string> heroes)
        {
            var tempPath = Path.GetTempFileName();
            File.Copy(lobbyFile, tempPath, true);
            m_lobbyBytes = File.ReadAllBytes(tempPath);

            InitializeHeroes(heroes);
        }

        public Game ParseLobbyInfo()
        {
            var game = new TagAndRegionParser(m_lobbyBytes).Parse();
            var heroes = new SelectedHeroParser(m_lobbyBytes, m_heroElements).ParseHeroesInfo();
            for (var i = 0; i < game.Players.Count; i++)
            {
                game.Players[i].SelectedHero = heroes[i];
            }

            return game;
        }

        private void InitializeHeroes(IEnumerable<string> heroes)
        {
            byte oddByte1 = 0;
            byte oddByte2 = 2;
            byte evenByte1 = 1;
            byte evenByte2 = 0;

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

                oddByte2++;
                if (oddByte2 > 0x1F)
                {
                    oddByte2 = 0;
                    oddByte1 += 4;
                }

                evenByte2++;
                if (evenByte2 > 0x01)
                {
                    evenByte2 = 0;
                    evenByte1++;
                }
            }
        }


        private readonly List<HeroElement> m_heroElements = new List<HeroElement>();

        private readonly byte[] m_lobbyBytes;
    }
}