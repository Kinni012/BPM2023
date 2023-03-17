#Encoding => int/real
$dataSetStart = 24
$dataSetEnd = 32

#runtime in minutes 60, 180, 300 minutes
$runTime = "300"

#Apply vns
$applyVns = "1"


for ($i=$dataSetStart; $i -lt $dataSetEnd+1; $i++){
    Start-Process -FilePath "C:\develop\easy4sim\FJSSP_Optimization\bin\Release\FJSSP_Optimization.exe" -ArgumentList "L$i", $runTime, $applyVns -Workingdirectory "C:\develop\easy4sim\FJSSP_Optimization\bin\Release\"
}