@echo off
setlocal enabledelayedexpansion

set "ENV_FILE=backend\.env"
if exist "%ENV_FILE%" (
  for /f "usebackq tokens=1,* delims==" %%A in ("%ENV_FILE%") do (
    set "key=%%A"
    set "val=%%B"
    if not "!key!"=="" if not "!key:~0,1!"=="#" (
      if "!val:~0,1!"=="\"" set "val=!val:~1!"
      if "!val:~-1!"=="\"" set "val=!val:~0,-1!"
      set "!key!=!val!"
    )
  )
)

if "%ASPNETCORE_URLS%"=="" set "ASPNETCORE_URLS=http://localhost:8080"

dotnet run --project backend/src/TaxTrack.Api
