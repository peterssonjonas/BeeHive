// <copyright file="ShotSystem.cs" company="BeeHive by Jonas Petersson and Oskar Krantz">
// Copyright (c) 10-18-2013 All Rights Reserved
// </copyright>
// <author>Jonas Petersson, Oskar Krantz</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BeeHive
{
    class ShotSystem
    {
        public Texture2D texture; 
        Shot[] shots;
        Vector2 origin;
        int currentShot = 0;

        public ShotSystem()
        {
            shots = new Shot[40];
            for (int n = 0; n < 40; n++)
                shots[n] = new Shot();

            origin = new Vector2(15, 15);
        }

        public void createShot(Vector2 pos, Vector2 targetPos, int target)
        {
            shots[currentShot].Init(pos, targetPos, target);
            currentShot++;
            currentShot %= 40;

        }

        public void Update(ref Intruder[] intruder)
        {
            
            foreach (Shot shot in shots)
            {
                if (!shot.dead)
                {
                    shot.Update();
                    if (shot.dead)
                        intruder[shot.target].health -= 20;
                }
            }
            

            

            //for (LinkedListNode<Shot> iterator = shots.First; true; iterator = iterator.Next)
            //{
            //    iterator.Value.Update();
            //    if (iterator.Value.dead)
            //    {
            //        intruder[iterator.Value.target].health -= 10;
            //        shots.Remove(iterator);
            //    }
            //    if (iterator == shots.Last) break;
            //}
        }

        public void Draw(ref SpriteBatch batch)
        {
            foreach (Shot shot in shots)
            {
                if (!shot.dead)
                    batch.Draw(texture, (shot.pos - Camera.pos) * Camera.zoom + Camera.centreOfScreen, null, Color.White, 0, origin, 1.2f * Camera.zoom, SpriteEffects.None, 0);
            }
        }

    }
}
