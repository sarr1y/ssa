@echo off
chcp 65001 > nul
title Recipes Web API (.NET 10)
cd /d "%~dp0RecipesApi"

echo ===================================================
echo   Запуск ASP.NET Core Web API (.NET 10)
echo   Адрес:   http://localhost:5000
echo   Swagger: http://localhost:5000/swagger
echo   API:     http://localhost:5000/api
echo ===================================================
echo.

dotnet run

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ОШИБКА] Не удалось запустить Web API!
)

pause
