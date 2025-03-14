# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Копируем файлы проекта и устанавливаем зависимости
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Используем .NET runtime для запуска
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .

# Указываем команду для запуска сервера
CMD ["dotnet", "TelegramWebhook.dll"]
