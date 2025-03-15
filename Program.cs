using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// ✅ Добавляем поддержку JSON
builder.Services.AddControllers().AddNewtonsoftJson();

string? BotToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
if (string.IsNullOrEmpty(BotToken))
{
    Console.WriteLine("❌ Ошибка: Токен бота не найден!");
    return;
}

Console.WriteLine($"✅ BOT_TOKEN загружен: {BotToken.Substring(0, 5)}********");

var botClient = new TelegramBotClient(BotToken);
builder.Services.AddSingleton(botClient);

// 🟢 **Храним все сообщения (Telegram + Unity)**
List<string> messageHistory = new List<string>();
long lastMessageId = 0;

var app = builder.Build();

app.UseRouting();

// ✅ API для Unity (отправка сообщений)
app.MapPost("/send-message", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<dynamic>(requestBody);

        long chatId = data.chat_id;
        string text = data.text;

        Console.WriteLine($"✅ Unity отправил сообщение: {text}");

        await botClient.SendTextMessageAsync(chatId, text);

        // 🟢 Добавляем сообщение в историю
        messageHistory.Add($"[South City] {text}");

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка отправки сообщения: {ex.Message}");
        return Results.Problem("Ошибка на сервере.");
    }
});

// ✅ **API для Unity (получение новых сообщений)**
app.MapGet("/get-latest-messages", () =>
{
    Console.WriteLine("📥 Unity запросил новые сообщения");

    string messages = string.Join("\n", messageHistory);
    return Results.Text(messages);
});

// ✅ **Обрабатываем входящие сообщения от Telegram (Webhook)**
app.MapPost("/web-hook", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        var update = JsonConvert.DeserializeObject<Update>(requestBody);

        if (update?.Message != null)
        {
            long chatId = update.Message.Chat.Id;
            string text = update.Message.Text;
            long messageId = update.Message.MessageId;

            string senderName = "Unknown"; // Значение по умолчанию
            if (update.Message.From != null)
            {
                senderName = !string.IsNullOrEmpty(update.Message.From.Username)
                    ? $"@{update.Message.From.Username}" // Если есть username, используем его
                    : update.Message.From.FirstName ?? "Unknown"; // Если нет - берем FirstName
            }

            if (messageId > lastMessageId) // Фильтруем только новые сообщения
            {
                lastMessageId = messageId;
                Console.WriteLine($"✅ Telegram: {senderName}: {text}");
                messageHistory.Add($"[Telegram] {senderName}: {text}"); // Добавляем ник отправителя
            }
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка в обработке Webhook: {ex.Message}");
        return Results.Problem("Ошибка на сервере.");
    }
});

app.Run();
