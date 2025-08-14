namespace InsiderTrade.Helper
{
    public static class SeriesHelper
    {
        public static string OptionRoot(string underlying) // "ITUB4" -> "ITUB"
        => new(underlying.Where(char.IsLetter).ToArray());

        public static IEnumerable<string> TakeSeries(List<string> all, string current, int take = 3)
        {
            var idx = all.FindIndex(s => s.Equals(current, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) yield break;

            for (int k = 0; k < take; k++)
                yield return all[(idx + k) % all.Count];
        }
    }
}
