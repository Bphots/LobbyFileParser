using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LobbyFileParser
{
    /// <summary>
    ///     The lobby parser for battle tags & regions 
    ///     https://github.com/poma/HotsStats
    /// </summary>
    public class TagAndRegionParser
    {
        private readonly int MaxTagByteLength = 32; // Longest player name is 12 letters. Unicode is allowed so 25 bytes + 7 for digits seems reasonable (but technically could be much more)
        private readonly string TagRegex = @"^\w{2,12}#\d{4,8}$";
        private readonly byte[] data;
        
        public TagAndRegionParser(byte[] data)
        {
            this.data = data;
            ExtractBattleTagsRegex();
        }

        public Game Parse()
        {
            var game = new Game();
            game.Region = ExtractRegion();
            var tags = ExtractBattleTagsNoboundaries();
            game.Players = tags.Select(tag => new Player(tag)).ToList();
            for (int i = 0; i < game.Players.Count; i++)
            {
                game.Players[i].Team = i >= 5 ? 1 : 0;
            }
            return game;
        }

        // Since we don't know structure of this file we will search for anything that looks like BattleTag
        // We know that BattleTags reside at file end after large '0' padding
        public List<string> ExtractBattleTagsRegex()
        {
            var result = new List<string>();

            var initialOffset = data.Find(Enumerable.Repeat<byte>(0, 32).ToArray());

            var strings = GetStrings(initialOffset, 8, MaxTagByteLength);

            foreach (var str in strings)
            {
                string s;
                try
                {
                    s = Encoding.UTF8.GetString(data, str.Item1, str.Item2);
                }
                catch (ArgumentException)
                {
                    continue; // not a valid string
                }

                if (!Regex.IsMatch(s, TagRegex))
                    continue;

                if (s.StartsWith("T:"))
                    continue;

                result.Add(s);

                if (result.Count == 10)
                    break; // do not parse observers
            }

            return result;
        }

        // Look for '#' symbol with digits on the right and letters to the left, prefixed with string length
        public List<string> ExtractBattleTagsNoboundaries()
        {
            var result = new List<string>();

            var offset = data.Find(Enumerable.Repeat<byte>(0, 32).ToArray());

            while (true)
            {
                offset = data.Find(new byte[] { (byte)'#' }, offset + 1);
                if (offset == -1)
                    break;
                var tag = TryExtractBattleTag(offset);
                if (tag != null)
                {
                    if (tag.Any(l => l > 512))
                    {
                        int index = tag.IndexOf(tag.First(l => l > 512));
                        tag = tag.Substring(index);
                    }

                    result.Add(tag);
                }
                if (result.Count == 10)
                    break; // do not parse observers
            }

            return result;
        }

        /// <summary>
        /// Extract region
        /// </summary>
        public Region ExtractRegion()
        {
            // looks like region is always follows this pattern
            var i = data.Find(new byte[] { (byte)'s', (byte)'2', (byte)'m', (byte)'h', 0, 0 });
            if (i == -1)
                throw new ApplicationException("Can't parse region");
            else
            {
                var region = new string(new char[] { (char)data[i + 6], (char)data[i + 7] });
                Region result;
                if (!Enum.TryParse<Region>(region, out result))
                    throw new ApplicationException("Can't parse region");
                return result;
            }
        }

        /// <summary>
        /// Try to extract BattleTag given position of '#' symbol
        /// </summary>
        private string TryExtractBattleTag(int offset)
        {
            string tag = "#";

            // look for digits to the right
            for (int i = 1; i < 10; i++)
            {
                var c = (char)data[offset + i];
                if (char.IsDigit(c))
                    tag += c;
                else
                    break;
            }

            // 3 digits for tag is too short and 9 is too much
            if (tag.Length < 5 || tag.Length > 9)
                return null;

            string name = null;
            for (int i = MaxTagByteLength - 1; i >= 3; i--)
            {
                try
                {
                    name = Encoding.UTF8.GetString(data, offset - i, i);
                    break;
                }
                catch (ArgumentException)
                {
                    // continue;
                }
            }
            if (name == null)
                return null;
            var m = Regex.Match(name, @"\w{2,12}$");
            if (m.Success)
            {
                return m.Value + tag;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Look for all possible strings in data array
        /// </summary>
        /// <param name="offset">Search offset</param>
        /// <param name="minLength">Minimum string length</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Returns offset-length pairs</returns>
        private List<Tuple<int, int>> GetStrings(int offset = 0, int minLength = 0, int maxLength = 255)
        {
            var result = new List<Tuple<int, int>>();
            for (int i = offset; i < data.Length; i++)
            {
                if (data[i] >= minLength && data[i] <= maxLength && i + data[i] + 1 < data.Length)
                {
                    result.Add(new Tuple<int, int>(i + 1, data[i]));
                }
            }
            return result;
        }
    }

    public class Game
    {
        public Region Region { get; set; }

        public List<Player> Players { get; set; }
    }

    public class Player
    {
        public Player(string tag)
        {
            if (char.IsDigit(tag[0]))
                tag = tag.Substring(1);

            Tag = tag;
        }

        public string Tag { get; set; }

        public int Team { get; set; }

        public string SelectedHero { get; set; }
    }
    public enum Region
    {
        US = 1,
        EU = 2,
        KR = 3,
        CN = 5,
        XX = -1 // PTR
    }
}
