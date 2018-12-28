using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyFileParser
{
    public class SelectedHeroParser
    {
        public const string Random = "Random";
        public const string Fail = "Fail";

        private readonly byte[] m_randomOddBytes = { 0x00, 0x00 };
        private readonly byte[] m_randomEvenBytes = { 0x00, 0x00 };

        private readonly List<HeroElement> m_heroElements = new List<HeroElement>();
        private readonly byte[] m_lobbyBytes;
        private readonly List<string> m_heroes;

        public SelectedHeroParser(byte[] data, List<string> heroes)
        {
            m_lobbyBytes = data;
            m_heroes = heroes;
        }

        public List<string> ParseHeroesInfo(LobbyParameter lobbyParameter)
        {
            var selectedHeroes = new List<string>();
            var offset = m_lobbyBytes.Find(new byte[] { 0x73, 0x32, 0x6D, 0x76, 0, 0 }) - lobbyParameter.OffSet;

            InitializeHeroes(lobbyParameter.OddByte1, lobbyParameter.OddByte2, lobbyParameter.EvenByte1, lobbyParameter.EvenByte2, lobbyParameter.OddNotation, lobbyParameter.EvenNotation, lobbyParameter.OddIncrement, lobbyParameter.EvenIncrement);
            m_randomOddBytes[0] = lobbyParameter.RandomOddByte1;
            m_randomOddBytes[1] = lobbyParameter.RandomOddByte2;
            m_randomEvenBytes[0] = lobbyParameter.RandomEvenByte1;
            m_randomEvenBytes[1] = lobbyParameter.RandomEvenByte2;

            if (lobbyParameter.StartWithOdd)
                FivePairsAttempt(offset, selectedHeroes, lobbyParameter.EvenIncrement);
            else
                SevenPairsAttempt(offset, selectedHeroes, lobbyParameter.OddIncrement);

            return selectedHeroes;
        }

        public List<LobbyParameter> LookForMatches(List<string> expectedHeroes, int startingOffset)
        {
            var selectedHeroes = new List<string>();
            var offset = m_lobbyBytes.Find(new byte[] { 0x73, 0x32, 0x6D, 0x76, 0, 0 }) - startingOffset;
            var successfulLobbyParameters = new List<LobbyParameter>();

            var possibleByteStarts = new[] { new byte[] { 0, 2, 0, 2 }, new byte[] { 2, 0, 0, 2 }, new byte[] { 0, 2, 1, 0 }, new byte[] { 2, 0, 2, 0 } };
            // 9 * 9 * 9 * 9 * 4 / 2
            for (byte oddNotation = 1; oddNotation > 0; oddNotation *= 2)
            {
                for (byte oddIncrement = 1; oddIncrement > 0; oddIncrement *= 2)
                {
                    for (byte evenNotation = 1; evenNotation > 0; evenNotation *= 2)
                    {
                        for (byte evenIncrement = 1; evenIncrement > 0; evenIncrement *= 2)
                        {
                            if (evenIncrement < oddNotation && oddIncrement < evenNotation)
                                continue;

                            
                            foreach (var possibleByteStart in possibleByteStarts)
                            {
                                InitializeHeroes(
                                    possibleByteStart[0], possibleByteStart[1], possibleByteStart[2], possibleByteStart[3], oddNotation, evenNotation, oddIncrement, evenIncrement);

                                selectedHeroes.Clear();
                                if (evenIncrement > oddNotation && FivePairsAttempt(offset, selectedHeroes, evenIncrement))
                                {
                                    if (expectedHeroes.All(h => selectedHeroes.Contains(h)))
                                    {
                                        var lobbyParameter = new LobbyParameter()
                                            {
                                                OddByte1 = possibleByteStart[0],
                                                OddByte2 = possibleByteStart[1],
                                                EvenByte1 = possibleByteStart[2],
                                                EvenByte2 = possibleByteStart[3],
                                                OddNotation = oddNotation,
                                                EvenNotation = evenNotation,
                                                OddIncrement = oddIncrement,
                                                EvenIncrement = evenIncrement,
                                                OffSet = startingOffset,
                                                StartWithOdd = true
                                            };

                                        successfulLobbyParameters.Add(lobbyParameter);
                                    }
                                }

                                selectedHeroes.Clear();
                                if (oddIncrement > evenNotation && SevenPairsAttempt(offset, selectedHeroes, oddIncrement))
                                {
                                    if (expectedHeroes.All(h => selectedHeroes.Contains(h)))
                                    {
                                        var lobbyParameter = new LobbyParameter()
                                            {
                                                OddByte1 = possibleByteStart[0],
                                                OddByte2 = possibleByteStart[1],
                                                EvenByte1 = possibleByteStart[2],
                                                EvenByte2 = possibleByteStart[3],
                                                OddNotation = oddNotation,
                                                EvenNotation = evenNotation,
                                                OddIncrement = oddIncrement,
                                                EvenIncrement = evenIncrement,
                                                OffSet = startingOffset,
                                                StartWithOdd = false
                                        };

                                        successfulLobbyParameters.Add(lobbyParameter);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return successfulLobbyParameters;
        }

        private static bool IsSuccessful(IList<string> selectedHeroes)
        {
            return !selectedHeroes.Contains(Fail) && !selectedHeroes.Contains(Random) && selectedHeroes.Count == selectedHeroes.Distinct().Count();
        }

        private void InitializeHeroes(byte oddByte1, byte oddByte2, byte evenByte1, byte evenByte2,
                                      byte oddNotation, byte evenNotation, byte oddIncrement, byte evenIncrement)
        {
            m_heroElements.Clear();

            foreach (var hero in m_heroes)
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
                if (oddByte2 >= oddNotation)
                {
                    oddByte2 = 0;
                    oddByte1 += oddIncrement;
                }

                evenByte2++;
                if (evenByte2 >= evenNotation)
                {
                    evenByte2 = 0;
                    evenByte1 += evenIncrement;
                }
            }
        }


        private bool FivePairsAttempt(int offset, List<string> selectedHeroes, int evenIncrement)
        {
            selectedHeroes.Clear();
            var oddOffsetStart = offset;
            var oddOffset = oddOffsetStart;
            for (; oddOffset <= oddOffsetStart + 12; oddOffset += 3)
            {
                selectedHeroes.Add(ParseOddHeroSelectionStartOddFirst(oddOffset, evenIncrement));
                var evenOffset = oddOffset + 1;
                selectedHeroes.Add(ParseEvenHeroSelectionStartOddFirst(evenOffset, evenIncrement));
            }

            return IsSuccessful(selectedHeroes);
        }
        private bool SevenPairsAttempt(int offset, List<string> selectedHeroes, int oddIncrement)
        {
            selectedHeroes.Clear();
            var oddOffsetStart = offset;
            selectedHeroes.Add(ParseOddHeroSelectionStartEvenFirst(oddOffsetStart, oddIncrement));
            var evenOffset = oddOffsetStart + 2;
            for (; evenOffset <= oddOffsetStart + 12; evenOffset += 3)
            {
                selectedHeroes.Add(ParseEvenHeroSelectionStartEvenFirst(evenOffset, oddIncrement));
                var oddOffset = evenOffset + 1;
                selectedHeroes.Add(ParseOddHeroSelectionStartEvenFirst(oddOffset, oddIncrement));
            }

            selectedHeroes.Add(ParseEvenHeroSelectionStartEvenFirst(evenOffset, oddIncrement));

            return IsSuccessful(selectedHeroes);
        }

        private string ParseOddHeroSelectionStartEvenFirst(int oddOffset, int oddIncrement)
        {
            var desiredFirstByte = m_lobbyBytes[oddOffset] / (oddIncrement) * (oddIncrement);
            var desiredSecondByte = m_lobbyBytes[oddOffset + 1];
            var hero = m_heroElements.FirstOrDefault(
                h => h.OddByte1 == desiredFirstByte
                     && h.OddByte2 == desiredSecondByte)?.Name;

            if (hero != null)
                return hero;

            if (m_randomOddBytes[0] == desiredFirstByte
                && m_randomOddBytes[1] == desiredSecondByte)
                return Random;

            if (desiredFirstByte == 0x00 && desiredSecondByte == 0x00)
                return Random;

            return Fail;
        }

        private string ParseEvenHeroSelectionStartEvenFirst(int evenOffset, int oddIncrement)
        {
            var desiredFirstByte = m_lobbyBytes[evenOffset];
            var desiredSecondByte = m_lobbyBytes[evenOffset + 1] % (oddIncrement);
            var hero =
                m_heroElements.FirstOrDefault(
                    h =>
                        h.EvenByte1 == desiredFirstByte && h.EvenByte2 == desiredSecondByte)?.Name;

            if (hero != null)
                return hero;

            if (desiredFirstByte == m_randomEvenBytes[0] && desiredSecondByte == m_randomEvenBytes[1])
                return Random;

            if (desiredFirstByte == 0x00 && desiredSecondByte == 0x00)
                return Random; 

            return Fail;
        }

        private string ParseOddHeroSelectionStartOddFirst(int oddOffset, int evenIncrement)
        {
            var desiredFirstByte = m_lobbyBytes[oddOffset];
            var desiredSecondByte = m_lobbyBytes[oddOffset + 1] % (evenIncrement);
            var hero = m_heroElements.FirstOrDefault(
                    h => h.OddByte1 == desiredFirstByte
                         && h.OddByte2 == desiredSecondByte)?.Name;

            if (hero != null)
                return hero;

            if (m_randomOddBytes[0] == desiredFirstByte 
                && m_randomOddBytes[1] == desiredSecondByte)
                return Random;

            if (desiredFirstByte == 0x00 && desiredSecondByte == 0x00)
                return Random;

            return Fail;
        }

        private string ParseEvenHeroSelectionStartOddFirst(int evenOffset, int evenIncrement)
        {
            var desiredFirstByte = m_lobbyBytes[evenOffset] / (evenIncrement) * (evenIncrement);
            var desiredSecondByte = m_lobbyBytes[evenOffset + 1];
            var hero =
                m_heroElements.FirstOrDefault(
                h =>
                    h.EvenByte1 == desiredFirstByte && h.EvenByte2 == desiredSecondByte)?.Name;

            if (hero != null)
                return hero;

            if (desiredFirstByte == m_randomEvenBytes[0] && desiredSecondByte == m_randomEvenBytes[1])
                return Random;

            if (desiredFirstByte == 0x00 && desiredSecondByte == 0x00)
                return Random;

            return Fail;
        }
    }
}