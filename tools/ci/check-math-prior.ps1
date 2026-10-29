param(
  [Parameter(Mandatory = $true)][string]$Runtime,
  [Parameter(Mandatory = $true)][string]$RuntimeIdentifier
)
$ErrorActionPreference = 'Stop'
dotnet run --project tools/LegacyOwnedRawInkAppend.Probe -c Release -- tools/LegacyOwnedRawInkAppend.Probe/append-fixtures.json
if ($LASTEXITCODE -ne 0) { throw 'Raw append ownership regression failed.' }
# The friend assembly name is shared with the existing Math publisher probes;
# each project and publish directory remains separate.
foreach ($probeName in @('LegacyMathTemporaryWriter.Probe', 'LegacyMathPriorDomDiff.Probe', 'LegacyMathTemporaryApply.Probe', 'LegacyMathCurrentSelectedIngress.Probe', 'LegacyMathPriorLateRegistration.Probe')) {
  $folder = "tools/$probeName"
  $project = "$folder/$probeName.csproj"
  foreach ($mode in @('managed', 'native')) {
    $capture = ".artifacts/$probeName-$mode.json"
    $probeArgs = if ($probeName -eq 'LegacyMathTemporaryWriter.Probe') {
      @("$folder/fixtures.json", $capture)
    } elseif ($probeName -eq 'LegacyMathPriorDomDiff.Probe') {
      @('.artifacts/math-mode0.ikrmath', 'models/math2/math2-unwrapped.res', $Runtime, "$folder/alignment-fixtures.json", $capture)
    } elseif ($probeName -in @('LegacyMathCurrentSelectedIngress.Probe', 'LegacyMathPriorLateRegistration.Probe')) {
      $fixtures = 'tools/LegacyMathCurrentSelectedIngress.Probe/fixtures'
      @('.artifacts/math-mode0.ikrmath', 'models/math2/math2-unwrapped.res', $Runtime,
        "$fixtures/append-geometries.json", "$fixtures/unchanged-prior-reference.json",
        "$fixtures/frozen-actual-math6.json", $capture)
    } else {
      @('.artifacts/math-mode0.ikrmath', 'models/math2/math2-unwrapped.res', $Runtime, $capture)
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
    } elseif ($probeName -eq 'LegacyMathPriorDomDiff.Probe') {
      python -I -B "$folder/audit_nn.py" $capture models/math2/math2-unwrapped.res
      if ($LASTEXITCODE -ne 0) { throw "Prior NN $mode independent audit failed." }
      python -I -B "$folder/audit_candidate.py" $capture
      if ($LASTEXITCODE -ne 0) { throw "Prior candidate $mode independent audit failed." }
      python -I -B "$folder/audit_diff.py" $capture
      if ($LASTEXITCODE -ne 0) { throw "Prior diff $mode independent audit failed." }
    } elseif ($probeName -in @('LegacyMathCurrentSelectedIngress.Probe', 'LegacyMathPriorLateRegistration.Probe')) {
      $auditors = 'tools/LegacyMathCurrentSelectedIngress.Probe'
      python -I -B "$auditors/audit_nn.py" $capture models/math2/math2-unwrapped.res
      if ($LASTEXITCODE -ne 0) { throw "Current selected $mode NN audit failed." }
      python -I -B "$auditors/audit_candidate.py" $capture
      if ($LASTEXITCODE -ne 0) { throw "Current selected $mode candidate audit failed." }
      python -I -B "$auditors/audit_diff.py" $capture "$capture.diff.json"
      if ($LASTEXITCODE -ne 0) { throw "Current selected $mode diff audit failed." }
      python -I -B "$auditors/audit_unchanged.py" $capture "$auditors/fixtures/unchanged-prior-reference.json"
      if ($LASTEXITCODE -ne 0) { throw "Current selected $mode unchanged route audit failed." }
      # This auditor imports the independently frozen sibling replay module.
      python -B "$auditors/audit_temporary_apply.py" $capture "$capture.apply.json"
      if ($LASTEXITCODE -ne 0) { throw "Current selected $mode temporary apply audit failed." }
      if ($probeName -eq 'LegacyMathPriorLateRegistration.Probe') {
        python -B "$folder/audit_output.py" $capture "$capture.late.json"
        if ($LASTEXITCODE -ne 0) { throw "Late registration $mode independent audit failed." }
      }
    } else {
      python -I -B "$folder/audit_output.py" $capture
      if ($LASTEXITCODE -ne 0) { throw "Temporary application $mode header/owner audit failed." }
      python -I -B tools/LegacyMathPriorDomDiff.Probe/audit_nn.py $capture models/math2/math2-unwrapped.res
      if ($LASTEXITCODE -ne 0) { throw "Temporary application $mode independent NN audit failed." }
      python -I -B tools/LegacyMathPriorDomDiff.Probe/audit_candidate.py $capture
      if ($LASTEXITCODE -ne 0) { throw "Temporary application $mode candidate audit failed." }
    }
  }
}
