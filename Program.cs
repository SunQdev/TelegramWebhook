using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// ✅ Используем Newtonsoft.Json для корректного парсинга Telegram API
builder.Services.AddControllers().AddNewtonsoftJson();

string? BotToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

if (string.IsNullOrEmpty(BotToken))
{
    Console.WriteLine("❌ Ошибка: Токен бота не найден! Убедись, что переменная окружения BOT_TOKEN задана.");
    return;
}

Console.WriteLine($"✅ BOT_TOKEN загружен: {BotToken.Substring(0, 5)}********");

var botClient = new TelegramBotClient(BotToken);

// ✅ Регистрируем Telegram Bot
builder.Services.AddSingleton(botClient);

var app = builder.Build();

app.UseRouting();

// ✅ Webhook - обработка сообщений с логированием
app.MapPost("/web-hook", async (HttpContext context) =>
{
    try
    {
        string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        Console.WriteLine($"📥 Получен Webhook JSON: {requestBody}");

        var update = JsonSerializer.Deserialize<Update>(requestBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (update == null)
        {
            Console.WriteLine("❌ Ошибка: JSON не распознан!");
            context.Response.StatusCode = 400; // Bad Request
            return;
        }

        if (update.Message != null)
        {
            Console.WriteLine($"📩 Новое сообщение от {update.Message.Chat.Id}: {update.Message.Text}");
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Ваше сообщение получено!");
        }

        context.Response.StatusCode = 200;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"🔥 Ошибка обработки Webhook: {ex.Message}");
        context.Response.StatusCode = 500; // Internal Server Error
    }
});

// ✅ Подключаем контроллеры (обязательно)
app.MapControllers();

app.Run();
