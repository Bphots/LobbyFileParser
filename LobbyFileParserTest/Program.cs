using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using LobbyFileParser;

namespace LobbyFileParserTest
{
    class Program
    {
        private static List<LobbyParameter> LookForMatch(LobbyFileProcessor lobbyFileProcessor)
        {
            //var expectedHeroes = new List<string>()
            //    {
            //        @"Maiev",
            //        @"Abathur",
            //        @"Alarak",
            //        @"Alexstrasza",
            //        @"Ana",
            //        @"Auriel",
            //        @"Blaze",
            //        @"Cassia",
            //        @"Yrel",
            //        @"Diablo"
            //    };
            var expectedHeroes = new List<string>()
                {
                    @"Zeratul",
                    @"Zul'jin",
                    @"Nazeebo",
                    @"Valeera",
                     @"Muradin",
                     @"Orphea",
                     @"Tyrande",
                     @"Valla",
                     @"Tyrael",
                     @"Tracer"
                };

            return lobbyFileProcessor.LookForMatches(expectedHeroes, 0x39A, HeroLists.Heroes);
        }

        static void Main(string[] args)
        {
            var lobbyProcessor = new LobbyFileProcessor(args[0]);
            var results = LookForMatch(lobbyProcessor);

            var lobbyParameter = new LobbyParameter()
                {
                    OddByte1 = 0,
                    OddByte2 = 2,
                    EvenByte1 = 0,
                    EvenByte2 = 2,
                    OddNotation = 4,
                    EvenNotation = 64,
                    OddIncrement = 1,
                    EvenIncrement = 8,
                    OffSet = 0x39A,
                    RandomOddByte1 = 0,
                    RandomOddByte2 = 1,
                    RandomEvenByte1 = 0,
                    RandomEvenByte2 = 1,
                    StartWithOdd = true
                };

            var game           = lobbyProcessor.ParseLobbyInfo(lobbyParameter, HeroLists.Heroes, MapLists.Maps);

            /*
            Console.WriteLine("Game.Region = {0}", game.Region);
            for (int i = 0; i < game.Players.Count; i++)
            {
                Console.WriteLine("Game.Player[{0}]: Team = {1}, Tag = {2}, SelectedHero = {3}", i, game.Players[i].Team, game.Players[i].Tag, game.Players[i].SelectedHero);
            }*/
            Console.WriteLine("Game.Map: {0}", game.Map);
        }
    }
}
