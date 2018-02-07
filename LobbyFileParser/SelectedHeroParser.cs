using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobbyFileParser
{
    public class SelectedHeroParser
    {
        public SelectedHeroParser(byte[] data, List<HeroElement> heroElements)
        {
            m_lobbyBytes = data;
            m_heroes = heroElements;
        }

        public List<string> ParseHeroesInfo()
        {
            var selectedHeroes = new List<string>();
            int offset = m_lobbyBytes.Find(new byte[] { 0x73, 0x32, 0x6D, 0x76, 0, 0 }) - 0x32D;

            var evenOffsetStart = offset; // version specific

            for (var evenOffset = evenOffsetStart; evenOffset <= evenOffsetStart + 12; evenOffset += 3)
            {
                selectedHeroes.Add(ParseEvenHeroSelection(evenOffset));
                var oddOffset = evenOffset + 1;
                selectedHeroes.Add(ParseOddHeroSelection(oddOffset));
            }

            return selectedHeroes;
        }

        private string ParseOddHeroSelection(int oddOffset)
        {
            return m_heroes.FirstOrDefault(
                h =>
                    h.OddByte1 ==
                    (m_lobbyBytes[oddOffset] % 2 == 0 ? m_lobbyBytes[oddOffset] : m_lobbyBytes[oddOffset] - 1) && h.OddByte2 ==
                    m_lobbyBytes[oddOffset + 1])?.Name ?? "Random";
        }

        private string ParseEvenHeroSelection(int evenOffset)
        {
            return
                m_heroes.FirstOrDefault(
                    h => h.EvenByte1 == m_lobbyBytes[evenOffset] && h.EvenByte2 == m_lobbyBytes[evenOffset + 1] % 2)?.Name ??
                "Random";
        }

        private readonly byte[] m_lobbyBytes;
        private readonly List<HeroElement> m_heroes;
    }
}
