#Encoding => int/real
$encoding = "int"

#Dataset, cp1-cp50
$dataSetStart = 39
#inclusive both borders 1-14 will start 4 tasks, 11, 12, 13, 14
$dataSetEnd = 46

#runtime in minutes 1h/3h/5h
$runtime = "300"

#repetitions
$repetitions = "1"

#TimeFactor
$timeFactor = "1"

#CostFactor
$costFactor = "1"

#Apply vns
$applyVns = "1"

#Amount of cobots
$cobots = "10"

#Minimize or maximize
$minOrMax = "1"

#Use normalized value (0 == false, 1== true)?
$normalizedValue = "1"

#Deterministic or stochastic (0 == Deterministic, 1 = Stochastic)
$determinism = "0"

for ($i=$dataSetStart; $i -lt $dataSetEnd+1; $i++){
    Start-Process -FilePath "C:\develop\easy4sim\PaperOptimization\bin\Release\PaperOptimization.exe" -ArgumentList $encoding, "cp$i", $runtime, $repetitions, $timeFactor, $costFactor, $applyVns, $cobots, $minOrMax, $normalizedValue, $determinism -Workingdirectory "C:\develop\easy4sim\PaperOptimization\bin\Release\"
				  
}