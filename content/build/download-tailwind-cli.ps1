#!/usr/bin/env pwsh
# Downloads the standalone Tailwind CSS CLI binary for the current platform.
param(
	[Parameter(Mandatory = $true)][string]$Version,
	[Parameter(Mandatory = $true)][string]$Destination
)

$ErrorActionPreference = "Stop"

$arch = if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) { "arm64" } else { "x64" }
$asset = "tailwindcss-windows-$arch.exe"
$url = "https://github.com/tailwindlabs/tailwindcss/releases/download/v$Version/$asset"

New-Item -ItemType Directory -Force -Path $Destination | Out-Null
$out = Join-Path $Destination "tailwindcss.exe"

Write-Host "Downloading Tailwind CLI v$Version ($asset)..."
Invoke-WebRequest -Uri $url -OutFile $out
Write-Host "Tailwind CLI installed at $out"
