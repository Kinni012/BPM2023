#!/bin/bash
#
#SBATCH --job-name=test2
#SBATCH --output=%j.txt
#SBATCH --error=%j_err.txt
#SBATCH --nodes=1
#SBATCH --partition=skylake_0096
#SBATCH --qos=skylake_0096

spack unload
spack load mono@6.12.0.122

mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
mono ./bin/Release/PaperOptimization.exe int cp1 180 1 0 1 2 0.1 0.9 5 0 0 &
wait