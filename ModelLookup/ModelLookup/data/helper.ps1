# $records=Get-Content .\all_data.json | ConvertFrom-Json

function Get-UniqueMaker ($records){
    $maker = $records.make
    $ret=$maker | Sort-Object -CaseSensitive | Get-Unique
    return $ret
}

function Sort-Tac(){
    $tac=@{}
    $files=@("google_pixel.json", ".\LG_models.json",".\motorola_model.json")
    foreach($i in $files){
        $data= Get-Content $i | ConvertFrom-Json
        foreach($j in $data.psobject.properties){
            Write-Host $j.Name
            foreach($k in $j.Value){
                Write-Host "$($k.uuid)=$($k.model)"
                if($tac.Contains($k.uuid)){
                    $l=$tac[$k.uuid]
                    $l.Add($k.model)
                }
                else{
                    $l=[System.Collections.ArrayList]@()
                    $l.Add($k.model)
                    $tac.Add($k.uuid,$l)
                }
            }
        }
    }
    ConvertTo-Json -InputObject $tac | Out-File test.json
}

