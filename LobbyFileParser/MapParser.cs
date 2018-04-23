using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Foole.Mpq;

namespace LobbyFileParser
{
    public class MapParser
    {
        public MapParser(byte[] data, List<string> maps)
        {
            m_lobbyBytes = data;
            m_maps       = maps;
        }

        public string Parse()
        {
            var s2MaPathNum = m_lobbyBytes[0];
            var s2MaPathLen = m_lobbyBytes[1];
            var dict        = new Dictionary<string, int>();

            foreach (var map in m_maps)
                dict.Add(map, 0);

            for (var i = 0; i < s2MaPathNum; i++)
            {
                var s2MaPath = Encoding.Default.GetString(m_lobbyBytes.Skip(2 + i * (s2MaPathLen + 2)).Take(s2MaPathLen).ToArray());
                try
                {
                    var tempPath = Path.GetTempFileName();
                    File.Copy(s2MaPath, tempPath, true);
                    s2MaPath = tempPath;
                    var mpq = new MpqArchive(s2MaPath);
                    if (mpq.FileExists("DocumentInfo"))
                    {
                        using (var ms = mpq.OpenFile("DocumentInfo"))
                        {
                            var buffer = new byte[ms.Length];
                            ms.Read(buffer, 0, (int)ms.Length);
                            var str = Encoding.Default.GetString(buffer);
                            foreach (var map in m_maps)
                                dict[map] += Regex.Matches(str, map.Substring(0,8)).Count;
                        }
                    }
                    File.Delete(tempPath);
                }
                catch
                {
                }
            }

            var count = 0;
            var mapName = "Unknown Map";
            foreach (var map in m_maps)
            {
                if (dict[map] > count)
                {
                    count = dict[map];
                    mapName = map;
                }
            }
            return mapName;
        }

        private readonly byte[] m_lobbyBytes;
        private readonly List<string> m_maps;
    }
}