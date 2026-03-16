@echo off
setlocal enabledelayedexpansion

if exist "backend\.env" (
  for /f "usebackq tokens=1,* delims==" %%A in ("backend\.env") do (
    set "line=%%A"
    if not "!line!"=="" if not "!line:~0,1!"=="#" (
      set "key=%%A"
      set "val=%%B"
      if "!val:~0,1!"=="\"" set "val=!val:~1!"
      if "!val:~-1!"=="\"" set "val=!val:~0,-1!"
      set "!key!=!val!"
    )
  )
)

dotnet run --project backend/src/TaxTrack.Api
