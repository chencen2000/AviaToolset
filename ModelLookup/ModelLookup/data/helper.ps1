$records=Get-Content .\all_data.json | ConvertFrom-Json

function Get-UniqueMaker ($records){
    $maker = $records.make
    $ret=$maker | Sort-Object -CaseSensitive | Get-Unique
    return $ret
}

$x=Get-UniqueMaker $records
Write-Host $x