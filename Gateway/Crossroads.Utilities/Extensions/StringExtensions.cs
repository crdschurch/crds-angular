namespace Crossroads.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string Right(this string str, int length)
        {
            str = str ?? string.Empty;
            return str.Length >= length ? str.Substring(str.Length - length, length) : str;
        }
    }
}