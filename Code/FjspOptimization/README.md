# FjspOptimization
This project is used to start the optimization of one cobot assingment and flexible job
shop scheduling problem based on console or command line input parameters.
Since it allows starting per command line parameters, shell scripts can be used to 
start multiple optimizations.

The optimization utilizes a genetic algorithm that is implemented in the optimization framework 
HeuristicLab. A HeuristicLab extension is utilized, to overwrite the problem, problem evaluation, 
initial solution generation and all other things that should be customized.