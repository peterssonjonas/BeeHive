// <copyright file="Intruder.cs" company="BeeHive by Jonas Petersson and Oskar Krantz">
// Copyright (c) 10-18-2013 All Rights Reserved
// </copyright>
// <author>Jonas Petersson, Oskar Krantz</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BeeHive
{
    class Intruder
    {
        public static Texture2D texture;
        static Vector2 origin = new Vector2(120, 100);
        public Vector2 pos;
        public bool alive;
        public bool safe;
        public int health;

        public Intruder()
        {
            alive = false;
        }

        public int prevCell, nextCell, progress;

        public void Init(ref Grid grid, bool isSmart)
        {
            alive = true;
            health = 100;
            pos = grid.cells[grid.indexOfEntry].centre;
            safe = isSmart;
            progress = 0;
            prevCell = grid.indexOfEntry;
            if (grid.cells[prevCell].safestWay == Dir.None)
                nextCell = prevCell;
            else
            {
                if (safe) nextCell = grid.cells[prevCell].connections[grid.cells[prevCell].safestWay].targetCell;
                else nextCell = grid.cells[prevCell].connections[grid.cells[prevCell].shortestWay].targetCell;
            }
        }

        public void Update(ref Grid grid)
        {
            if (health <= 0)
            {
                alive = false;
                grid.cells[nextCell].occupied = false;
                if (Game1.learning)
                {
                    if (prevCell != grid.indexOfEntry)
                        grid.cells[prevCell].SetHazardLevel(1);
                    if (nextCell != grid.indexOfExit)
                        grid.cells[nextCell].SetHazardLevel(1);
                    grid.CalcSafestPath();
                }
                return;
            }
            pos = (progress * grid.cells[nextCell].centre + (40 - progress) * grid.cells[prevCell].centre) / 40f;

            if (progress >= 40)
            {
                int newNextCell;


                if (nextCell == grid.indexOfExit)
                {
                    alive = false;
                    grid.cells[nextCell].occupied = false;
                    progress = 0;
                    return;
                }

                else
                {
                    if (grid.cells[nextCell].safestWay == Dir.None)
                        newNextCell = nextCell;
                    else
                    {
                        if (safe) newNextCell = grid.cells[nextCell].connections[grid.cells[nextCell].safestWay].targetCell;
                        else newNextCell = grid.cells[nextCell].connections[grid.cells[nextCell].shortestWay].targetCell;
                    }

                    if (!grid.cells[newNextCell].occupied)
                    {
                        prevCell = nextCell;
                        if (prevCell != grid.indexOfExit)
                            nextCell = newNextCell;

                        grid.cells[prevCell].occupied = false;
                        grid.cells[nextCell].occupied = true;

                        progress = 0;
                    }
                }
            }

            if (progress < 40)
                progress++;
        }

        public void Draw(ref SpriteBatch batch)
        {
            batch.Draw(texture, (pos - Camera.pos) * Camera.zoom + Camera.centreOfScreen, null, safe ? Color.DarkRed : Color.White, 0, origin, 0.3f * Camera.zoom, SpriteEffects.None, 0);
        }
    }
}
