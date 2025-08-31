# Start each project and keep process objects
$procs = @()
$procs += Start-Process dotnet -ArgumentList 'run --project ".\ContractViewerApi\UserApi\UserApi.csproj"' -PassThru
$procs += Start-Process dotnet -ArgumentList 'run --project ".\ContractViewerApi\PowerOfAttorneyApi\PowerOfAttorneyApi.csproj"' -PassThru
$procs += Start-Process dotnet -ArgumentList 'run --project ".\ContractViewerApi\ContractListApi\ContractListApi.csproj"' -PassThru
$procs += Start-Process dotnet -ArgumentList 'run --project ".\ContractViewerApi\ContractDetailsApi\ContractDetailsApi.csproj"' -PassThru
$procs += Start-Process dotnet -ArgumentList 'run --project ".\ContractViewerApi\ContractViewerApi\ContractViewerApi.csproj"' -PassThru
$procs += Start-Process dotnet -ArgumentList 'run --project ".\ContractViewerApi\OtherServiceApi\OtherServiceApi.csproj"' -PassThru

Write-Host "Press 'q' to stop all processes."

while ($true) {
    if ([Console]::KeyAvailable) {
        $key = [Console]::ReadKey($true).Key
        if ($key -eq 'Q') {
            Write-Host "Stopping all processes..."
            foreach ($p in $procs) {
                if (!$p.HasExited) {
                    $p.Kill()
                }
            }
            break
        }
    }
    Start-Sleep -Milliseconds 100
}
