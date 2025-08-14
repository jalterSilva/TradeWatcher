using TimeZoneConverter;

namespace InsiderTrade.Logic
{
    public static class TimeHelper
    {
        public static TimeZoneInfo GetBrtTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"); }
            catch { return TZConvert.GetTimeZoneInfo("America/Sao_Paulo"); }
        }

        public static DateTime NowBrt() =>
            TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, GetBrtTz());

        // ---------- helpers ----------
        private static DateTime FloorToStep(DateTime t, int stepMinutes) =>
            new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute - (t.Minute % stepMinutes), 0);

        private static DateTime CeilToStep(DateTime t, int stepMinutes)
        {
            var floor = FloorToStep(t, stepMinutes);
            return floor == t ? t : floor.AddMinutes(stepMinutes);
        }
        // -----------------------------

        /// Última janela FECHADA de X minutos em BRT (ex.: 15 -> 11:00..11:15)
        public static (DateTime fromBrt, DateTime toBrt) LastClosedWindowBrt(int minutes)
        {
            if (minutes <= 0) throw new ArgumentOutOfRangeException(nameof(minutes));

            var now = NowBrt();
            var boundary = FloorToStep(now, minutes);
            if (boundary >= now) boundary = boundary.AddMinutes(-minutes);

            var toBrt = boundary;
            var fromBrt = boundary.AddMinutes(-minutes);
            return (fromBrt, toBrt);
        }

        /// Última janela fechada no horário do pregão (10:00–17:00 por padrão)
        public static (DateTime fromBrt, DateTime toBrt) LastClosedWindowInSessionBrt(
            int minutes, string open = "10:00", string close = "17:00")
        {
            if (minutes <= 0) throw new ArgumentOutOfRangeException(nameof(minutes));

            var now = NowBrt();

            var openTs = TimeSpan.Parse(open);
            var closeTs = TimeSpan.Parse(close);

            var sessOpen = new DateTime(now.Year, now.Month, now.Day, openTs.Hours, openTs.Minutes, 0);
            var sessClose = new DateTime(now.Year, now.Month, now.Day, closeTs.Hours, closeTs.Minutes, 0);

            DateTime boundary;
            if (now < sessOpen)
            {
                // Antes da abertura -> última janela do dia anterior
                boundary = FloorToStep(sessClose.AddDays(-1), minutes);
            }
            else if (now > sessClose)
            {
                // Depois do fechamento -> última janela do dia
                boundary = FloorToStep(sessClose, minutes);
            }
            else
            {
                // Durante o pregão -> última janela fechada
                boundary = FloorToStep(now, minutes);
                if (boundary >= now) boundary = boundary.AddMinutes(-minutes);
            }

            var toBrt = boundary;
            var fromBrt = boundary.AddMinutes(-minutes);
            return (fromBrt, toBrt);
        }

        /// Gera TODAS as janelas [open..close] do dia BRT (ex.: 10:00→17:00) alinhadas ao step.
        /// Use à noite para reprocessar o pregão inteiro.
        public static IEnumerable<(DateTime fromBrt, DateTime toBrt)> SessionWindowsBrt(
            int minutes, DateTime? dayBrt = null, string open = "10:00", string close = "17:00")
        {
            if (minutes <= 0) throw new ArgumentOutOfRangeException(nameof(minutes));

            var baseDay = (dayBrt ?? NowBrt()).Date;
            var openTs = TimeSpan.Parse(open);
            var closeTs = TimeSpan.Parse(close);

            var sessOpen = new DateTime(baseDay.Year, baseDay.Month, baseDay.Day, openTs.Hours, openTs.Minutes, 0);
            var sessClose = new DateTime(baseDay.Year, baseDay.Month, baseDay.Day, closeTs.Hours, closeTs.Minutes, 0);

            var startFrom = CeilToStep(sessOpen, minutes);   // primeira janela dentro do pregão
            var lastEnd = FloorToStep(sessClose, minutes); // último término alinhado (ex.: 16:45 p/ 17:00)

            for (var from = startFrom; from < lastEnd; from = from.AddMinutes(minutes))
                yield return (from, from.AddMinutes(minutes));
        }

        // ✅ Formato aceito pela OpLab (sem timezone):
        public static string ToOpLabString(DateTimeOffset dtBrt) =>
            dtBrt.ToString("yyyy-MM-dd'T'HH:mm");
    }
}
