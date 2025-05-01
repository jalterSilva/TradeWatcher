namespace TradeWatcher.Service
{
    public class TelegramService
    {
        private readonly string _botToken;
        //private readonly string _chatId;
        private readonly List<string> _chatIds;
        private readonly HttpClient _httpClient;

        public TelegramService(IConfiguration configuration)
        {
            var telegramConfig = configuration.GetSection("Telegram");
            _botToken = telegramConfig.GetValue<string>("BotToken") ?? throw new ArgumentNullException("BotToken");
            //_chatId = telegramConfig.GetValue<string>("ChatId") ?? throw new ArgumentNullException("ChatId");
            _chatIds = telegramConfig.GetSection("ChatIds").Get<List<string>>() ?? new List<string>();
            _httpClient = new HttpClient();
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                // Codifica a mensagem para URL
                var encodedMessage = Uri.EscapeDataString(message);

                foreach (var _chatId in _chatIds)
                {

                    var url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={_chatId}&text={encodedMessage}";

                    var response = await _httpClient.GetAsync(url);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"Resposta do Telegram: {responseBody}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Erro na requisição: {response.StatusCode} - {responseBody}");
                    }

                    response.EnsureSuccessStatusCode();
                }

             
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção ao enviar mensagem: {ex.Message}");
                throw;
            }
        }

    }
}
