
$stopwatch = [system.diagnostics.stopwatch]::new()

[int]$global:totalTime = 0
function Update-Total([int]$add){
    $global:totalTime += $add
}
function Run-Task($name, $execTask){
    Start-Task $name
    Invoke-Command $execTask
    End-Task $name
}
function Start-Task([string]$name){
    $stopwatch.Start();
    Write-Host ""
    Write-Host $name -ForegroundColor Green
}

function End-Task([string]$name){
    $stopwatch.Stop();
    $elapsed = $stopwatch.ElapsedMilliseconds;
    Update-Total $elapsed
    Write-Host "$name done in $elapsed ms"
}


function End-Build(){
    Write-Host ""
    $total = ([math]::Round( ($global:totalTime / 100) )) / 10
    Write-Host "Total time: $total seconds ($global:totalTime ms)" -ForegroundColor Yellow
    Write-Host ""
}