# Базовый образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проекта и восстанавливаем зависимости
COPY ["ExamYandexApp.csproj", "."]
RUN dotnet restore "ExamYandexApp.csproj"

# Копируем все файлы и собираем приложение
COPY . .
RUN dotnet publish "ExamYandexApp.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Устанавливаем инструменты для диагностики и загружаем сертификат
RUN apt-get update && apt-get install -y curl && \
    mkdir -p /root/.postgresql && \
    curl -o /root/.postgresql/root.crt https://storage.yandexcloud.net/cloud-certs/CA.pem

# Копируем собранное приложение
COPY --from=build /app/publish .

# Создаем пользователя для безопасности
RUN groupadd -r appuser && useradd -r -g appuser appuser && \
    mkdir -p /home/appuser/.postgresql && \
    cp /root/.postgresql/root.crt /home/appuser/.postgresql/ && \
    chown -R appuser:appuser /app /home/appuser

USER appuser

# Открываем порт
EXPOSE 8080

# Запускаем приложение
ENTRYPOINT ["dotnet", "ExamYandexApp.dll"]