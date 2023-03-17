# HeuristicLab plugin

## Overview
Cite from the [HeuristicLab homepage](https://dev.heuristiclab.com/trac.fcgi/):

<em>HeuristicLab is a framework for heuristic and evolutionary algorithms that is developed by members of the Heuristic and Evolutionary Algorithms Laboratory (HEAL) since 2002.</em>


## Plugin
HeuristicLab offers a performance focused foundation for any kind of optimization. 
It can be easily extended due to its [Plugin based architecture](https://dev.heuristiclab.com/trac.fcgi/wiki/Documentation/DevelopmentCenter/Architecture). 
HeuristicLab has a large number of Algorithms that can be applied on different optimization problems.
Since algorithms and problems are separated, we can add new problems easily by deriving from SingleObjectiveBasicProblem<IntegerVectorEncoding\> as it is done in the Easy4SimFjsspEncoding class.
This means in this plugin, we have created a problem class for a combined cobot assingnment and job shop scheduling problem as well as a problem file for a combined cobot assignment and flexible job shop scheduling problem. 

Additionally, we can create new operators for all stages of existing algorithms. An example is the PriorityRuleSolutionGenerator that can be used to create priority rule based initial solutions in a genetic algorithm.

## Variable neighborhood search
Whenever a solution in one of our problems is evaluated, we can
start a variable neighborhood search. This neighborhood searches are 
implemented in the "NeighborhoodOperators" folder. Some of these
neighborhood operators utilize local process models. These local process models and local process model mining algorithms are implemented in the 
"LocalProcessModels" folder.