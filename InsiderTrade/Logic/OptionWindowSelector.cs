using System.Text.Json.Serialization;

namespace InsiderTrade.Logic
{
    public record OptionInfo(
       [property: JsonPropertyName("symbol")] string Symbol,
       [property: JsonPropertyName("name")] string Name,
       [property: JsonPropertyName("strike")] decimal Strike,
       [property: JsonPropertyName("due_date")] DateTime? DueDate,
       [property: JsonPropertyName("maturity_type")] string? MaturityType
   );

    public class OptionWindowSelector
    {
        public static IEnumerable<OptionInfo> OnlyMonthly(IEnumerable<OptionInfo> list) =>
           list.Where(o => !(o.Symbol.Length >= 2 && char.ToUpperInvariant(o.Symbol[^2]) == 'W'));

        public static string SeriesLetter(string symbol) =>
            symbol.Length >= 5 ? symbol.Substring(4, 1) : string.Empty;

        public static int MonthFromSeriesLetter(string s) => s switch
        {
            "A" => 1,
            "B" => 2,
            "C" => 3,
            "D" => 4,
            "E" => 5,
            "F" => 6,
            "G" => 7,
            "H" => 8,
            "I" => 9,
            "J" => 10,
            "K" => 11,
            "L" => 12,
            "M" => 1,
            "N" => 2,
            "O" => 3,
            "P" => 4,
            "Q" => 5,
            "R" => 6,
            "S" => 7,
            "T" => 8,
            "U" => 9,
            "V" => 10,
            "W" => 11,
            "X" => 12,
            _ => 0
        };

        public static int CycleYearForLetter(string s, DateTime today)
        {
            var m = MonthFromSeriesLetter(s);
            if (m == 0) return today.Year;
            return (m >= today.Month) ? today.Year : today.Year + 1;
        }

        public static List<OptionInfo> WindowPerSeriesFillNextYear(
           List<OptionInfo> list, decimal spot, int targetPerSide, List<string> seriesOrder, DateTime today)
        {
            var result = new List<OptionInfo>();

            foreach (var s in seriesOrder)
            {
                var seriesList = list.Where(o => SeriesLetter(o.Symbol) == s).ToList();
                if (seriesList.Count == 0) continue;

                var cycleYear = CycleYearForLetter(s, today);
                var nextYear = cycleYear + 1;

                var curr = seriesList.Where(o => !o.DueDate.HasValue || o.DueDate!.Value.Year == cycleYear).ToList();
                var next = seriesList.Where(o => o.DueDate.HasValue && o.DueDate!.Value.Year == nextYear).ToList();

                var currStrikes = curr.Select(x => x.Strike).Distinct().OrderBy(x => x).ToList();
                var nextStrikes = next.Select(x => x.Strike).Distinct().OrderBy(x => x).ToList();

                var baseStrikes = currStrikes.Count > 0 ? currStrikes : nextStrikes;
                if (baseStrikes.Count == 0) continue;

                int nearestIdx = NearestIndex(baseStrikes, spot);

                var below = PickBelow(baseStrikes, targetPerSide, spot);
                var above = PickAbove(baseStrikes, targetPerSide, spot);

                if (below.Count < targetPerSide && nextStrikes.Count > 0)
                {
                    var need = targetPerSide - below.Count;
                    var add = nextStrikes.Where(k => k < spot).Reverse().Take(need).ToList();
                    below.AddRange(add);
                }
                if (above.Count < targetPerSide && nextStrikes.Count > 0)
                {
                    var need = targetPerSide - above.Count;
                    var add = nextStrikes.Where(k => k > spot).Take(need).ToList();
                    above.AddRange(add);
                }

                var pickedStrikes = new HashSet<decimal>(below.Concat(above).Concat(new[] { baseStrikes[nearestIdx] }));
                var picked = seriesList.Where(o => pickedStrikes.Contains(o.Strike)).ToList();
                result.AddRange(picked);
            }

            return result
                .OrderBy(o => seriesOrder.IndexOf(SeriesLetter(o.Symbol)))
                .ThenBy(o => o.Strike)
                .ToList();
        }

        private static int NearestIndex(List<decimal> strikes, decimal spot)
        {
            var idx = 0; var best = decimal.MaxValue;
            for (int i = 0; i < strikes.Count; i++)
            {
                var d = Math.Abs(strikes[i] - spot);
                if (d < best) { best = d; idx = i; }
            }
            return idx;
        }

        private static List<decimal> PickBelow(List<decimal> strikes, int n, decimal spot)
        {
            var res = new List<decimal>();
            var allBelow = strikes.Where(k => k < spot).ToList();
            for (int i = allBelow.Count - 1; i >= 0 && res.Count < n; i--) res.Add(allBelow[i]);
            return res;
        }

        private static List<decimal> PickAbove(List<decimal> strikes, int n, decimal spot)
        {
            var res = new List<decimal>();
            var allAbove = strikes.Where(k => k > spot).ToList();
            for (int i = 0; i < allAbove.Count && res.Count < n; i++) res.Add(allAbove[i]);
            return res;
        }

    }
}
