param([Parameter(Mandatory = $true)][string]$Executable)
$ErrorActionPreference = 'Stop'
$fixtures = @(
    @('latex(-123456789)', '-123456789'),
    @('latex(-1/2)', '-\frac{1}{2}'),
    @('latex(solve(x^2-1=0,x))', '\left[-1,1\right]'),
    @('latex(solve(x=x,x))', '\left[x\right]'),
    @('latex(solve([x+y=3,x-y=1],[x,y]))', '\left[\left[2,1\right]\right]'),
    @('latex([[1,2],[3,4]]^(-1))', '\left[\begin{array}{cc}-2&1\\\frac{3}{2}&-\frac{1}{2}\end{array}\right]'),
    @('latex([[1,2],[3,4]])', '\left[\begin{array}{cc}1&2\\3&4\end{array}\right]'),
    @('latex(integrate(x^2,x,10,1))', '-333')
)
foreach ($fixture in $fixtures) {
    $lines = @($fixture[0] | & $Executable --sage)
    if ($LASTEXITCODE -ne 0) { throw "CLI transport failed: $($fixture[0])" }
    $quoted = @($lines | Where-Object { $_.StartsWith('"') -and $_.EndsWith('"') })
    if ($quoted.Count -ne 1 -or $quoted[0] -ne ('"' + $fixture[1] + '"')) {
        throw "Transport differs for $($fixture[0]): $($lines -join ' / ')"
    }
}
Write-Host "PASS Giac single-line transport: $($fixtures.Count) negative, root-list, identity-list and matrix controls."
