namespace Elders.Pandora.Api
{
    public static class StringExtensions
    {
        public static string EnsureTrailingSlash(this string input)
        {
            if (!input.EndsWith("/"))
            {
                return input + "/";
            }

            return input;
        }
    }
}