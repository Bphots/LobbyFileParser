namespace LobbyFileParser
{
    /// <summary>
    ///     https://github.com/poma/HotsStats
    /// </summary>
    public static class BytesHelper
    {
        /// <summary>
        /// Search for pattern in data array
        /// </summary>
        public static int Find(this byte[] data, byte[] pattern, int offset = 0)
        {
            for (int i = offset; i < data.Length - pattern.Length; i++)
                if (Match(data, pattern, i))
                    return i;

            return -1;
        }

        /// <summary>
        /// Try to match pattern at certain offset
        /// </summary>
        public static bool Match(this byte[] data, byte[] pattern, int offset = 0)
        {
            for (int i = 0; i < pattern.Length; i++)
                if (data[offset + i] != pattern[i])
                    return false;

            return true;
        }
    }
}
