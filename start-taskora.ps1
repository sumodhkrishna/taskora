[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$backendProject = Join-Path $repoRoot "backend\Sumodh.Taskora\Sumodh.Taskora.Api.csproj"
$frontendDir = Join-Path $repoRoot "frontend\taskora-web"
$frontendEnvFile = Join-Path $frontendDir ".env"
$frontendNodeModules = Join-Path $frontendDir "node_modules"
$frontendPackageLock = Join-Path $frontendDir "package-lock.json"
$backendUrl = "https://localhost:7002"
$frontendUrl = "http://localhost:5173"

function Write-Step {
    param([string]$Message)

    Write-Host "[Taskora] $Message" -ForegroundColor Cyan
}

function Write-Missing {
    param([string]$Message)

    Write-Host "[Missing] $Message" -ForegroundColor Red
}

function Test-CommandAvailable {
    param([string]$Name)

    return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Get-ShellExecutable {
    if (Test-CommandAvailable "pwsh") {
        return (Get-Command "pwsh").Source
    }

    if (Test-CommandAvailable "powershell") {
        return (Get-Command "powershell").Source
    }

    throw "PowerShell executable was not found."
}

function Test-HttpsDevCertificate {
    $result = Start-Process -FilePath "dotnet" `
        -ArgumentList "dev-certs", "https", "--check", "--quiet" `
        -NoNewWindow `
        -Wait `
        -PassThru

    return $result.ExitCode -eq 0
}

function Wait-ForUrl {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url,
        [Parameter(Mandatory = $true)]
        [string]$DisplayName,
        [int]$TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest -Uri $Url -Method Get -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                Write-Step "$DisplayName is ready at $Url"
                return $true
            }
        }
        catch {
        }

        Start-Sleep -Milliseconds 750
    }

    Write-Host "[Warning] Timed out waiting for $DisplayName at $Url" -ForegroundColor Yellow
    return $false
}

function Test-PortAvailable {
    param(
        [Parameter(Mandatory = $true)]
        [int]$Port
    )

    try {
        $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $Port)
        $listener.Start()
        $listener.Stop()
        return $true
    }
    catch {
        return $false
    }
}

$missingItems = New-Object System.Collections.Generic.List[string]

Write-Step "Checking Windows compatibility and local dependencies..."

if (-not $IsWindows) {
    $missingItems.Add("This startup script is intended for Windows systems.")
}

if (-not (Test-CommandAvailable "dotnet")) {
    $missingItems.Add(".NET SDK was not found. Install the .NET SDK to run the backend.")
}

if (-not (Test-CommandAvailable "node")) {
    $missingItems.Add("Node.js was not found. Install Node.js to run the frontend.")
}

if (-not (Test-CommandAvailable "npm")) {
    $missingItems.Add("npm was not found. Install Node.js with npm to run the frontend.")
}

if (-not (Test-Path $backendProject)) {
    $missingItems.Add("Backend project file is missing at '$backendProject'.")
}

if (-not (Test-Path $frontendDir)) {
    $missingItems.Add("Frontend directory is missing at '$frontendDir'.")
}

if (-not (Test-PortAvailable -Port 7002)) {
    $missingItems.Add("Port 7002 is already in use. Free that port before starting Taskora.")
}

if (-not (Test-PortAvailable -Port 5173)) {
    $missingItems.Add("Port 5173 is already in use. Free that port before starting Taskora.")
}

if (-not (Test-Path $frontendEnvFile)) {
    Write-Step "Creating frontend .env file with default API URL..."
    Set-Content -Path $frontendEnvFile -Value "VITE_API_BASE_URL=$backendUrl" -Encoding UTF8
}
if (-not (Select-String -Path $frontendEnvFile -Pattern '^VITE_API_BASE_URL=' -Quiet)) {
    $missingItems.Add("Frontend .env file does not define VITE_API_BASE_URL.")
}

if ($missingItems.Count -eq 0 -and -not (Test-HttpsDevCertificate)) {
    $missingItems.Add("The local ASP.NET HTTPS development certificate is missing or not trusted. Run 'dotnet dev-certs https --trust'.")
}

if ($missingItems.Count -gt 0) {
    Write-Host ""
    Write-Host "Taskora could not be started because some dependencies or prerequisites are missing:" -ForegroundColor Yellow

    foreach ($item in $missingItems) {
        Write-Missing $item
    }

    Write-Host ""
    Write-Host "Fix the items above and run '.\start-taskora.ps1' again." -ForegroundColor Yellow
    exit 1
}

$shellExe = Get-ShellExecutable

Write-Step "Restoring backend NuGet packages..."
dotnet restore $backendProject

if (-not (Test-Path $frontendNodeModules)) {
    $npmInstallCommand = if (Test-Path $frontendPackageLock) { "ci" } else { "install" }
    Write-Step "Installing frontend npm packages with 'npm $npmInstallCommand'..."
    Push-Location $frontendDir
    try {
        & npm $npmInstallCommand
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Step "Frontend npm packages already installed."
}

$backendCommand = @"
Set-Location '$repoRoot'
`$Host.UI.RawUI.WindowTitle = 'Taskora Backend'
dotnet run --project '$backendProject' --launch-profile https
"@

$frontendCommand = @"
Set-Location '$frontendDir'
`$Host.UI.RawUI.WindowTitle = 'Taskora Frontend'
npm run dev
"@

Write-Step "Starting backend in a new PowerShell window..."
Start-Process -FilePath $shellExe -ArgumentList @("-NoExit", "-Command", $backendCommand) | Out-Null

Write-Step "Waiting for backend to become ready..."
$backendReady = Wait-ForUrl -Url "$backendUrl/health" -DisplayName "Backend API"

Write-Step "Starting frontend in a new PowerShell window..."
Start-Process -FilePath $shellExe -ArgumentList @("-NoExit", "-Command", $frontendCommand) | Out-Null

Write-Step "Waiting for frontend to become ready..."
$frontendReady = Wait-ForUrl -Url $frontendUrl -DisplayName "Frontend"

if ($frontendReady) {
    Write-Step "Opening frontend in the default browser..."
    Start-Process -FilePath $frontendUrl | Out-Null
}

Write-Host ""
Write-Host "Taskora is starting up." -ForegroundColor Green
Write-Host "Backend:  $backendUrl" -ForegroundColor Green
Write-Host "Frontend: $frontendUrl" -ForegroundColor Green
if (-not $backendReady -or -not $frontendReady) {
    Write-Host "One or more services took longer than expected to start. Check the opened PowerShell windows for details." -ForegroundColor DarkYellow
}
Write-Host ""
