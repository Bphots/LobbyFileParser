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
        static void Main(string[] args)
        {
            var lobbyProcesser = new LobbyFileProcessor(args[0], HeroLists.Heroes, MapAttributesDict.MapAttributes);
            var game           = lobbyProcesser.ParseLobbyInfo();

            Console.WriteLine("Game.Region = {0}", game.Region);
            for (int i = 0; i < game.Players.Count; i++)
            {
                Console.WriteLine("Game.Player[{0}]: Team = {1}, Tag = {2}, SelectedHero = {3}", i, game.Players[i].Team, game.Players[i].Tag, game.Players[i].SelectedHero);
            }
            Console.WriteLine("Game.Map: {0}", game.Map);
        }
    }
}
