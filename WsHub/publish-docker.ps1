#1.0.3

function Main()
{
  $version = GetNextVersion

  $publishRoot = "./_build"
  $imageName = "ws-hub"
  rmdir -Force -Recurs $publishRoot
  dotnet publish -c Release -o _build\

  docker build --pull -t "maxbl4/$($imageName):$version" -t "maxbl4/$($imageName):latest" -t "maxbl4/$($imageName):release" -f dockerfile $publishRoot
  docker push "maxbl4/$($imageName):$version"
  docker push "maxbl4/$($imageName):latest"
  docker push "maxbl4/$($imageName):release"

  UpdateVersion $version
}

function GetNextVersion()
{
  $lines = Get-Content $MyInvocation.ScriptName
  $version = [System.Version]::Parse($lines[0].Substring(1))
  return "$($version.Major).$($version.Minor).$($version.Build + 1)"
}

function UpdateVersion($version)
{
  $lines = Get-Content $MyInvocation.ScriptName
  $lines[0] = "#$version"
  Set-Content $MyInvocation.ScriptName $lines -Encoding UTF8
}

Main
