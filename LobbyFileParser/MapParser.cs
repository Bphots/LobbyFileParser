using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobbyFileParser
{
    public class MapParser
    {
        public MapParser(byte[] data, Dictionary<string, string> mapAttributes)
        {
            m_lobbyBytes = data;
            m_mapAttributes = mapAttributes;
        }

        public string Parse()
        {
            foreach (var item in m_mapAttributes)
            {
                if (BytesHelper.Match(m_lobbyBytes, System.Text.Encoding.Default.GetBytes(item.Key), 662))
                {
                    return item.Value;
                }
            }
            return "unknown map";
        }

        private readonly byte[] m_lobbyBytes;
        private readonly Dictionary<string, string> m_mapAttributes;
    }
}