// <copyright file="Tower.cs" company="BeeHive by Jonas Petersson and Oskar Krantz">
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
    class Tower
    {
        public Vector2 pos;
        public static Texture2D texture;
        public bool alive;
        public int target;
        private int reloadTimer;
        Vector2 origin;

        public Tower()
        {
            alive = false;
            origin = new Vector2(40, 93);
        }

        public void Init(Vector2 pos, int cell)
        {
            this.pos = pos;
            alive = true;
            target = -1;
            reloadTimer = 0;
        }

        public void Update(ref Intruder[] intruder, ref ShotSystem shotSystem)
        {
            if (target == -1)
                getTarget(intruder);


            if (target != -1)
            {
                if ((pos - intruder[target].pos).LengthSquared() > 20000 || !intruder[target].alive)
                {
                    target = -1;
                    getTarget(intruder);
                }
                else
                {
                    if (reloadTimer == 0)
                    {
                        shotSystem.createShot(pos - 70 * Vector2.UnitY, intruder[target].pos, target);

                        reloadTimer = 20;
                    }
                }
            }
            if (reloadTimer > 0)
                reloadTimer--;
        }

        private void getTarget(Intruder[] intruder)
        {
            float minDist = 40000;
            int minIdx = -1;

            for (int i = 0; i < 50; i++)
            {
                if (intruder[i].alive)
                {
                    float dist = (pos - intruder[i].pos).LengthSquared();
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minIdx = i;
                    }
                }
            }
            target = minIdx;
        }

        public void Draw(ref SpriteBatch batch)
        {
            batch.Draw(texture, (pos - Camera.pos) * Camera.zoom + Camera.centreOfScreen, null, Color.White, 0, origin, 1.1f * Camera.zoom, SpriteEffects.None, 0);
        }
    }
}
