# Cobot assignment and flexible job shop scheduling problem
This is a libarary for the simulation framework easy4sim. This library can be used to
evaluate a large number of cobot assignment and flexible job shop schduling problem instances and 
return a fitness value for each instance.

The FjspLoader loads one optimization problem in the initialize method (this is called once).
After loading the problem this problem information is passed to a FjspEvaluation component
with a link in the simulation. This FjspEvaluation evaluates a specific solution and returns a 
fitness value.
To avoid multiple reoccuring file accesses, the whole SolverSettings object with the initialized 
problem instance is cloned after the initialization. The parameters that should be evaluated
are set in the start method of the FjspEvaluation class on a clone.

The FjspPriorityRuleSolutionGenerator can be used to generate priority rule solutions for 
a optimization problem. This allows us to generate good initial solution that can be optimized with a
memetic algorithm. 