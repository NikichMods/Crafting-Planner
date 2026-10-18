# SPDX-License-Identifier: MPL-2.0
param(
    [Parameter(Mandatory = $true)][string]$GameRoot,
    [Parameter(Mandatory = $true)][string]$CompilerPath,
    [Parameter(Mandatory = $true)][string]$OutputFile
)
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath $OutputFile) { throw 'Refusing to overwrite an existing research artifact.' }
$managedPath = Join-Path $GameRoot 'Graveyard Keeper_Data/Managed'
$referenceFiles = @('mscorlib.dll','System.dll','System.Core.dll','Assembly-CSharp.dll','Assembly-CSharp-firstpass.dll','UnityEngine.dll','UnityEngine.CoreModule.dll') |
    ForEach-Object { Join-Path $managedPath $_ }
$referenceFiles += @('BepInEx.dll','0Harmony.dll') | ForEach-Object { Join-Path $GameRoot ('BepInEx/core/' + $_) }
foreach ($file in $referenceFiles) { if (!(Test-Path -LiteralPath $file)) { throw "Missing reference: $file" } }
$sourceFile = Join-Path $PSScriptRoot 'Probe.cs'
$compilerArguments = @('/nologo','/target:library','/nostdlib+','/noconfig','/langversion:7.3','/optimize+','/deterministic+','/warnaserror+',('/out:' + $OutputFile))
$compilerArguments += $referenceFiles | ForEach-Object { '/reference:' + $_ }
$compilerArguments += $sourceFile
& dotnet $CompilerPath @compilerArguments
if ($LASTEXITCODE -ne 0) { throw 'Research native-reference compilation failed.' }
if ([Reflection.AssemblyName]::GetAssemblyName((Resolve-Path -LiteralPath $OutputFile).Path).Version.ToString() -ne '0.1.0.0') { throw 'Research version mismatch.' }
Get-FileHash -LiteralPath $OutputFile -Algorithm SHA256

