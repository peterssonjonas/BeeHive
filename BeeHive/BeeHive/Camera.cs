// <copyright file="Camera.cs" company="BeeHive by Jonas Petersson and Oskar Krantz">
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
    static class Camera
    {
        public static float zoom = 0.7f;
        public static Vector2 pos = Vector2.Zero;
        private static Vector2 vel = Vector2.Zero;
        public static Vector2 centreOfScreen;


        public static void Scroll(Vector2 dir)
        {
            vel += dir / zoom;
        }

        public static void ZoomIn()
        {
            zoom *= 1.021f;
        }

        public static void ZoomOut()
        {
            zoom *= 0.98f;
        }

        public static void Update()
        {
            vel *= 0.9f;
            pos += vel;
        }

    }
}
