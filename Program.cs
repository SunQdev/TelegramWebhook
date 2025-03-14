using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Microsoft.AspNetCore.Mvc;
using System;

var builder = WebApplication.CreateBuilder(args);

// Используем Newtonsoft.Json (если установлен)
builder.Services.AddControllers()
    .AddNewtonsoftJson();

string BotToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

// Подключаем Telegram Bot
builder.Services.AddSingleton<TelegramBotClient>(new TelegramBotClient(BotToken));

var app = builder.Build();

app.UseRouting();

app.MapControllers(); // Новая схема маршрутов для .NET 7/8

app.Run();

