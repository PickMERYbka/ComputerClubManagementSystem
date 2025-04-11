@echo off
echo Инициализация базы данных...
set PGPASSWORD=123
"C:\Program Files\PostgreSQL\17\bin\psql.exe" -U postgres -d postgres -f Database/init.sql

echo.
echo Компиляция приложения...
dotnet build

echo.
echo Запуск приложения...
dotnet run

pause 