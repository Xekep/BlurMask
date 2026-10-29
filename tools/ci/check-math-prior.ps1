param(
  [Parameter(Mandatory = $true)][string]$Runtime,
  [Parameter(Mandatory = $true)][string]$RuntimeIdentifier
)
$ErrorActionPreference = 'Stop'
# The friend assembly name is shared with the existing Math publisher probes;
# each project and publish directory remains separate.
foreach ($probeName in @('LegacyMathTemporaryWriter.Probe', 'LegacyMathPriorDomDiff.Probe')) {
  $folder = "tools/$probeName"
  $project = "$folder/$probeName.csproj"
  foreach ($mode in @('managed', 'native')) {
    $capture = ".artifacts/$probeName-$mode.json"
    $probeArgs = if ($probeName -eq 'LegacyMathTemporaryWriter.Probe') {
      @("$folder/fixtures.json", $capture)
    } else {
      @('.artifacts/math-mode0.ikrmath', 'models/math2/math2-unwrapped.res', $Runtime, "$folder/alignment-fixtures.json", $capture)
    }
    if ($mode -eq 'managed') {
      dotnet run --project $project -c Release -- @probeArgs
      if ($LASTEXITCODE -ne 0) { throw "$probeName managed run failed." }
    } else {
      $publishDirectory = ".artifacts/$probeName-native"
      dotnet publish $project -c Release -r $RuntimeIdentifier -p:PublishAot=true -o $publishDirectory
      if ($LASTEXITCODE -ne 0) { throw "$probeName Native AOT publish failed." }
      $suffix = if ($IsWindows) { '.exe' } else { '' }
      & "$publishDirectory/LegacyMathFreshPublisher.Probe$suffix" @probeArgs
      if ($LASTEXITCODE -ne 0) { throw "$probeName native run failed." }
    }
    if ($probeName -eq 'LegacyMathTemporaryWriter.Probe') {
      python -I -B "$folder/audit.py" $capture
      if ($LASTEXITCODE -ne 0) { throw "$probeName $mode independent audit failed." }
    } else {
      python -I -B "$folder/audit_nn.py" $capture models/math2/math2-unwrapped.res
      if ($LASTEXITCODE -ne 0) { throw "Prior NN $mode independent audit failed." }
      python -I -B "$folder/audit_candidate.py" $capture
      if ($LASTEXITCODE -ne 0) { throw "Prior candidate $mode independent audit failed." }
      python -I -B "$folder/audit_diff.py" $capture
      if ($LASTEXITCODE -ne 0) { throw "Prior diff $mode independent audit failed." }
    }
  }
}
