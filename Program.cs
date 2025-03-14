using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Microsoft.AspNetCore.Mvc;
using System;

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

// ✅ 📌 Только один маршрут (он уже задан в TelegramController.cs)
app.MapControllers(); 

app.Run();
