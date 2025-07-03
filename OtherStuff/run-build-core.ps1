#
# PSake build script for ThCADPlugin Apps
#
Task Release.Build {
    $script:buildType = "Release"
}

Task Debug.Build {
    $script:buildType = "Debug"
}
function Get-LatestMsbuildLocation
{
  Param 
  (
    [bool] $allowPreviewVersions = $false
  )
    if ($allowPreviewVersions) {
      $latestVsInstallationInfo = Get-VSSetupInstance -All -Prerelease | Sort-Object -Property InstallationVersion -Descending | Select-Object -First 1
    } else {
      $latestVsInstallationInfo = Get-VSSetupInstance -All | Sort-Object -Property InstallationVersion -Descending | Select-Object -First 1
    }
    Write-Host "Latest version installed is $($latestVsInstallationInfo.InstallationVersion)"
    if ($latestVsInstallationInfo.InstallationVersion -like "15.*") {
      $msbuildLocation = "$($latestVsInstallationInfo.InstallationPath)\MSBuild\15.0\Bin\msbuild.exe"
    
      Write-Host "Located msbuild for Visual Studio 2017 in $msbuildLocation"
    } else {
      $msbuildLocation = "$($latestVsInstallationInfo.InstallationPath)\MSBuild\Current\Bin\msbuild.exe"
      Write-Host "Located msbuild in $msbuildLocation"
    }

    return $msbuildLocation
}

Task Requires.Confuser {
    $Script:confuserEx = "..\Tools\ConfuserEx\Confuser.CLI.exe"
}

Task Requires.MSBuild {
    $script:msbuildExe = Get-LatestMsbuildLocation

    if ($msbuildExe -eq $null)
    {
            throw "Failed to find MSBuild"
    }

    Write-Host "Found MSBuild here: $msbuildExe"
}

# $buildType build for AutoCAD R18
Task Compile.Core.R18 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\ThMEPCore.sln" /p:Configuration=$buildType /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\$buildType\",IntermediateOutputPath="..\build\obj\$buildType\" ".\ThMEPCore.sln" /p:Configuration=$buildType /t:rebuild
    }
}

Task Confuser.Core.R18 -Depends Requires.Confuser, Compile.Core.R18  {
    exec {
        & $Script:confuserEx -n "ThMEPCore.Release.crproj"
    }
}

# $buildType build for AutoCAD R19
Task Compile.Core.R19 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThMEPCore.sln" /p:Configuration="${buildType}-NET40" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET40\",IntermediateOutputPath="..\build\obj\${buildType}-NET40\" ".\ThMEPCore.sln" /p:Configuration="${buildType}-NET40" /t:rebuild
    }
}

Task Confuser.Core.R19 -Depends Requires.Confuser, Compile.Core.R19 {
    exec {
        & $Script:confuserEx -n "ThMEPCore.Release-NET40.crproj"
    }
}

# $buildType build for AutoCAD R20
Task Compile.Core.R20 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThMEPCore.sln" /p:Configuration="${buildType}-NET45" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET45\",IntermediateOutputPath="..\build\obj\${buildType}-NET45\" ".\ThMEPCore.sln" /p:Configuration="${buildType}-NET45" /t:rebuild
    }
}

Task Confuser.Core.R20 -Depends Requires.Confuser, Compile.Core.R20 {
    exec {
        & $Script:confuserEx -n "ThMEPCore.Release-NET45.crproj"
    }
}

# $buildType build for AutoCAD R22
Task Compile.Core.R22 -Depends Requires.MSBuild {
    exec {
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\ThMEPCore.sln" /p:Configuration="${buildType}-NET46" /t:restore
        & $msbuildExe /verbosity:minimal /property:OutDir="..\build\bin\${buildType}-NET46\",IntermediateOutputPath="..\build\obj\${buildType}-NET46\" ".\ThMEPCore.sln" /p:Configuration="${buildType}-NET46" /t:rebuild
    }
}

Task Confuser.Core.R22 -Depends Requires.Confuser, Compile.Core.R22 {
    exec {
        & $Script:confuserEx -n "ThMEPCore.Release-NET46.crproj"
    }
}

Task Requires.BuildType {
    if ($buildType -eq $null) {
        throw "No build type specified"
    }

    Write-Host "$buildType build confirmed"
}

Task Compile.Core -Depends Requires.BuildType, Compile.Core.R18, Compile.Core.R19, Compile.Core.R20, Compile.Core.R22
{

}

Task Confuser.Core -Depends Confuser.Core.R18, Confuser.Core.R19, Confuser.Core.R20, Confuser.Core.R22
{

}