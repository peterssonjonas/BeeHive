// <copyright file="Guard.cs" company="BeeHive by Jonas Petersson and Oskar Krantz">
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
    class Guard
    {
        private const int STEPS_PER_CELL = 30;
        public static Texture2D texture;
        static Vector2 origin = new Vector2(50, 65);
        public Vector2 pos;
        public bool alive;
        public bool safe;
        public int health;
        public int target;
        GuardStrategy strategy;
        GuardPathfinding pathing;
        bool targetLookAhead;

        public Guard()
        {
            target = -1;
            alive = false;
            
        }

        int prevCell, nextCell, progress;

        public void Init(ref Grid grid)
        {
            target = -1;
            alive = false;
            health = 100;
            pos = grid.cells[grid.indexOfExit].centre;
            safe = Rnd.r.NextDouble() > 0.5;
            progress = 0;
            prevCell = grid.indexOfExit;
            nextCell = prevCell;
            

            strategy = GuardStrategy.GoForMostCritical;
            pathing = GuardPathfinding.Dumb;
            targetLookAhead = false;
        }


        public void Update(ref Grid grid, Intruder[] intruder, ref ShotSystem shotSystem)
        {
            if (target == -1)
                target = GetNewTarget(ref grid, intruder);
            
            if (target != -1 && !intruder[target].alive)
                target = GetNewTarget(ref grid, intruder);

            if (progress % 20 == 0 && target != -1 && intruder[target].alive && (pos - intruder[target].pos).LengthSquared() < 10000)
                shotSystem.createShot(pos - Vector2.One * 20, intruder[target].pos, target);

            pos = (progress * grid.cells[nextCell].centre + (STEPS_PER_CELL - progress) * grid.cells[prevCell].centre) / (float)STEPS_PER_CELL;
            
            if (progress >= STEPS_PER_CELL) 
                if (target != -1 && intruder[target].alive)
                {
                    if (pathing == GuardPathfinding.Dumb)
                    {
                        float dirToTarget = (float)((Math.Atan2(grid.cells[intruder[target].nextCell].centre.Y - pos.Y, pos.X - grid.cells[intruder[target].nextCell].centre.X) / Math.PI + 1) * 3);
                        int dirToMove = Dir.None;
                        float diff;
                        float minDiff = 7;

                        for (int d = 0; d < 6; d++)
                        {
                            if (grid.cells[nextCell].connections[d].passable)
                            {
                                diff = Math.Abs(d - dirToTarget);
                                if (diff > 3) diff = 6 - diff;
                                if (diff < minDiff)
                                {
                                    minDiff = diff;
                                    dirToMove = d;
                                }
                            }
                        }
                        if (dirToMove != -1)
                        {
                            int newNextCell = grid.cells[nextCell].connections[dirToMove].targetCell;
                            prevCell = nextCell;
                            nextCell = newNextCell;
                            progress = 0;

                        }
                    }
                    else
                    {
                        grid.CalcGuardPath(nextCell, intruder[target].nextCell);
                        
                        int dir = grid.cells[nextCell].guardWay;
                        if (dir != -1)
                        {
                            prevCell = nextCell;
                            nextCell = grid.cells[nextCell].connections[dir].targetCell;
                            progress = 0;
                        }
                        else
                        {
                            prevCell = nextCell;
                            
                            progress = 10;
                        }
                    }

                }
                
            

            if (progress < STEPS_PER_CELL)
                progress++;
        }



        private int GetNewTarget(ref Grid grid, Intruder[] intruder)
        {
            int newTarget = -1;
            float minDist = 10000000;
            float dist;
            switch (strategy)
            {
                case GuardStrategy.GoForClosest:
                    if (targetLookAhead) ;

                    else
                        for (int n = 0; n < 50; n++)
                        {
                            if (intruder[n].alive)
                            {
                                dist = (intruder[n].pos - pos).LengthSquared();
                                if (dist < minDist)
                                {
                                    newTarget = n;
                                    minDist = dist;
                                }
                            }
                        }
                    break;

                case GuardStrategy.GoForMostCritical:
                    if (targetLookAhead) ;

                    else
                        for (int n = 0; n < 50; n++)
                        {
                            if (intruder[n].alive)
                            {
                                dist =  grid.cells[intruder[n].nextCell].shortestDist;
                                if (dist < minDist)
                                {
                                    newTarget = n;
                                    minDist = dist;
                                }
                            }
                        }

                    break;


                case GuardStrategy.MaximizeEfficiency:


                    break;


            }

            return newTarget;
        }

        public void SwitchPathing()
        {
            if (pathing == GuardPathfinding.Dumb) pathing = GuardPathfinding.Smart;
            else pathing = GuardPathfinding.Dumb;

        }

        public void SwitchStrategy()
        {
            if (strategy == GuardStrategy.GoForClosest) strategy = GuardStrategy.GoForMostCritical;
            else strategy = GuardStrategy.GoForClosest;

        }

        public void Draw(ref SpriteBatch batch)
        {
            batch.Draw(texture, (pos - Camera.pos) * Camera.zoom + Camera.centreOfScreen, null, pathing == GuardPathfinding.Dumb ?  Color.White : Color.Red, 0, origin, Camera.zoom, SpriteEffects.None, 0);
        }
    }
}