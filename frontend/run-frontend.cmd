@echo off
setlocal enabledelayedexpansion

if exist "frontend\.env" (
  for /f "usebackq tokens=1,* delims==" %%A in ("frontend\.env") do (
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

cd frontend
call npm install
call npm run dev
