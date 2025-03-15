using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
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
    Console.WriteLine("❌ Ошибка: Токен бота не найден! Убедись, что переменная окружения BOT_TOKEN задана.");
    return;
}

Console.WriteLine($"✅ BOT_TOKEN загружен: {BotToken.Substring(0, 5)}********");

var botClient = new TelegramBotClient(BotToken);
builder.Services.AddSingleton(botClient);

// 🟢 Временное хранилище сообщений
List<string> messageHistory = new List<string>();

var app = builder.Build();

app.UseRouting();

// ✅ Логирование ошибок
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        Console.WriteLine("❌ Вебхуку вызвана ошибка 500");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Ошибка сервера!");
    });
});

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

        // 🟢 Сохраняем сообщение в историю
        messageHistory.Add($"[Unity] {text}");

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка отправки сообщения: {ex.Message}");
        return Results.Problem("Ошибка на сервере.");
    }
});

// ✅ API для Unity (получение сообщений)
app.MapGet("/get-messages", () =>
{
    Console.WriteLine("📥 Unity запросил последние сообщения");

    string messages = string.Join("\n", messageHistory);
    return Results.Text(messages);
});

// ✅ Подключаем контроллеры
app.MapControllers();

app.Run();
