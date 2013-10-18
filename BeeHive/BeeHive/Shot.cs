// <copyright file="Shot.cs" company="BeeHive by Jonas Petersson and Oskar Krantz">
// Copyright (c) 10-18-2013 All Rights Reserved
// </copyright>
// <author>Jonas Petersson, Oskar Krantz</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeeHive
{
    class Shot
    {
        public Vector2 pos, vel;
        public bool dead;
        int life;
        public int target;

        public Shot()
        {
            dead = true;
        }

        public void Init(Vector2 pos, Vector2 endPos, int target)
        {
            this.pos = pos;
            this.target = target;
            dead = false;
            float dist = (endPos - this.pos).Length();
            life = (int) dist / 17 + 1;
            vel = (endPos - this.pos) / life;
        }

        public void Update()
        {
            if (--life < 0) dead = true;
            else
            {
                pos += vel;
                
            }
        }

    }
}
