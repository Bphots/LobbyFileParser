using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobbyFileParser
{
    public class SelectedHeroParser
    {
        public const string Random = "Random";

        public SelectedHeroParser(byte[] data, List<HeroElement> heroElements)
        {
            m_lobbyBytes = data;
            m_heroes = heroElements;
        }

        public List<string> ParseHeroesInfo()
        {
            var selectedHeroes = new List<string>();
            int offset = m_lobbyBytes.Find(new byte[] { 0x73, 0x32, 0x6D, 0x76, 0, 0 }) - 0x32D;

            var firstSelectedHero =
                m_heroes.FirstOrDefault(
                    h => h.OddByte1 == m_lobbyBytes[offset] && h.OddByte2 == m_lobbyBytes[offset + 1])?.Name ?? Random;
            selectedHeroes.Add(firstSelectedHero);

            var evenOffsetStart = offset + 2;

            for (var evenOffset = evenOffsetStart; evenOffset <= evenOffsetStart + 9; evenOffset += 3)
            {
                selectedHeroes.Add(ParseEvenHeroSelection(evenOffset));
                var oddOffset = evenOffset + 1;
                selectedHeroes.Add(ParseOddHeroSelection(oddOffset));
            }

            var evenOffsetEnd = evenOffsetStart + 12;
            selectedHeroes.Add(ParseEvenHeroSelection(evenOffsetEnd));

            return selectedHeroes;
        }

        private string ParseOddHeroSelection(int oddOffset)
        {
            return m_heroes.FirstOrDefault(
                h =>
                    h.OddByte1 == m_lobbyBytes[oddOffset] && h.OddByte2 ==
                    m_lobbyBytes[oddOffset + 1])?.Name ?? Random;
        }

        private string ParseEvenHeroSelection(int evenOffset)
        {
            return
                m_heroes.FirstOrDefault(
                    h => h.EvenByte1 == m_lobbyBytes[evenOffset])?.Name ??
                Random;
        }

        private readonly byte[] m_lobbyBytes;
        private readonly List<HeroElement> m_heroes;
    }
}
