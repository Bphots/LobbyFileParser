using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            if (game.Players.Count == 5 && heroes.GetRange(0, 5).All(h => h == SelectedHeroParser.Random))
            {
                heroes.RemoveRange(0, 5);
            }

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
            byte evenByte1 = 2;
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
                if (oddByte2 > 0x07)
                {
                    oddByte2 = 0;
                    oddByte1 += 1;
                }
                
                evenByte2 = 0;
                evenByte1++;
            }
        }


        private readonly List<HeroElement> m_heroElements = new List<HeroElement>();

        private readonly byte[] m_lobbyBytes;
    }
}