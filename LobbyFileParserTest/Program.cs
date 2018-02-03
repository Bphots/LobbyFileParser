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
            var lobbyProcesser = new LobbyFileProcessor(args[0], HeroLists.Heroes);
            lobbyProcesser.ParseLobbyInfo();
        }
    }
}
