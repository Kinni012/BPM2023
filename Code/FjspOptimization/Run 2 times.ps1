#Encoding => int/real
$dataSet = "BBD0.0"

#runtime in minutes 60, 180, 300 minutes
$runTime = "60"

#Apply vns
$applyVns = "1"

#Cobot percentage
$cobotPercentage = "0.2"

$vnsPercentage = "0.1"

$vnsNeighborhood = 2


for ($i=0; $i -lt 2; $i++){
    Start-Process -FilePath "C:\develop\easy4sim\FJSSP_Optimization\bin\Release\FJSSP_Optimization.exe" -ArgumentList $dataSet, $runTime, $applyVns, $cobotPercentage, $vnsPercentage, $vnsNeighborhood -Workingdirectory "C:\develop\easy4sim\FJSSP_Optimization\bin\Release\"
}