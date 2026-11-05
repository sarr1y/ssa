@echo off
chcp 65001 > nul
title Recipes Web API
cd /d "%~dp0RecipesApi"

echo ===================================================
echo   Запуск ASP.NET Core Web API
echo   Адрес:   http://localhost:5000
echo   Swagger: http://localhost:5000/swagger
echo ===================================================
echo.

dotnet run --urls "http://localhost:5000;http://127.0.0.1:5000"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ОШИБКА] Не удалось запустить Web API!
)

pause
