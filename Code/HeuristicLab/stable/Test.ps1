﻿# find ms build
$programFilesX86Dir = ($Env:ProgramFiles, ${Env:ProgramFiles(x86)})[[bool]${Env:ProgramFiles(x86)}]
$vsDir = [System.IO.Path]::Combine($programFilesX86Dir, "Microsoft Visual Studio")
$years = @("2019", "2017")
$editions = @("Enterprise", "Professional", "Community", "BuildTools")
$versions = @("Current", "15.0")

$msBuildPath = $undefined
$vstestPath = $undefined
:search Foreach ($year in $years) {
  $loc = [System.IO.Path]::Combine($vsDir, $year)
  Foreach ($edition in $editions) {
    $edLoc = [System.IO.Path]::Combine($loc, $edition, "MSBuild")
    Foreach ($version in $versions) {
      $binLoc = [System.IO.Path]::Combine($edLoc, $version, "Bin")
      $loc64 = [System.IO.Path]::Combine($binLoc, "amd64", "MSBuild.exe")
      $loc32 = [System.IO.Path]::Combine($binLoc, "MSBuild.exe")

      If ([System.IO.File]::Exists($loc64)) {
        $msBuildPath = $loc64
        $vstestPath = [System.IO.Path]::Combine($loc, $edition, "Common7", "IDE", "CommonExtensions", "Microsoft", "TestWindow", "vstest.console.exe")
        Break search;
      }
      If ([System.IO.File]::Exists($loc32)) {
        $msBuildPath = $loc32
        $vstestPath = [System.IO.Path]::Combine($loc, $edition, "Common7", "IDE", "CommonExtensions", "Microsoft", "TestWindow", "vstest.console.exe")
        Break search;
      }
    }
  }
}

If ($vstestPath -eq $undefined) {
  "Could not locate vstest.console.exe, ABORTING ..."
  Return
}

If(-not [System.IO.File]::Exists($vstestPath)) {
  "vstest.console.exe is not found at $vstestPath, ABORTING ..."
  Return
}

$testcontainer = "HeuristicLab.Tests.dll"
$input = Read-Host "Which container to test? [$($testcontainer)]"
$testcontainer = ($testcontainer, $input)[[bool]$input]

$testplatform = "x64"
$input = Read-Host "Which platform to run the tests? [$($testplatform)]"
$testplatform = ($testplatform, $input)[[bool]$input]

$testcategory = "Essential"
$input = Read-Host "Which category do you want to run? [$($testcategory)]"
$testcategory = ($testcategory, $input)[[bool]$input]

# query whether to build
$input = Read-Host "Should the tests be rebuilt y/N?"
$input = ([string]("n", $input)[[bool]$input]).ToLowerInvariant()
$dobuild = $input -eq "y"

If($dobuild) {
  $curPath = $MyInvocation.MyCommand.Path
  $curDir = Split-Path $curPath

  $slnFiles = Get-ChildItem $curDir -Filter *Tests.sln

  If ($slnFiles.Count -le 0) {
    "No solutions found, ABORTING ..."
    Return
  }

  $slnIndices = @()

  If ($slnFiles.Count -eq 1) {
    "Selecting the only solution found: `"{0}`"" -f $slnFiles[0].Name
    $slnIndices += 0
  } Else {
    "Found the following solutions:"

    ""

    $slnFiles | % { $i = 0 } { ("  {0}. `"{1}`"" -f ($i + 1), $_.Name); $i++ }

    ""

    $success = $false

    # query solution to build
    $slnIndex = -1
    Do {
        $input = Read-Host "Which solution(s) to build? (e.g.: 1 2 3) { 1..$($slnFiles.Count) }"
        $inputParts = $input -Split " "

        Foreach ($part in $inputParts) {
          If ($part -eq "") { Continue }

          $success = [int]::TryParse($part, [ref]$slnIndex) -and ($slnIndex -gt 0) -and ($slnIndex -le $slnFiles.Count)

          If ($success) {
            $slnIndices += $slnIndex - 1
          } Else {
            $slnIndices = @()
            Break
          }
        }
    } While (-Not $success)

    $slnIndices = $slnIndices | Select-Object -Unique
  }


  # query configuration to build
  $config = "Release"
  $input = Read-Host "Which configuration to build? [$($config)]"
  $config = ($config, $input)[[bool]$input]

  # query platform to build
  $platform = "Any CPU"
  $input = Read-Host "Which platform to build? [$($platform)]"
  $platform = ($platform, $input)[[bool]$input]

  # query clean desire
  $clean = $false
  Do {
      $input = Read-Host "Would you like to clean before building? [y/N]"
      $input = ([string]("n", $input)[[bool]$input]).ToLowerInvariant()
      $success = $input -eq "n" -or ($clean = $input -eq "y")
  } While (-Not $success)

  ""

  If ($clean) {
    Foreach ($slnIndex in $slnIndices) {
      $solution = $slnFiles[$slnIndex]
      "Cleaning `"$($solution.Name)`" ..."
      $args = @(
        $solution.FullName,
        "/t:Clean",
        "/p:Configuration=`"$config`",Platform=`"$platform`"",
        "/m", "/nologo", "/verbosity:q", "/clp:ErrorsOnly"
      )
      & $msBuildPath $args
      "===== CLEAN FINISHED ====="
    }
  }

  Foreach ($slnIndex in $slnIndices) {
    $solution = $slnFiles[$slnIndex]
    "Building `"$($solution.Name)`" ($config|$platform) ..."
    $args = @(
      $solution.FullName,
      "/t:Restore,Build",
      "/p:Configuration=`"$config`",Platform=`"$platform`"",
      "/m", "/nologo", "/verbosity:q", "/clp:ErrorsOnly"
    )
    & $msBuildPath $args
    "===== BUILD FINISHED ====="
  }
}

& $vstestPath "bin\$testcontainer" /Framework:framework40 /Platform:$testplatform /TestCaseFilter:"TestCategory=$testcategory"