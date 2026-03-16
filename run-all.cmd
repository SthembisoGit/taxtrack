@echo off
setlocal

set "ROOT=%~dp0"

start "TaxTrack API" cmd /k "%ROOT%backend\run-api.cmd"
start "TaxTrack Frontend" cmd /k "%ROOT%frontend\run-frontend.cmd"
