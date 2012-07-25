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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Test : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Scenario2DPainter scenario2DPainter;
        Texture2D texture1;
        Texture2D texture2;

        Scenario2D scenario;

        public Test()
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
            // TODO: Add your initialization logic here


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            scenario = new Scenario2D(this);
            scenario2DPainter = new Scenario2DPainter(GraphicsDevice, scenario);

            scenario.CameraFacing = new Vector2(0, 1);
            scenario.CameraPosition = new Vector2(0, 0);
            scenario.ViewHeight = 300;
            scenario.ViewWidth = 400;

            texture1 = Content.Load<Texture2D>("field");
            texture2 = Content.Load<Texture2D>("cloud");


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.A))
                scenario.CameraPosition = new Vector2(scenario.CameraPosition.X - 1, scenario.CameraPosition.Y);
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.D))
                scenario.CameraPosition = new Vector2(scenario.CameraPosition.X + 1, scenario.CameraPosition.Y);
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.W))
                scenario.CameraPosition = new Vector2(scenario.CameraPosition.X, scenario.CameraPosition.Y - 1);
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.S))
                scenario.CameraPosition = new Vector2(scenario.CameraPosition.X, scenario.CameraPosition.Y + 1);
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Up))
            {
                scenario.ViewHeight = ((scenario.ViewHeight)*0.99f);
                scenario.ViewWidth = ((scenario.ViewWidth)*0.99f);         
            }
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Down))
            {
                scenario.ViewHeight = ((scenario.ViewHeight) * 1.01f);
                scenario.ViewWidth = ((scenario.ViewWidth) * 1.01f);   
            }


            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            scenario2DPainter.Begin();
            scenario2DPainter.DrawInScenario(texture1, new Vector2(0, 0), Color.White);
            scenario2DPainter.DrawInScenario(texture2, new Vector2(100, 100), Color.White);
            scenario2DPainter.End();

            base.Draw(gameTime);
        }
    }

