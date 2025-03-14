using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("web-hook")] // 📌 Этот маршрут должен совпадать с URL вебхука!
public class TelegramController : ControllerBase
{
    private readonly TelegramBotClient _botClient;
    private readonly long _chatId; // 👈 Добавляем переменную для chat_id

    public TelegramController(TelegramBotClient botClient)
    {
        _botClient = botClient;

        // Загружаем CHAT_ID из переменной окружения
        string? chatIdEnv = Environment.GetEnvironmentVariable("CHAT_ID");
        if (!long.TryParse(chatIdEnv, out _chatId))
        {
            Console.WriteLine("❌ Ошибка: CHAT_ID не задан или имеет неверный формат!");
            _chatId = 0; // Значение по умолчанию
        }
        else
        {
            Console.WriteLine($"✅ CHAT_ID загружен: {_chatId}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        try
        {
            if (update?.Message != null)
            {
                var incomingChatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;

                Console.WriteLine($"✅ Сообщение от {incomingChatId}: {messageText}");

                // ✅ Проверяем, совпадает ли входящий chat_id с указанным в переменной окружения
                if (_chatId == 0 || incomingChatId != _chatId)
                {
                    Console.WriteLine($"⚠️ Игнорируем сообщение от {incomingChatId}, так как оно не из указанного чата.");
                    return Ok();
                }

                await _botClient.SendTextMessageAsync(_chatId, $"Ты написал: {messageText}");
            }
            else
            {
                Console.WriteLine("⚠️ Пустое обновление получено!");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка при обработке вебхука: {ex.Message}");
            return StatusCode(500, "Ошибка на сервере.");
        }
    }
}
