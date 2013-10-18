// <copyright file="Game1.cs" company="BeeHive by Jonas Petersson and Oskar Krantz">
// Copyright (c) 10-18-2013 All Rights Reserved
// </copyright>
// <author>Jonas Petersson, Oskar Krantz</author>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BeeHive
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const int NO_OF_INTRUDERS = 50;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D cursor;
        Grid grid;
        MouseState mouse;
        Vector2 mousePos;
        Intruder[] intruder;
        ShotSystem shotSystem;
        Guard guard;
        public static bool paused = false;
        public static bool devMode = false;
        public static bool smartIntruders = false;
        public static bool learning = false;
        

        long timer = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            grid = new Grid(13, 11);
            //grid = new Grid(47, 54); 


            intruder = new Intruder[NO_OF_INTRUDERS];
            shotSystem = new ShotSystem();
            for (int n = 0; n < NO_OF_INTRUDERS; n++)
                intruder[n] = new Intruder();
            guard = new Guard();
            guard.Init(ref grid);

            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferMultiSampling = true;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            
            Camera.centreOfScreen = new Vector2(1366 * 0.5f, 768 * 0.5f);
            Camera.pos = Camera.centreOfScreen;

            keyboard = Keyboard.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            grid.cellTexture = Content.Load<Texture2D>("cell");
            grid.blockTexture = Content.Load<Texture2D>("block");
            grid.safeArrow = Content.Load<Texture2D>("bluearrow");
            grid.shortArrow = Content.Load<Texture2D>("redarrow");
            cursor = Content.Load<Texture2D>("cursor");
            Intruder.texture = Content.Load<Texture2D>("intruder");
            shotSystem.texture = Content.Load<Texture2D>("shot");
            Tower.texture = Content.Load<Texture2D>("tower");
            Guard.texture = Content.Load<Texture2D>("guard");
        }

        protected override void UnloadContent()
        {
            
        }

        MouseState oldMouse;
        KeyboardState keyboard, oldKeyboard;

        protected override void Update(GameTime gameTime)
        {
            oldKeyboard = keyboard;
            keyboard = Keyboard.GetState();
            oldMouse = mouse;
            mouse = Mouse.GetState();
            mousePos.X = mouse.X;
            mousePos.Y = mouse.Y;


            // Allows the game to exit
            if (keyboard.IsKeyDown(Keys.Escape))
                this.Exit();
            if (keyboard.IsKeyDown(Keys.P) && !oldKeyboard.IsKeyDown(Keys.P))
                paused = !paused;
            if (keyboard.IsKeyDown(Keys.D) && !oldKeyboard.IsKeyDown(Keys.D))
                devMode = !devMode;
            if (keyboard.IsKeyDown(Keys.G) && !oldKeyboard.IsKeyDown(Keys.G))
                guard.alive = !guard.alive;
            if (keyboard.IsKeyDown(Keys.D1) && !oldKeyboard.IsKeyDown(Keys.D1))
                guard.SwitchPathing();
            if (keyboard.IsKeyDown(Keys.D2) && !oldKeyboard.IsKeyDown(Keys.D2))
                guard.SwitchStrategy();
            if (keyboard.IsKeyDown(Keys.I) && !oldKeyboard.IsKeyDown(Keys.I))
                smartIntruders = !smartIntruders;
            if (keyboard.IsKeyDown(Keys.L) && !oldKeyboard.IsKeyDown(Keys.L))
                learning = !learning;


            if (timer % 180 == 0)
                intruder[(timer / 120) % NO_OF_INTRUDERS].Init(ref grid, smartIntruders);



            if (mouse.ScrollWheelValue > oldMouse.ScrollWheelValue || keyboard.IsKeyDown(Keys.X))
                Camera.ZoomIn();
            else if (mouse.ScrollWheelValue < oldMouse.ScrollWheelValue || keyboard.IsKeyDown(Keys.Z))
                Camera.ZoomOut();

            if (keyboard.IsKeyDown(Keys.Up)) Camera.Scroll(-Vector2.UnitY);
            if (keyboard.IsKeyDown(Keys.Down)) Camera.Scroll(Vector2.UnitY);
            if (keyboard.IsKeyDown(Keys.Left)) Camera.Scroll(-Vector2.UnitX);
            if (keyboard.IsKeyDown(Keys.Right)) Camera.Scroll(Vector2.UnitX);

            Camera.Update();

            grid.Update(mouse, oldMouse, ref intruder, ref shotSystem);
            //tower.Update(ref intruder, ref shotSystem);
            if (!paused)
            {
                shotSystem.Update(ref intruder);

                for (int n = 0; n < NO_OF_INTRUDERS; n++)
                    if (intruder[n].alive)
                        intruder[n].Update(ref grid);
                if (guard.alive)
                    guard.Update(ref grid, intruder, ref shotSystem);

                timer++;
            }

            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            grid.Draw(ref spriteBatch);

            if (guard.alive)
                guard.Draw(ref spriteBatch);

            for (int n = 0; n < 50; n++)
                if (intruder[n].alive)
                    intruder[n].Draw(ref spriteBatch);
        
            grid.DrawTowers(ref spriteBatch);
            shotSystem.Draw(ref spriteBatch);

            spriteBatch.Draw(cursor, mousePos, Color.White);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
