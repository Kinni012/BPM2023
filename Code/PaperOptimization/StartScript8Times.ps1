#Encoding => int/real
$encoding = "int"

#runtime in minutes 1h/3h/5h
$runtime = "300"

#TimeFactor
$timeFactor = "1"

#CostFactor
$costFactor = "1"

#Apply vns
$applyVns = "1"

$Neighborhood = "1"

#Apply vns
$vnsNeighborhood = "0.1"
#Apply vns
$secondary = "0.9"

#Amount of cobots
$cobots = "10"

#Minimize or maximize
$minOrMax = "0"

#Use normalized value (0 == false, 1== true)?
$normalizedValue = "0"


for ($i=1; $i -lt 9; $i++){
    Start-Process -FilePath "C:\develop\easy4sim\PaperOptimization\bin\Release\PaperOptimization.exe" -ArgumentList $encoding, "cp1", $runtime, $timeFactor, $costFactor, $applyVns, $Neighborhood, $vnsNeighborhood, $secondary, $cobots, $minOrMax, $normalizedValue -Workingdirectory "C:\develop\easy4sim\PaperOptimization\bin\Release\"
				  
}