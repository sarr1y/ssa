@echo off
chcp 65001 > nul
title Recipes Angular Client
cd /d "%~dp0recipes-client"

echo ===================================================
echo   Запуск Angular Клиента
echo   Адрес: http://localhost:4200
echo ===================================================
echo.

call npm start

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ОШИБКА] Не удалось запустить Angular клиент!
)

pause
