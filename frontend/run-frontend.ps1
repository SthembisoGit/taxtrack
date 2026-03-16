$ErrorActionPreference = "Stop"

function Load-EnvFile {
  param([string]$Path)
  if (-not (Test-Path $Path)) { return }

  Get-Content $Path | ForEach-Object {
    $line = $_.Trim()
    if (-not $line -or $line.StartsWith('#')) { return }

    $match = [regex]::Match($line, '^\s*([^=]+?)\s*=\s*(.*)\s*$')
    if (-not $match.Success) { return }

    $name = $match.Groups[1].Value.Trim()
    $value = $match.Groups[2].Value.Trim()

    if ($value.StartsWith('"') -and $value.EndsWith('"')) {
      $value = $value.Substring(1, $value.Length - 2)
    }

    [System.Environment]::SetEnvironmentVariable($name, $value, "Process")
  }
}

Load-EnvFile "frontend/.env"

Push-Location frontend
if (-not (Test-Path "node_modules")) {
  npm install
}
npm run dev
Pop-Location
