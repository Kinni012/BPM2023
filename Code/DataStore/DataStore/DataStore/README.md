# Datastore
The data store class contains all problems that should be optimized by the algorithm. 
The idea is, that all problem files are packed in one library and the FileLoader class 
can be used to get problem instances out of this library. The advantage of this method is,
that no file paths are necessary, which allows us to run this code on windows computers as 
well as on linux computers via the mono framework.