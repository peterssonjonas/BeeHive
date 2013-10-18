// <copyright file="Cell.cs" company="BeeHive by Jonas Petersson and Oskar Krantz">
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
    class Cell
    {
        //Class vars
        public Connection[] connections;
        public Color color, devModeColor;
        public Vector2 centre;
        public int indexInGrid;
        public int shortestWay;
        public int safestWay;
        public bool passable, occupied, done, hasTower;
        public int hazardLevel; // Rnd.r.Next(10);
        public int shortestDist;
        public int safestDist;
        public int guardWay;

        //Constructors
        public Cell(Vector2 centre, int index, int[] d)
        {
            color = new Color(new Vector3(1f, 0.8f, 0.5f));
            SetHazardLevel(1);
            indexInGrid = index;
            this.centre = centre;
            passable = (Rnd.r.NextDouble() > 0.2);
            hasTower = false;

            connections = new Connection[6];
            
            connections[Dir.East].targetCell = d[Dir.East];
            connections[Dir.NorthEast].targetCell = d[Dir.NorthEast];
            connections[Dir.NorthWest].targetCell = d[Dir.NorthWest];
            connections[Dir.West].targetCell = d[Dir.West];
            connections[Dir.SouthWest].targetCell = d[Dir.SouthWest];
            connections[Dir.SouthEast].targetCell = d[Dir.SouthEast];

            for (int i = 0; i < 6; i++)
                if (connections[i].targetCell >= 0)
                {
                    connections[i].exists = true;
                    connections[i].passable = true;
                }

            safestWay = Dir.None;

            shortestWay = Dir.None;
        }

        public void SetHazardLevel(int hazardLevel)
        {
            this.hazardLevel += hazardLevel;
            devModeColor = new Color(new Vector3(1f, 0.8f, 0.5f) * (1 - 0.09f * (this.hazardLevel - 1)));
        }
    }
}
