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

// Используем Newtonsoft.Json для десериализации Telegram API JSON
builder.Services.AddControllers().AddNewtonsoftJson();

string BotToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
var botClient = new TelegramBotClient(BotToken);

// Регистрируем Telegram Bot
builder.Services.AddSingleton(botClient);

var app = builder.Build();

app.UseRouting();

// ✅ Регистрируем Webhook
app.MapPost("/webhook", async ([FromBody] Update update) =>
{
    if (update.Message != null)
    {
        Console.WriteLine($"Новое сообщение от {update.Message.Chat.Id}: {update.Message.Text}");
        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Ваше сообщение получено!");
    }
});

app.MapControllers(); // ОБЯЗАТЕЛЬНО для работы контроллеров!
app.Run();