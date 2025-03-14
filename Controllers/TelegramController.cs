using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

[ApiController]
[Route("web-hook")] // 📌 Маршрут контроллера
public class TelegramController : ControllerBase
{
    private readonly TelegramBotClient _botClient;

    public TelegramController(TelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        // 🔥 Логируем полный JSON-запрос от Telegram
        Console.WriteLine($"🔥 Получено обновление: {System.Text.Json.JsonSerializer.Serialize(update)}");

        if (update?.Message != null)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            Console.WriteLine($"✅ Сообщение от {chatId}: {messageText}");

            await _botClient.SendTextMessageAsync(chatId, $"Ты написал: {messageText}");
        }

        return Ok();
    }
}
