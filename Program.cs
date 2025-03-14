using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using System;

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

// ✅ Регистрируем Telegram Bot
builder.Services.AddSingleton(botClient);

var app = builder.Build();

app.UseRouting();

// ✅ Включаем логирование ошибок
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        Console.WriteLine("❌ Вебхуку вызвана ошибка 500");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Ошибка сервера!");
    });
});

// ✅ Подключаем контроллеры
app.MapControllers();

app.Run();