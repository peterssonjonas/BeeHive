// <copyright file="Grid.cs" company="BeeHive by Jonas Petersson and Oskar Krantz">
// Copyright (c) 10-18-2013 All Rights Reserved
// </copyright>
// <author>Jonas Petersson, Oskar Krantz</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BeeHive
{

    class Grid
    {
        const int NO_OF_TOWERS = 5;
        //Klassvariabler
        public Texture2D cellTexture, wallTexture, blockTexture, safeArrow, shortArrow;
        public Tower[] tower;
        public List<Cell> cells;
        public int indexOfEntry, indexOfExit;

        const float cellWidth = 100f;
        const float cellHeight = 60f;
        Vector2 cellOrigin = new Vector2(100, 80);
        Vector2 blockOrigin = new Vector2(100, 94);
        Vector2 arrowOrigin = new Vector2(0, 20);

        int height, width, numCells, indexAtMouse;
        int[] startIndexOfRow;

        //Constructor
        public Grid(int height, int width)
        {
            this.height = height;
            this.width = width;
            
            cells = new List<Cell>();
            startIndexOfRow = new int[height];
            int index = 0;
            int[] d = new int[6];
            tower = new Tower[NO_OF_TOWERS];
            for (int n = 0; n < NO_OF_TOWERS; n++)
                tower[n] = new Tower();

            for (int row = 0; row < height; row++)
            {
                startIndexOfRow[row] = index;
                int widthOfRow = width - Math.Abs(row - height / 2);

                for (int col = 0; col < widthOfRow; col++)
                {
                    //Räkna ut positionen för en cell.
                    Vector2 pos = new Vector2(1366 * 0.5f - (widthOfRow - 0.5f) * 0.5f * cellWidth + col * cellWidth, 768 * 0.5f - (height - 1) * 0.5f * cellHeight + row * cellHeight);
                    
                    d[(int)Dir.East] = (col < widthOfRow - 1 ? index + 1 : -1);
                    d[(int)Dir.NorthEast] = (row <= height / 2 ? (col < widthOfRow - 1 && row > 0 ? index - widthOfRow + 1 : -1) : index - widthOfRow);
                    d[(int)Dir.NorthWest] = (row <= height / 2 ? (col > 0 && row > 0 ? index - widthOfRow : -1) : index - widthOfRow - 1);
                    d[(int)Dir.West] = (col > 0 ? index - 1 : -1);
                    d[(int)Dir.SouthWest] = (row >= height / 2 ? (col > 0 && row < height - 1 ? index + widthOfRow - 1 : -1) : index + widthOfRow);
                    d[(int)Dir.SouthEast] = (row >= height / 2 ? (col < widthOfRow - 1 && row < height - 1 ? index + widthOfRow : -1) : index + widthOfRow + 1);
                    Cell c = new Cell(pos, index, d);

                    cells.Add(c);
                    index++;
                }
            }

            numCells = index;
            indexOfEntry = startIndexOfRow[height / 2];
            indexOfExit = startIndexOfRow[height / 2 + 1] - 1;

            //Se till att in och utgången inte är blockerad
            cells[indexOfEntry].passable = true;
            cells[indexOfEntry].color = cells[indexOfEntry].devModeColor = new Color(0.7f, 1f, 0.8f);
            cells[indexOfExit].passable = true;
            cells[indexOfExit].color = cells[indexOfExit].devModeColor = new Color(1f, 0.4f, 0.1f);


            //Sätt rätt värde på alla Connection.passable
            for (int n = 0; n < numCells; n++)
            {
                if (!cells[n].passable)
                    SetCellUnpassable(n);
            }

            CalcShortestPath();
            CalcSafestPath();

        }

        public void Update(MouseState mouse, MouseState oldMouse, ref Intruder[] intruder, ref ShotSystem shotSystem)
        {
            // X = 1366 * 0.5f - (widthOfRow - 0.5f) * 0.5f * cellWidth + col * cellWidth, 
            // Y = 768 * 0.5f - (height - 1) * 0.5f * cellHeight + row * cellHeight);

            Vector2 mouseWorldPos;
            mouseWorldPos.X = (mouse.X - Camera.centreOfScreen.X) / Camera.zoom + Camera.pos.X;
            mouseWorldPos.Y = (mouse.Y - Camera.centreOfScreen.Y) / Camera.zoom + Camera.pos.Y;
                // (w.X) = 

            int targetCell = 0, counter = 0;

            int row = (int)(mouseWorldPos.Y + 0.5f * cellHeight + (height - 1) * 0.5f * cellHeight - 768 * 0.5f) / (int)cellHeight;
            int widthOfRow = width - Math.Abs(row - height / 2);
            int col = (int)(mouseWorldPos.X + 0.5f * cellWidth + (widthOfRow - 0.5f) * 0.5f * cellWidth - 1366 * 0.5f) / (int)cellWidth;

            if (row >= 0 && row < height && col >= 0 && col < width)
                indexAtMouse = startIndexOfRow[row] + col;
            else
                indexAtMouse = -1;


            if (mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released && indexAtMouse != -1 && !cells[indexAtMouse].passable && !cells[indexAtMouse].hasTower)
            {
                while (counter < NO_OF_TOWERS && tower[counter % NO_OF_TOWERS].alive)
                    counter++;

                if (!tower[counter % NO_OF_TOWERS].alive)
                {
                    tower[counter % NO_OF_TOWERS].Init(cells[indexAtMouse].centre, indexAtMouse);
                    cells[indexAtMouse].SetHazardLevel(4);
                    cells[indexAtMouse].hasTower = true;
                    for (int d = 0; d < 6; d++)
                    {
                        targetCell = cells[indexAtMouse].connections[d].targetCell;
                        if (targetCell != -1)
                            if (cells[targetCell].indexInGrid != indexOfEntry && cells[targetCell].indexInGrid != indexOfExit)
                                cells[targetCell].SetHazardLevel(2);
                    }
                    CalcSafestPath();
                    counter++;
                }
            }

            if (mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released && indexAtMouse != -1 && cells[indexAtMouse].passable && !cells[indexAtMouse].occupied)
            {
                SetCellUnpassable(indexAtMouse);
                CalcShortestPath();
                if (cells[indexOfEntry].shortestDist == Dir.None)
                {
                    SetCellPassable(indexAtMouse);
                    CalcShortestPath();
                }
                else
                    foreach (Intruder i in intruder)
                    {
                        if (i.alive && cells[i.nextCell].shortestWay == Dir.None)
                        {
                            SetCellPassable(indexAtMouse);
                            CalcShortestPath();
                            break;
                        }
                    }
                if (!cells[indexAtMouse].passable)
                    CalcSafestPath();
            }
            if (mouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released && indexAtMouse != -1 && !cells[indexAtMouse].passable)
            {
                for (int n = 0; n < NO_OF_TOWERS; n++)
                    if (tower[n].alive && tower[n].pos == cells[indexAtMouse].centre)
                    {
                        tower[n].alive = false;
                        cells[indexAtMouse].hasTower = false;
                        if (cells[indexAtMouse].hazardLevel >= 5)
                            cells[indexAtMouse].SetHazardLevel(-4);
                        else cells[indexAtMouse].hazardLevel = 1;
                        for (int d = 0; d < 6; d++)
                        {
                            targetCell = cells[indexAtMouse].connections[d].targetCell;
                            if (targetCell != -1)
                                if (cells[targetCell].hazardLevel >= 3)
                                    cells[targetCell].SetHazardLevel(-2);
                        }
                        CalcSafestPath();
                        counter = n;
                        return;
                    }
                SetCellPassable(indexAtMouse);
                CalcSafestPath();
                CalcShortestPath();
            }
            if (!Game1.paused)
                for (int n = 0; n < NO_OF_TOWERS; n++)
                    if (tower[n].alive)
                        tower[n].Update(ref intruder, ref shotSystem);
        }
    

        void SetCellPassable(int n)
        {
            for (int dir = 0; dir < 6; dir++)
            {
                int targetCell = cells[n].connections[dir].targetCell;
                if (targetCell >= 0 && cells[targetCell].passable)
                {
                    cells[n].connections[dir].passable = true;
                    cells[cells[n].connections[dir].targetCell].connections[(dir + 3) % 6].passable = true;
                }
            }
            cells[n].passable = true;
        }

        void SetCellUnpassable(int n)
        {
            for (int dir = 0; dir < 6; dir++)
            {
                if (cells[n].connections[dir].targetCell >= 0)
                {
                    cells[n].connections[dir].passable = false;
                    cells[cells[n].connections[dir].targetCell].connections[(dir + 3) % 6].passable = false;
                }
            }
            cells[n].passable = false;
        }

        public bool CalcShortestPath()
        {
            //TO-DO: En algoritm för att räkna ut kortaste vägen till utgången från varje cell.
            //Om någon cell som innehåller ingången till världen eller en insekt inte har en väg till utgången returneras false, annars true.
            Queue<int> queue = new Queue<int>();
            bool[] visited = new bool[numCells];
            int current, targetCell;

            foreach (Cell c in cells)
            {
                c.shortestDist = 9999;
                c.shortestWay = Dir.None;
                c.done = false;
            }

            queue.Enqueue(indexOfExit);
            cells[indexOfExit].shortestDist = 0;

            while (queue.Count > 0)
            {
                current = queue.Dequeue();
                visited[current] = true;

                for (int d = 0; d < 6; d++)
                    if (cells[current].connections[d].passable)
                    {
                        targetCell = cells[current].connections[d].targetCell;
                        if (!visited[targetCell])
                        {
                            queue.Enqueue(targetCell);
                            cells[targetCell].shortestDist = cells[current].shortestDist + 1;
                            cells[targetCell].shortestWay = (d + 3) % 6;
                            visited[targetCell] = true;
                        }
                    }
            }

            return true;
        }


        public void CalcSafestPath()
        {
            //TO-DO: En algoritm för att räkna ut den lägst viktade vägen till utgången från varje cell.
            //Efterson framkomlighet testas i .
            int current = 0, targetCell = 0;

            foreach (Cell c in cells)
            {
                c.safestDist = 9999;
                c.safestWay = Dir.None;
                c.done = false;
            }

            cells[indexOfExit].safestDist = 0;
            cells[indexOfExit].done = true;
            current = indexOfExit;

            while (true)
            {
                for (int d = 0; d < 6; d++)
                    if (cells[current].connections[d].passable)
                    {
                        targetCell = cells[current].connections[d].targetCell;

                        if (!cells[targetCell].done && cells[targetCell].safestDist > cells[current].safestDist + cells[targetCell].hazardLevel)
                        {
                            cells[targetCell].safestDist = cells[current].safestDist + cells[targetCell].hazardLevel;
                            cells[targetCell].safestWay = (d + 3) % 6;
                        }
                    }

                int dist = 9999;

                foreach (Cell c in cells)
                {
                    if (!c.done && c.safestDist < dist)
                    {
                        dist = c.safestDist;
                        current = c.indexInGrid;
                    }
                }
                if (dist == 9999) break;

                cells[current].done = true;

            }
        }


        public bool CalcGuardPath(int guardCell, int target)
        {
            //TO-DO: En algoritm för att räkna ut kortaste vägen till utgången från varje cell.
            //Om någon cell som innehåller ingången till världen eller en insekt inte har en väg till utgången returneras false, annars true.
            Queue<int> queue = new Queue<int>();
            bool[] visited = new bool[numCells];
            int current, targetCell;

            foreach (Cell c in cells)
            {
                c.guardWay = Dir.None;
                c.done = false;
            }

            queue.Enqueue(target);
            
            while (queue.Count > 0)
            {
                current = queue.Dequeue();
                visited[current] = true;
                if (current == guardCell) return true;

                for (int d = 0; d < 6; d++)
                    if (cells[current].connections[d].passable)
                    {
                        targetCell = cells[current].connections[d].targetCell;
                        if (!visited[targetCell])
                        {
                            queue.Enqueue(targetCell);
                            cells[targetCell].guardWay = (d + 3) % 6;
                            visited[targetCell] = true;
                            if (targetCell == guardCell) return true;
                        }
                    }
            }

            return true;
        }



        public void Draw(ref SpriteBatch batch)
        {
            float light;
            foreach (Cell c in cells)
            {
                if (c.indexInGrid == indexAtMouse) light = 0.6f;
                else light = 1;
                batch.Draw(cellTexture, (c.centre - Camera.pos) * Camera.zoom + Camera.centreOfScreen, null, (Game1.devMode ? c.devModeColor : c.color) * light, 0, cellOrigin, 0.5f * Camera.zoom, SpriteEffects.None, 0);
            }
            if (Game1.devMode)
                foreach (Cell c in cells)
                {
                    if (c.safestWay != Dir.None) batch.Draw(safeArrow, (c.centre - Camera.pos) * Camera.zoom + Camera.centreOfScreen, null, Color.White, -c.safestWay * (float)Math.PI * 0.33f, arrowOrigin, 0.3f * Camera.zoom, SpriteEffects.None, 0);
                    if (c.shortestWay != Dir.None) batch.Draw(shortArrow, (c.centre - Camera.pos) * Camera.zoom + Camera.centreOfScreen, null, Color.White, -c.shortestWay * (float)Math.PI * 0.33f, arrowOrigin, 0.3f * Camera.zoom, SpriteEffects.None, 0);
                }

            foreach (Cell c in cells)
                if (!c.passable) batch.Draw(blockTexture, (c.centre - Camera.pos) * Camera.zoom + Camera.centreOfScreen, null, Color.White, 0, blockOrigin, 0.5f * Camera.zoom, SpriteEffects.None, 0);

           
        }


        public void DrawTowers(ref SpriteBatch batch)
        {
            for (int n = 0; n < NO_OF_TOWERS; n++)
                if (tower[n].alive)
                    tower[n].Draw(ref batch);
        }
    
    }
}
