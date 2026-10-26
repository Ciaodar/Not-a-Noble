param(
    [string]$AssemblyInfoPath,
    [string]$Configuration
)

if (-not (Test-Path $AssemblyInfoPath)) {
    Write-Host "AssemblyInfo.cs not found at $AssemblyInfoPath"
    exit 1
}

$content = Get-Content $AssemblyInfoPath

# Find the AssemblyVersion line
$versionLine = $content | Where-Object { $_ -match '^\[assembly: AssemblyVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)\]' }

if ($versionLine) {
    $matches = [regex]::Match($versionLine, '^\[assembly: AssemblyVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)\]')
    $major = [int]$matches.Groups[1].Value
    $update = [int]$matches.Groups[2].Value
    $hotfix = [int]$matches.Groups[3].Value
    $debug = [int]$matches.Groups[4].Value

    if ($Configuration -eq "BigUpdate") {
        $major++
        $update = 0
        $hotfix = 0
        $debug = 0
    }
    elseif ($Configuration -eq "Update") {
        $update++
        $hotfix = 0
        $debug = 0
    }
    elseif ($Configuration -eq "Hotfix") {
        $hotfix++
        $debug = 0
    }
    else {
        # Debug configuration
        $debug++
    }

    $newVersion = "$major.$update.$hotfix.$debug"
    
    # Replace in content
    $content = $content -replace '\[assembly: AssemblyVersion\(".*"\)\]', "[assembly: AssemblyVersion(`"$newVersion`")]"
    $content = $content -replace '\[assembly: AssemblyFileVersion\(".*"\)\]', "[assembly: AssemblyFileVersion(`"$newVersion`")]"
    
    Set-Content -Path $AssemblyInfoPath -Value $content
    Write-Host "Updated Version to $newVersion"

    # Update SubModule.xml only if this is the main project
    if ($AssemblyInfoPath -match "NaN\\Properties") {
        $subModulePath = Join-Path (Split-Path (Split-Path $AssemblyInfoPath) -Parent) "..\SubModule.xml"
        if (Test-Path $subModulePath) {
            $subContent = Get-Content $subModulePath
            $subContent = $subContent -replace '<Version value=".*" />', "<Version value=`"v$newVersion`" />"
            Set-Content -Path $subModulePath -Value $subContent
        }
    }
} else {
    Write-Host "Could not find AssemblyVersion in $AssemblyInfoPath"
    exit 1
}
