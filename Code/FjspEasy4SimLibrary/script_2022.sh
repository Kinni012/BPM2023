#!/bin/bash
#
#SBATCH --job-name=test
#SBATCH --output=%j.txt
#SBATCH --error=%j_err.txt
#SBATCH --nodes=1
#SBATCH --partition=skylake_0096
#SBATCH --qos=skylake_0096

spack unload
spack load mono@6.12.0.122

mono ./bin/Release/FJSSP_Optimization.exe B1 1 1 0.2 0.1 0.1 &
wait