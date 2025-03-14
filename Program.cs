using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// ✅ Используем Newtonsoft.Json для корректного парсинга
builder.Services.AddControllers().AddNewtonsoftJson();

string? BotToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

if (string.IsNullOrEmpty(BotToken))
{
    Console.WriteLine("❌ Ошибка: Токен бота не найден!");
    return;
}

var botClient = new TelegramBotClient(BotToken);

// ✅ Регистрируем Telegram Bot
builder.Services.AddSingleton(botClient);

var app = builder.Build();

app.UseRouting();

// ✅ Webhook - обработка сообщений
app.MapPost("/web-hook", async (HttpContext context) =>
{
    try
    {
        // 📌 Логируем весь JSON перед обработкой
        string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        Console.WriteLine($"📩 Получен Webhook JSON: {requestBody}");

        // ✅ Парсим JSON вручную
        var update = JsonSerializer.Deserialize<Update>(requestBody);

        if (update?.Message != null)
        {
            Console.WriteLine($"💬 Новое сообщение от {update.Message.Chat.Id}: {update.Message.Text}");
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "✅ Ваше сообщение получено!");
        }

        await context.Response.WriteAsync("OK");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка обработки Webhook: {ex.Message}");
        await context.Response.WriteAsync("Error");
    }
});

// ✅ Подключаем контроллеры
app.MapControllers();

app.Run();
