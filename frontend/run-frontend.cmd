@echo off
setlocal enabledelayedexpansion

set "ENV_FILE=frontend\.env"
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

cd frontend
if not exist "node_modules" (
  call npm install
)
call npm run dev
