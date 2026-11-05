@echo off
chcp 65001 > nul
title Запуск Recipes Project

echo ===================================================
echo   Запуск полного проекта: Web API + Angular
echo ===================================================
echo.

echo [1/2] Запуск бэкенда ASP.NET Core Web API...
start "Recipes Web API" cmd /k ""%~dp0run-api.bat""

timeout /t 4 > nul

echo [2/2] Запуск фронтенда Angular Client...
start "Recipes Angular Client" cmd /k ""%~dp0run-client.bat""

echo.
echo ===================================================
echo   Бэкенд и фронтенд запускаются в двух окнах!
echo   Web API: http://localhost:5000 (Swagger)
echo   Angular: http://localhost:4200
echo ===================================================
timeout /t 5
