# 🚀 Инструкция по развертыванию и запуску проекта

Данное руководство содержит пошаговое описание настройки, сборки и запуска серверной и клиентской частей приложения.

---

## 🛠 Предварительные требования

Перед началом работы убедитесь, что у вас установлены и запущены:
* СУБД **PostgreSQL**
* Брокер сообщений **Apache Kafka**
* **.NET SDK** (версия 8)

---

## 📋 Пошаговая настройка конфигурации

### 1. Настройка базы данных Postgres
1. Создайте базу данных в **PostgreSQL** (например, `user_service`).
2. Откройте файл `UserService.Web/appsettings.json`.
3. В секции `ConnectionStrings` укажите параметры подключения:
```json
"ConnectionStrings": {
  "UserService": "Host=localhost;Database=user_service;Username=postgres;Password=YOUR_PASSWORD"
}
```

### 2. Настройка Apache Kafka
1. Откройте файл `UserService.Web/appsettings.json`.
2. В секции `Kafka` задайте адрес брокера и имя топика:
```json
"Kafka": {
  "BootstrapServers": "localhost:9092",
  "Topic": "users.events"
}
```

### 3. Настройка CORS (Backend)
1. Откройте файл `UserService.Web/appsettings.json`.
2. Укажите адрес, с которого фронтенд будет отправлять запросы, для обхода ограничений CORS:
```json
"AdminUiUrl": "http://localhost:8082"
```

### 4. Настройка интеграции со стороны Frontend
1. Откройте файл `UserService.AdminUi/appsettings.json`.
2. Укажите порт для фронтенда — он должен совпадать с адресом из **шага 3** (`http://localhost:8082`).
3. Откройте файл `UserService.AdminUi/wwwroot/appsettings.json`.
4. Задайте базовый адрес бэкенда для подключения к API:
```json
"ApiBaseUrl": "http://localhost:5000"
```

---

## 🚀 Сборка и запуск приложения

### Запуск Backend
Перейдите в директорию бэкенда и запустите проект:
```bash
cd UserService.Web
dotnet build
dotnet run
```

### Запуск Frontend
Перейдите в директорию фронтенда и запустите проект:
```bash
cd ../UserService.AdminUi
dotnet build
dotnet run
```
