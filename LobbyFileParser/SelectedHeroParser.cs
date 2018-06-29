using System.Collections.Generic;
using System.Linq;

namespace LobbyFileParser
{
    public class SelectedHeroParser
    {
        public const string Random = "Random";
        public const string Fail = "Fail";

        private static readonly byte[] RandomBytes = { 0x00, 0x00 };
        private readonly List<HeroElement> m_heroes;
        private readonly byte[] m_lobbyBytes;

        public SelectedHeroParser(byte[] data, List<HeroElement> heroElements)
        {
            m_lobbyBytes = data;
            m_heroes = heroElements;
        }

        public List<string> ParseHeroesInfo()
        {
            var selectedHeroes = new List<string>();
            var offset = m_lobbyBytes.Find(new byte[] {0x73, 0x32, 0x6D, 0x76, 0, 0}) - 0x30B;
            
            var oddOffsetStart = offset;

            for (var oddOffset = oddOffsetStart; oddOffset <= oddOffsetStart + 12; oddOffset += 3)
            {
                selectedHeroes.Add(ParseOddHeroSelection(oddOffset));
                var evenOffset = oddOffset + 1;
                selectedHeroes.Add(ParseEvenHeroSelection(evenOffset));
            }
            
            return selectedHeroes;
        }

        private string ParseOddHeroSelection(int oddOffset)
        { 
            var hero = m_heroes.FirstOrDefault(
                    h => h.OddByte1 == m_lobbyBytes[oddOffset] && h.OddByte2 == m_lobbyBytes[oddOffset + 1] % 2)?.Name;

            if (hero != null)
                return hero;

            if (m_lobbyBytes[oddOffset] == RandomBytes[0] && m_lobbyBytes[oddOffset + 1] % 2 == RandomBytes[1])
                return Random;

            return Fail;
        }

        private string ParseEvenHeroSelection(int evenOffset)
        {
            var hero =
                m_heroes.FirstOrDefault(
                h =>
                    h.EvenByte1 ==
                    (m_lobbyBytes[evenOffset] % 2 == 0 ? m_lobbyBytes[evenOffset] : m_lobbyBytes[evenOffset] - 1) && h.EvenByte2 ==
                    m_lobbyBytes[evenOffset + 1])?.Name;

            if (hero != null)
                return hero;

            if (m_lobbyBytes[evenOffset] == RandomBytes[0] && m_lobbyBytes[evenOffset + 1] == RandomBytes[1])
                return Random;

            return Fail;
        }
    }
}