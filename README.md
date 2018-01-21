# WerewolfInvasion
Project of Procedural Generation using Unity

## Goal

You have to kill all werewolves ! Help the peasants by creating forges to create weapons. Kill werewolves when you see them.
Gather resources you can find in mazes !

You have around 3 minutes when you started the game before werewolves become too dangerous and kill at least every poor peasant.

## Features

- Procedural generation of a toric world
    + Procedural generation of terrain chunks (voronoi algorithm)
    + Procedural generation of trees on the terrain
    + Procedural generation of a maze (prim algorithm)
    + Procedural generation of houses on the terrain according to created routes from voronoi
- Management of peasants/armed peasants/werewolves according to SIR epidemiology model
- Little AI for Non Player Characters where they move on the ground with a NavMesh
    + Each peasant moves randomly
        - If a forge is nearby, moves to the forge and is replaced by an Armed Peasant
    + Each armed peasant runs after werewolves
    + Each werewolf runs after peasants
- Possibility to kill werewolves for the player (based on the chunk on which the player is currently)
- Possibility to enter a maze, find a treasure and earn resources (by just colliding with the treasure)
- Possibility to create a forge
- (Test) Possibility to spawn a maze
- Little UI
    + Display of the current state of the propagation of the plague
        - Number of peasants, armed peasants and werewolves
    + Display Current number of forges
    + Display current resources
    + Information messages when trying to create a forge

## How to play

- Use `ZQSD` keys to move (or arrow keys)
- Use `Space` to fly
- Use `mouse left` or `Left Shift` to activate weapon to kill werewolves on the current chunk
- Use `F` key to start placing a forge, and click with `mouse left` to place it
- Use `L` key to spawn a maze nearby.
- Use `Left Ctrl` to sprint

## Video Presentation

[YouTube Link](https://youtu.be/YnIq_Z3ACRw)