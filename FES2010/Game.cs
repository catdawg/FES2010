/*
    A football made for a game development course in the Faculty of Engineering from the University of Porto, Portugal
    Copyright (C) 2010  João Xavier, Ricardo Moutinho, Rui Campos, Tiago Carvalho

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Collections;
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
using GameMenu;


namespace FES2010
{
    // enums go here or in an extra file
    // match related
    public enum CameraType { top }
    public enum Difficulty { none, normal }
    public enum Weather { sunny, rainy }

    // player related
    public enum TacticalPosition { goalkeeper, defender, dfmidfielder, midfielder, ofmidfielder, striker }
    public enum Side { left, center, right }

    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ArrayList teams;

        // match settings
        public Match Match{get;set;}
        
        //Field field;
        
        int matchDuration;
        Difficulty matchDifficulty;
        CameraType matchCamera;
        Weather matchWeather;


        //Scenario2D scenario;
        //Scenario2DPainter scenario2DPainter;
        

        Menu mainMenu;
        Texture2D credits, splash, splashSpace, disclaimerTexture;
        SoundEffect soundChoice, soundSelected;
        bool showCredits, showSplash;
        bool animSplash;
        int splashTimer;
        bool disclaimer = true;
        int disclaimerTimer = 7000;

        public KeyboardState CurrentKeyboardState { get; set; }
        public KeyboardState PrevKeyboardState { get; set; }
        public Controller Cmd1 { get; set; }
        public Controller Cmd2 { get; set; }

        // Control Type
        //bool gamepadsUsed = false;

        // Game Start
        public bool gameStarted = false;

        // Screen Resolution
        public int screenWidth = 800;
        public int screenHeight = 600;
        bool screenFull = false;

        


        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            showSplash = true;
            animSplash = false;
            splashTimer = 0;

            //DefaultMatchInfo();
            InitGraphicsMode(screenWidth, screenHeight, screenFull);
            this.Cmd1 = new Controller();
            this.Cmd2 = new Controller(2);
        }

        protected override void Initialize()
        {
            Match = null;
            teams = new ArrayList();
            new TeamsParser("Teams.txt").Parse(this, teams /*, scenario2DPainter */);

            matchCamera = CameraType.top;
            matchDifficulty = Difficulty.normal;
            matchWeather = Weather.sunny;
            matchDuration = 5 * 60;
            SetSound(1.0f, 0.3f);
            showCredits = false;
            base.Initialize();
        }

        public void Reinitialize()
        {
            Match.UnloadContent();
            Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            credits = Content.Load<Texture2D>("credits");
            splash = Content.Load<Texture2D>("splash");
            splashSpace = Content.Load<Texture2D>("splashspace");
            disclaimerTexture = Content.Load<Texture2D>("disclaimer");
            soundChoice = Content.Load<SoundEffect>("choice");
            soundSelected = Content.Load<SoundEffect>("select");

           // new TeamsParser("Teams.txt").Parse(this, teams /*, scenario2DPainter */); 
            

            mainMenu = new GameMenu.Menu(this);
            mainMenu.textColor = Color.White;
            mainMenu.selectColor = Color.Orange;
            mainMenu.visible = true;

            MenuChoice choice = mainMenu.AddChoice("Play");
            choice.AddChoice("Player vs Player");
            choice.AddChoice("Player vs Computer");

            // match settings
            choice = choice.AddChoice("Settings");
            MenuChoice dur = choice.AddChoice("Full duration (minutes) ");
            dur.AddLeftRightChoices(new string[] { "2", "5", "10", "20" });
            MenuChoice cam = choice.AddChoice("Camera ");
            cam.AddLeftRightChoices(new string[] { "Top" });
            MenuChoice wea = choice.AddChoice("Weather ");
            wea.AddLeftRightChoices(new string[] { "Sunny", "Rainy" });
            MenuChoice dif = choice.AddChoice("Difficulty ");
            dif.AddLeftRightChoices(new string[] { "Normal" });

            // game settings
            choice = mainMenu.AddChoice("Settings");
            MenuChoice scr = choice.AddChoice("Screen ");
            scr.AddChoice("640x480");
            scr.AddChoice("800x600");
            scr.AddChoice("1024x768");
            choice.AddChoice("Toggle Fullscreen");
            MenuChoice sou = choice.AddChoice("Sound ");
            sou.AddLeftRightChoices(new string[] { "On", "Off" });
            MenuChoice cmds = choice.AddChoice("Controls");
            //cmds.AddLeftRightChoices(new string[] { "Player 1", "Player 2" });
            
            
            MenuChoice pl1 = cmds.AddChoice("Player 1");
            pl1.AddChoice("Buy the full version!");
            MenuChoice pl2 = cmds.AddChoice("Player 2");
            pl2.AddChoice("Buy the full version!");


            choice = mainMenu.AddChoice("Credits");

            mainMenu.ChoiceExecuted += new Menu.ChoiceExecutedHandler(ChoiceExecuted);
            mainMenu.ChoiceSelected += new Menu.ChoiceSelectedHandler(ChoiceSelected);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            
        }

        public void ChoiceExecuted(object source, Menu.MenuEvent e)
        {
            soundSelected.Play();

            switch (e.choiceString)
            {
                case "Return":
                    mainMenu.visible = false; break;

                // game type
                case "Player vs Player":
                    matchDifficulty = Difficulty.none;
                    gameStarted = true;

                    Match = new Match(this, (Team)teams[0], (Team)teams[1], matchCamera, matchDifficulty, matchWeather, matchDuration, false /*, scenario, scenario2DPainter, field */);
                    
                    Match.Initialize();
                    Match.LoadContent();
                    
                    break;
                case "Player vs Computer":
                    gameStarted = true;

                    Match = new Match(this, (Team)teams[0], (Team)teams[1], matchCamera, matchDifficulty, matchWeather, matchDuration, true /*, scenario, scenario2DPainter, field */);
                    
                    Match.Initialize();
                    Match.LoadContent();

                    break;

                case "Credits":
                    showCredits = true; break;

                // screen resolution
                case "Toggle Fullscreen":
                    screenFull = !screenFull;
                    InitGraphicsMode(screenWidth, screenHeight, screenFull); break;
                case "640x480":
                    screenWidth = 640;
                    screenHeight = 480;
                    InitGraphicsMode(screenWidth, screenHeight, screenFull); break;
                case "800x600":
                    screenWidth = 800;
                    screenHeight = 600;
                    InitGraphicsMode(screenWidth, screenHeight, screenFull); break;
                case "1024x768":
                    screenWidth = 1024;
                    screenHeight = 768;
                    InitGraphicsMode(screenWidth, screenHeight, screenFull); break;
                }
        }

        public void ChoiceSelected(object source, Menu.MenuEvent e)
        {
            soundChoice.Play();

            switch (e.choiceString)
            {
                // game settings
                case "Sunny":
                    matchWeather = Weather.sunny; break;
                case "Rainy":
                    matchWeather = Weather.rainy; break;
                case "Top":
                    matchCamera = CameraType.top; break;
                case "Normal":
                    matchDifficulty = Difficulty.normal; break;
                case "2":
                case "5":
                case "10":
                case "20":
                    matchDuration = Convert.ToInt32(e.choiceString) * 60;
                   ; break;
                // sound settings
                case "On":
                    SetSound(1.0f, 0.3f); break;
                case "Off":
                    SetSound(0.0f, 0.0f); break;
            }
        }

        
        protected override void Update(GameTime gameTime)
        {
            CurrentKeyboardState = Keyboard.GetState();

            if (CurrentKeyboardState.IsKeyDown(Keys.LeftAlt) && CurrentKeyboardState.IsKeyDown(Keys.Enter) &&
                !PrevKeyboardState.IsKeyDown(Keys.Enter))
            {
                screenFull = !screenFull;
                InitGraphicsMode(screenWidth, screenHeight, screenFull);
            }

            if (disclaimer)
            {
                disclaimerTimer -= gameTime.ElapsedGameTime.Milliseconds;
                if (disclaimerTimer < 0)
                    disclaimer = false;
                if (CurrentKeyboardState.IsKeyDown(Keys.Space) || CurrentKeyboardState.IsKeyDown(Keys.Enter))
                    disclaimer = false;
            }
            else if (showSplash)
            {
                splashTimer += gameTime.ElapsedGameTime.Milliseconds;

                if (splashTimer > 500)
                {
                    animSplash = !animSplash;
                    splashTimer -= 500;
                }

                if (CurrentKeyboardState.IsKeyDown(Keys.Space) && PrevKeyboardState.IsKeyUp(Keys.Space))
                    showSplash = false;
            }
            else
            {
                if (showCredits)
                {
                    if (CurrentKeyboardState.GetPressedKeys().Length > 0 && !PrevKeyboardState.IsKeyDown(Keys.Enter))
                        showCredits = false;
                }

                if (showSplash)
                {
                    splashTimer += gameTime.ElapsedGameTime.Milliseconds;

                    if (splashTimer > 500)
                    {
                        animSplash = !animSplash;
                        splashTimer -= 500;
                    }

                    if (CurrentKeyboardState.IsKeyDown(Keys.Space) && PrevKeyboardState.IsKeyUp(Keys.Space))
                        showSplash = false;
                }

                if (!gameStarted)
                {
                    if (CurrentKeyboardState.IsKeyDown(Keys.Escape) && !PrevKeyboardState.IsKeyDown(Keys.Escape))
                    {
                        mainMenu.visible = !mainMenu.visible;
                    }

                    // exit case, esc pressed when menu is showing
                    if (!mainMenu.visible)
                        this.Exit();

                    mainMenu.Update(gameTime);
                }
                else
                {
                    if (CurrentKeyboardState.IsKeyDown(Keys.Escape) && !PrevKeyboardState.IsKeyDown(Keys.Escape))
                    {
                        gameStarted = false;
                        mainMenu.visible = true;
                        MediaPlayer.Stop();
                        Reinitialize();
                    }
                    else
                    {
                        graphics.GraphicsDevice.Clear(Color.Gray);
                        Match.Update(gameTime);
                    }
                }
            }

            base.Update(gameTime);


            PrevKeyboardState = CurrentKeyboardState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (disclaimer)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(disclaimerTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                spriteBatch.End();
            }
            else if (showSplash)
            {
                spriteBatch.Begin();

                if (animSplash)
                    spriteBatch.Draw(splashSpace, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                else
                    spriteBatch.Draw(splash, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                
                spriteBatch.End();
            }
            else if (showCredits)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(credits, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                spriteBatch.End();
            }
            else
            {
                if (!gameStarted)
                {
                    graphics.GraphicsDevice.Clear(Color.Black);

                    mainMenu.Draw(gameTime);
                }
                else Match.Draw(gameTime);
            }

            base.Draw(gameTime);
        }


        private void SetSound(float sfxVol, float musicVol)
        {
            SoundEffect.MasterVolume = sfxVol;
            MediaPlayer.Volume = musicVol;
        }

        private bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    graphics.PreferredBackBufferWidth = iWidth;
                    graphics.PreferredBackBufferHeight = iHeight;
                    graphics.IsFullScreen = bFullScreen;
                    graphics.ApplyChanges();
                    Initialize();
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        graphics.PreferredBackBufferWidth = iWidth;
                        graphics.PreferredBackBufferHeight = iHeight;
                        graphics.IsFullScreen = bFullScreen;
                        graphics.ApplyChanges();
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
