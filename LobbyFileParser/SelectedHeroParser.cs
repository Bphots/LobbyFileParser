using System.Collections.Generic;
using System.Linq;

namespace LobbyFileParser
{
    public class SelectedHeroParser
    {
        public const string Random = "Random";
        public const string Fail = "Fail";

        private static readonly byte[] RandomOddBytes = { 0x00, 0x01 };
        private static readonly byte[] RandomEvenBytes = { 0x01, 0x00 };
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
            var offset = m_lobbyBytes.Find(new byte[] {0x73, 0x32, 0x6D, 0x76, 0, 0}) - 0x32B;
            
            var oddOffsetStart = offset;
            var oddOffset = oddOffsetStart;
            for (; oddOffset <= oddOffsetStart + 12; oddOffset += 3)
            {
                selectedHeroes.Add(ParseOddHeroSelection(oddOffset));
                var evenOffset = oddOffset + 1;
                selectedHeroes.Add(ParseEvenHeroSelection(evenOffset));
            }
            

            return selectedHeroes;
        }

        private string ParseOddHeroSelection(int oddOffset)
        {
            var desiredFirstByte = m_lobbyBytes[oddOffset];
            var desiredSecondByte = m_lobbyBytes[oddOffset + 1];
            var hero = m_heroes.FirstOrDefault(
                    h => h.OddByte1 == desiredFirstByte
                         && h.OddByte2 == desiredSecondByte)?.Name;

            if (hero != null)
                return hero;

            if (RandomOddBytes[0] == desiredFirstByte 
                && RandomOddBytes[1] == desiredSecondByte)
                return Random;

            return Fail;
        }

        private string ParseEvenHeroSelection(int evenOffset)
        {
            var desiredFirstByte = m_lobbyBytes[evenOffset] / 16;
            var desiredSecondByte = m_lobbyBytes[evenOffset + 1];
            var hero =
                m_heroes.FirstOrDefault(
                h =>
                    h.EvenByte1 == desiredFirstByte && h.EvenByte2 == desiredSecondByte)?.Name;

            if (hero != null)
                return hero;

            if (desiredFirstByte == RandomEvenBytes[0] && desiredSecondByte == RandomEvenBytes[1])
                return Random;

            return Fail;
        }
    }
}