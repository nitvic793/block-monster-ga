# Block Monster Genetic Algorithm

Genetic Algorithm for a 2 Hinged block creature which can apply force to both of its "legs".

## Genome encoding 
- 3 Force Vectors x 2 Rhythms encoded sequentially in a floating point array.
- [x11,y11,z11,x12,y12,z12,x13,y13,z13,x21,y21,z21,x22,y22,z22,x23,y23,z23]
- Where (x11,y11,z11) corresponds to Left "limb" force, then right limb force and then front force.

## Parameters

- Selection - Fitness Proportionate Selection
- Crossover - Uniform crossover with 50% chance
- Mutation - 0.5 probability to modify one