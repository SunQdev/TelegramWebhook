using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.AspNetCore.Mvc;
using System;
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
    if (update?.Message != null)
    {
        Console.WriteLine($"Новое сообщение от {update.Message.Chat.Id}: {update.Message.Text}");
        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Ваше сообщение получено!");
    }

    return Results.Ok();
});

// ✅ ОБЯЗАТЕЛЬНО: подключение контроллеров
app.MapControllers();

app.Run();
