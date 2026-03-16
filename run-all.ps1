$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Start-Process powershell -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$root\backend\run-api.ps1`""
Start-Process powershell -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$root\frontend\run-frontend.ps1`""

Write-Host "Started API and frontend in separate terminals."
