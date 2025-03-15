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
using System.Net.WebSockets;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// ✅ Поддержка JSON
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

List<WebSocket> connectedClients = new List<WebSocket>();

var app = builder.Build();

app.UseRouting();

// ✅ API для WebSocket (Unity подключается)
app.UseWebSockets();

app.Map("/ws-chat", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        connectedClients.Add(webSocket);
        Console.WriteLine("✅ Unity подключился к WebSocket");

        try
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    connectedClients.Remove(webSocket);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрыто клиентом", CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка WebSocket: {ex.Message}");
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// ✅ Webhook от Telegram (получение сообщений)
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
            string senderName = update.Message.From.Username ?? update.Message.From.FirstName ?? "Unknown";
            string text = update.Message.Text;
            string formattedMessage = $"[Telegram] {senderName}: {text}";

            Console.WriteLine($"✅ {formattedMessage}");

            // Рассылаем всем Unity-клиентам через WebSocket
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(formattedMessage);
            List<WebSocket> closedSockets = new List<WebSocket>();
            foreach (var client in connectedClients)
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    closedSockets.Add(client);
                }
            }
            // Удаляем отключенных клиентов
            connectedClients.RemoveAll(c => closedSockets.Contains(c));
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка в Webhook: {ex.Message}");
        return Results.Problem("Ошибка на сервере.");
    }
});

// ✅ Отправка сообщений из Unity в Telegram
app.MapPost("/send-message", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<dynamic>(requestBody);

        long chatId = data.chat_id;
        string text = data.text;
        string formattedMessage = $"[South City] {text}";

        Console.WriteLine($"✅ Unity отправил: {text}");

        await botClient.SendTextMessageAsync(chatId, text);

        // Рассылаем всем клиентам через WebSocket
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(formattedMessage);
        foreach (var client in connectedClients)
        {
            if (client.State == WebSocketState.Open)
            {
                await client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка отправки сообщения: {ex.Message}");
        return Results.Problem("Ошибка на сервере.");
    }
});

app.Run();
