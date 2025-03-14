using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

[ApiController]
[Route("web-hook")] // 📌 Этот маршрут должен совпадать с URL вебхука!
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
        try
        {
            if (update?.Message != null)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;

                Console.WriteLine($"✅ Сообщение от {chatId}: {messageText}");

                await _botClient.SendTextMessageAsync(chatId, $"Ты написал: {messageText}");
            }
            else
            {
                Console.WriteLine("⚠️ Пустое обновление получено!");
            }

            return Ok();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"❌ Ошибка при обработке вебхука: {ex.Message}");
            return StatusCode(500, "Ошибка на сервере.");
        }
    }
}
