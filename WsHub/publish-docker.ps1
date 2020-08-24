#1.0.13

function Main()
{
  $version = GetNextVersion

  $publishRoot = "./_build"
  $imageName = "ws-hub"
  rmdir -Force -Recurs $publishRoot
  dotnet publish -c Release -o _build\

  docker buildx build --platform "linux/arm64" --pull -t "maxbl4/$($imageName):arm64-$version" -t "maxbl4/$($imageName):arm64" -f dockerfile $publishRoot
  docker buildx build --platform "linux/amd64" --pull -t "maxbl4/$($imageName):amd64-$version" -t "maxbl4/$($imageName):amd64" -f dockerfile $publishRoot
  docker push "maxbl4/$($imageName):arm64-$version"
  docker push "maxbl4/$($imageName):arm64"
  docker push "maxbl4/$($imageName):amd64-$version"
  docker push "maxbl4/$($imageName):amd64"

  docker manifest create --amend "maxbl4/$($imageName):latest" "maxbl4/$($imageName):arm64" "maxbl4/$($imageName):amd64"
  docker manifest push -p "maxbl4/$($imageName):latest"
  docker manifest create --amend "maxbl4/$($imageName):release" "maxbl4/$($imageName):arm64" "maxbl4/$($imageName):amd64"
  docker manifest push -p "maxbl4/$($imageName):release"
  
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
