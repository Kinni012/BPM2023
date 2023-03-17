#Encoding => int/real
$dataSetStart = 2
$dataSetEnd = 9

#runtime in minutes 5, 30, 60, 180 minutes
$runTime = "180"

#Apply vns
$applyVns = "1"


for ($i=$dataSetStart; $i -lt $dataSetEnd+1; $i++){
    Start-Process -FilePath "C:\develop\easy4sim\FJSSP_Optimization\bin\Release\FJSSP_Optimization.exe" -ArgumentList "B$i", $runTime, $applyVns -Workingdirectory "C:\develop\easy4sim\FJSSP_Optimization\bin\Release\"
}