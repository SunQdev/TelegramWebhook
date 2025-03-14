using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// ✅ Используем Newtonsoft.Json для корректной работы с Telegram API
builder.Services.AddControllers().AddNewtonsoftJson();

// ✅ Загружаем токен из переменной окружения
string? BotToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

if (string.IsNullOrEmpty(BotToken))
{
    Console.WriteLine("❌ Ошибка: Токен бота не найден! Убедись, что переменная окружения BOT_TOKEN задана.");
    return;
}

var botClient = new TelegramBotClient(BotToken);

// ✅ Регистрируем Telegram Bot
builder.Services.AddSingleton(botClient);

var app = builder.Build();

app.UseRouting();

// ✅ Webhook - обработка сообщений
app.MapPost("/web-hook", async ([FromBody] Update update) =>
{
    try
    {
        if (update.Message != null)
        {
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            Console.WriteLine($"🔹 Новое сообщение от {chatId}: {messageText}");

            await botClient.SendTextMessageAsync(chatId, "✅ Ваше сообщение получено!");
        }
        else
        {
            Console.WriteLine("⚠️ Получено обновление без сообщения.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка обработки сообщения: {ex.Message}");
    }
});

// ✅ ОБЯЗАТЕЛЬНО: подключение контроллеров
app.MapControllers(); 

app.Run();
