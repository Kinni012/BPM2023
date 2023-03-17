# Easy4Sim
## Before we start
This simulation framework is developed by the [RISC Software GmbH](https://www.risc-software.at/ "RISC Website").

## Running a simulation
### Libraries
To start a simulation, a simulation library needs to be created.
Classes of this simulation library need to implement the CSimBase class of the easy4sim framework.
Once this is done, these classes can be used in the simulation.
Each simulation object has a unique name and identifier.
Additionally, each simulation object can overwrite methods, examples for these methods are:
 - Start &rarr; Called before the simulation starts
 - End &rarr; Called at the end of the simulation
 - DiscreteSimulation &rarr; Called on component event
 - DynamicCalculation &rarr; Called in every step of the simulation

### SolverSettings
The SolverSettings class contains all information regarding one simulation:
 - Simulation objects
 - Environment
 - Statistics
 - Logger

The simulation objects class contains all components with links between these components. 
A link describes information that is transported in the simulation.
An example would be one simulation object, that loads a problem.
That component might have a connection to component that returns a
fitness value for this loaded problem.

In the environment class, information such as the starting time, the end time and the
time increase is stored. Additionally it contains a flag "SimulationFinished" that
can be set by any component to end the simulation.

The simulation statistics class can be used to important information regading the 
current simulation run, an example would be the fitness of one run.

The logger class can be used for efficient logging of one simulation run.

### Solver
A solver class is initialized with a SolverSettings class. It allows different
ways to run the simulation. An example is to run to a specific simulation time or
until one component of the simulation sets the finished flag.
