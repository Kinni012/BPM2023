# Cobot assignment and job shop scheduling problem
This is a libarary for the simulation framework easy4sim. This library can be used to
evaluate a large number of cobot assignment and job shop schduling problem instances and 
return a fitness value for each instance.

The ProjectLoader loads one optimization problem in the initialize method (this is called once).
After loading the problem this problem information is passed to a ProjectProcessor component
with a link in the simulation. This project processor evaluates a specific solution and returns a 
fitness value.
To avoid multiple reoccuring file accesses, the whole SolverSettings object with the initialized 
problem instance is cloned after the initialization. The parameters that should be evaluated
are set in the start method of the ProjectProcessor class on a clone.




