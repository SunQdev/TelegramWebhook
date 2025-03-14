using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

[ApiController]
[Route("webhook")]
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
        if (update.Message != null)
        {
            var chatId = update.Message.Chat.Id;
            var text = update.Message.Text;

            await _botClient.SendTextMessageAsync(chatId, $"Ты сказал: {text}");
        }

        return Ok();
    }
}
