# Loads backend/.env into the current PowerShell session (best effort).
if (Test-Path "backend/.env") {
  Get-Content "backend/.env" | ForEach-Object {
    $line = $_.Trim()
    if (-not $line -or $line.StartsWith('#')) { return }
    $parts = $line.Split('=',2)
    if ($parts.Count -eq 2) {
      $name = $parts[0].Trim()
      $value = $parts[1].Trim().Trim('"')
      [System.Environment]::SetEnvironmentVariable($name, $value, "Process")
    }
  }
}

dotnet run --project backend/src/TaxTrack.Api
