using TimeZoneConverter;

namespace InsiderTrade.Logic
{
    public static class TimeHelper
    {
        // Pega o fuso de Brasília em Windows ou Linux
        public static TimeZoneInfo GetBrtTz()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
            catch
            {
                return TZConvert.GetTimeZoneInfo("America/Sao_Paulo");
            }
        }

        public static DateTime NowBrt()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, GetBrtTz());
        }

        public static (DateTime fromBrt, DateTime toBrt) LastClosedWindowBrt(int minutes)
        {
            var now = NowBrt();
            var boundary = new DateTime(now.Year, now.Month, now.Day, now.Hour,
                now.Minute - (now.Minute % minutes), 0);

            // Se boundary for igual ou depois de agora, volta um intervalo
            if (boundary >= now)
                boundary = boundary.AddMinutes(-minutes);

            var toBrt = boundary;
            var fromBrt = boundary.AddMinutes(-minutes);
            return (fromBrt, toBrt);
        }

        // Formato esperado pela OpLab
        public static string ToOpLabString(DateTime dtBrt)
        {
            return dtBrt.ToString("yyyy-MM-ddHH:mm:ss");
        }
    }
}
