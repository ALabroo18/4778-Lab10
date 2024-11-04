# 4778-Lab10

## Video 



https://github.com/user-attachments/assets/ab97aa55-5743-464c-8c76-eb3c6bf81d5a


## Questions
### How does A* pathfinding calculate and prioritize paths?
We used the A* pathfinding algorithm by creating a dictionary which holds Vector2Int's, and using the current and next Vector2DInt variables that determine where the current and next positions are in the path. We created a while loop that continues until the number of Vector2Int's in the frontier queue are empty, and store the current variable with the first variable in the queue, while also dequing at the same time. Right after this, we made an if statemnet that checks if the current variable is equivilent to the goal Vector2Int location, and if so, the loop will break. If not, a foreach loop will be used that for every direction in the directions array, the next direction variable will be equal to the total of the current and the direction in the array that is firstmost. The looop will also check to see if the next direction is inbounds, which if so would then add it to the Frontier queue and the dictionary we made would get updated by having its next position be equal to the current. If there is no path, then there will be a Debug.log statement that will be printed. Finally, path will be traced from the goal direction, to the start direction.


### What challenges arise when dynamically updating obstacles in real-time?
When in real time, there might be issues where the program might have a difficult time in determining whether or not there will be a obstacle in the position that the probability locates to, which means that there could be some obstacles that are not shown.

### How could you adapt this code for larger grids or open-world settings?
We could change the directions array to dynamically update depending on how big the grid is going to be. This will allow for more than four directions, which can help with enlarging the grid. As of right now, the grid sizes can only go up to 20 x 20 dimensions, so we'd have to allow for it to past those restrictions.

### What would your approach be if you were to add weighted cells (e.g., "difficult terrain" areas)?
If we were to add weighted cells, then for each cell we'd have to allocate space for each part of the terrain that would get added. For example, if the map was a grassy area, with hills that players can go on, then the number of cells would increase, and the size of each cell would drastically change.
