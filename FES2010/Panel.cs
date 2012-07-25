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


namespace FES2010
{

    public class Panel : Microsoft.Xna.Framework.GameComponent
    {
        SpriteFont playerFont, stateFont, numberFont, teamFont;
        SpriteBatch spriteBatch;
        Scenario2DPainter scenario2DPainter;
        //textures
        Texture2D texture, staminaTexture;

        Rectangle rectangle;
        Rectangle runFatigue;
        float timeElapsed;
        Radar radar;
        float middleX;
        float middleY;
        float alignUp;
        float alignDown;
        float alignLeft;
        float alignRight;
       
        int fixedWidth = 150;
        int fixedWidthFatigue = 1;
        int stop = 45;
        public float Time { get; set; }
        public string Status { get; set; }
        public Color PanelColor { get; set; }
        public Color HomeColor { get; set; }
        public Color AwayColor { get; set; }
        public Panel(Game game)
            : base(game)
        {
            //homeScore  = awayScore = 0;
            timeElapsed = 0;
            middleX = Game.Window.ClientBounds.Width / 2;
            middleY = Game.Window.ClientBounds.Height / 2;
            alignUp = Game.Window.ClientBounds.Height * .05f;
            alignDown = Game.Window.ClientBounds.Height * .85f;
            alignLeft = Game.Window.ClientBounds.Width * .2f;
            alignRight = Game.Window.ClientBounds.Width * .8f;
            PanelColor = Color.Black;
            AwayColor = Color.DarkSlateBlue;
            HomeColor = new Color(140, 45, 25);
        }

        public Panel(Game game, Scenario2DPainter scenario2DPainter)
            : base(game)
        {
            middleX = Game.Window.ClientBounds.Width / 2;
            middleY = Game.Window.ClientBounds.Height / 2;
            alignUp = Game.Window.ClientBounds.Height * .05f;
            alignDown = Game.Window.ClientBounds.Height * .85f;
            alignLeft = Game.Window.ClientBounds.Width * .2f;
            alignRight = Game.Window.ClientBounds.Width * .8f;
            //homeScore = awayScore = 0;
            timeElapsed = 0;
            this.scenario2DPainter = scenario2DPainter;
            this.rectangle = new Rectangle(Game.Window.ClientBounds.Width / 20,
                                            (int)(Game.Window.ClientBounds.Height * 0.9), fixedWidth, 10);
            this.runFatigue = new Rectangle(Game.Window.ClientBounds.Width / 20,
                                            (int)(Game.Window.ClientBounds.Height * 0.9), fixedWidth, 10);
            PanelColor = Color.Black;
            AwayColor = Color.Red;
            HomeColor = Color.Blue;
            Time = 0.0f;
        }


        public override void Initialize()
        {
            PanelColor = Color.DarkBlue;
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            //timeElapsed += (float)gameTime.ElapsedRealTime.Milliseconds ;
            //Console.WriteLine("tempo de jogo: :"+timeElapsed);
            if (!((Game)Game).Match.Pause)
            {
                float second = (float)gameTime.ElapsedGameTime.Milliseconds/1000;
                timeElapsed += (float)(second*(90.0f / ((Game)Game).Match.Duration)); //(0.1f / 6)
            
                if (timeElapsed >= stop)
                {
                    if (stop < 90)  //halftime
                    {
                        //refill stamina from both teams to 3/4
                        ((Game)Game).Match.HomeTeam.RefillStamina();
                        ((Game)Game).Match.AwayTeam.RefillStamina();

                        if (((Game)Game).Match.KickOff)
                        {
                            if(((Game)Game).Match.Ball.CurrentPlayer != null)
                                ((Game)Game).Match.Ball.CurrentPlayer.Pass(gameTime);
                        }
                        if (((Game)Game).Match.Ball.FreeKick)
                        {
                            if (((Game)Game).Match.Ball.CurrentPlayer != null)
                                ((Game)Game).Match.Ball.CurrentPlayer.Pass(gameTime);
                            ((Game)Game).Match.Ball.FreeKick = false;
                            ((Game)Game).Match.Ball.Position = new Vector2(0);
                        }
                        if (((Game)Game).Match.Ball.CurrentPlayer != null)
                            ((Game)Game).Match.Ball.CurrentPlayer.HasBall = false;

                        ((Game)Game).Match.Ball.Reset();
                        ((Game)Game).Match.Scenario.CameraPosition = new Vector2(0);
                        ((Game)Game).Match.HomeTeamStart = false;
                        ((Game)Game).Match.KickOff = true;
                        ((Game)Game).Match.KickOffStart = true;
                        //reset the teams position
                        ((Game)Game).Match.HomeTeam.OrganizePlayers(true);
                        ((Game)Game).Match.AwayTeam.OrganizePlayers(true);
                        ((Game)Game).Match.setState("Half-Time", 3000);
                        stop = 90;
                    }
                    else
                    {
                        ((Game)Game).Match.setState("Full-Time", 3000);
                        ((Game)Game).Match.Ball.Reset();
                        ((Game)Game).Match.Scenario.CameraPosition = new Vector2(0);
                        //reset the teams position
                        ((Game)Game).Match.HomeTeam.OrganizePlayers(true);
                        ((Game)Game).Match.AwayTeam.OrganizePlayers(true);  
                    }
                }
            }
            radar.Update(gameTime);

            base.Update(gameTime);
        }

        public void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            playerFont = Game.Content.Load<SpriteFont>("PlayerFont");
            numberFont = Game.Content.Load<SpriteFont>("Number");
            stateFont = Game.Content.Load<SpriteFont>("State");
            teamFont = Game.Content.Load<SpriteFont>("Team");
            texture = Game.Content.Load<Texture2D>("staminaBar");
            staminaTexture = Game.Content.Load<Texture2D>("staminaBar2");
            radar = new Radar((Game)Game, spriteBatch, playerFont, texture);
            radar.LoadContent();
        }

        public void DrawString(String print, Color color, SpriteFont font, float alignX, float alignY, Vector2 textSize)
        {
            spriteBatch.DrawString(font,
                    print,
                    (new Vector2(alignX - 1.3f, alignY - 1.3f)) - textSize,
                    Color.Black,
                    0,
                    new Vector2(0, 0),
                    1.0f,
                    SpriteEffects.None,
                    1.0f);
            spriteBatch.DrawString(font,
                print,
                (new Vector2(alignX + 1.3f, alignY + 1.3f)) - textSize,
                Color.Black,
                0,
                new Vector2(0, 0),
                1.0f,
                SpriteEffects.None,
                1.0f);
            spriteBatch.DrawString(font,
                print,
                (new Vector2(alignX + 1.3f, alignY - 1.3f)) - textSize,
                Color.Black,
                0,
                new Vector2(0, 0),
                1.0f,
                SpriteEffects.None,
                1.0f);
            spriteBatch.DrawString(font,
                print,
                (new Vector2(alignX - 1.3f, alignY + 1.3f)) - textSize,
                Color.Black,
                0,
                new Vector2(0, 0),
                1.0f,
                SpriteEffects.None,
                1.0f);

            spriteBatch.DrawString(font,
                print,
                (new Vector2(alignX, alignY)) - textSize,
                color,
                0,
                new Vector2(0, 0),
                1.0f,
                SpriteEffects.None,
                1.0f);
        }

        public void Draw(GameTime gameTime)
        {
            Team homeTeam = ((Game)Game).Match.HomeTeam;
            Team awayTeam = ((Game)Game).Match.AwayTeam;
            Vector2 textSize;
            
            spriteBatch.Begin();
            /*** TIME ***/
            textSize = numberFont.MeasureString(Convert.ToString((int)(timeElapsed)));
            DrawString(Convert.ToString((int)(timeElapsed)), Color.AntiqueWhite, numberFont, middleX, alignUp, textSize / 2);

            if (Status != null)
            {
                textSize = stateFont.MeasureString(Status);
                DrawString(Status, PanelColor, stateFont, middleX, middleY, textSize / 2);
            }
            else PanelColor = Color.DarkBlue;
            /*** HOME TEAM ***/
            textSize = teamFont.MeasureString(homeTeam.Name);
            DrawString(homeTeam.Name, HomeColor, teamFont, alignLeft, alignUp, textSize / 2);

            textSize = numberFont.MeasureString(Convert.ToString(((Game)Game).Match.HomeScore));
            DrawString(Convert.ToString(((Game)Game).Match.HomeScore), HomeColor, numberFont, alignLeft, alignUp * 2, textSize / 2);

            if ((((Game)Game).Match.Player1) != null)
            {
                float alignPlayer = alignLeft * 0.3f;
                textSize = new Vector2(0, 0);
                DrawString((((Game)Game).Match.Player1).Name, HomeColor, playerFont, alignPlayer, alignDown, textSize);
                //global fatigue
                rectangle.X = (int)alignPlayer;
                rectangle.Y = (int)(alignDown*1.07f); //Game.Window.ClientBounds.Height * 0.8f) + 50;
                rectangle.Width = (int)(fixedWidth * (float)(((Game)Game).Match.Player1.TopSpeedFactor - 1));
                rectangle.Height = 10;
                if (rectangle.Width < fixedWidth * 0.15)
                    spriteBatch.Draw(staminaTexture, rectangle, new Color(Color.Red,160));
                else if (rectangle.Width < fixedWidth * 0.4)
                    spriteBatch.Draw(staminaTexture, rectangle, new Color(Color.Yellow,160));
                else spriteBatch.Draw(staminaTexture, rectangle, new Color(Color.Green, 160));

                //current run fatigue
                runFatigue.X = (int)alignPlayer;
                runFatigue.Y = (int)(alignDown * 1.1f);//(int)((float)Game.Window.ClientBounds.Height * 0.8f) + 30;
                runFatigue.Width = (int)(fixedWidthFatigue * (float)(((Game)Game).Match.Player1.TopSpeed - ((Game)Game).Match.Player1.WalkingSpeed));
                runFatigue.Height = 10;
                if (runFatigue.Width < fixedWidthFatigue * 20)
                    spriteBatch.Draw(texture, runFatigue, new Color(Color.Red, 160));
                else if (runFatigue.Width < fixedWidthFatigue * 50)
                    spriteBatch.Draw(texture, runFatigue, new Color(Color.Yellow, 160));
                else spriteBatch.Draw(texture, runFatigue, new Color(Color.Green, 160));
            }
            /*** AWAY TEAM ***/
            textSize = teamFont.MeasureString(awayTeam.Name);
            DrawString(awayTeam.Name, AwayColor, teamFont, alignRight, alignUp, textSize / 2);

            textSize = numberFont.MeasureString(Convert.ToString(((Game)Game).Match.AwayScore));
            DrawString(Convert.ToString(((Game)Game).Match.AwayScore), AwayColor, numberFont, alignRight, alignUp * 2, textSize / 2);

            if ((((Game)Game).Match.Player2) != null)
            {
                
                float textSizeX = (playerFont.MeasureString(Convert.ToString(((Game)Game).Match.Player2.Name))).X;
                DrawString((((Game)Game).Match.Player2).Name, AwayColor, playerFont, alignRight - textSizeX / 2, alignDown, new Vector2(0, 0));

                //global fatigue
                rectangle.X = (int)(Game.Window.ClientBounds.Width * 0.7f);
                rectangle.Y = (int)(alignDown*1.07f);
                rectangle.Width = (int)(fixedWidth * (float)(((Game)Game).Match.Player2.TopSpeedFactor - 1));
                rectangle.Height = 10;
                if (rectangle.Width < fixedWidth * 0.15)
                    spriteBatch.Draw(staminaTexture, rectangle, new Color(Color.Red, 160));
                else if(rectangle.Width < fixedWidth*0.4)
                    spriteBatch.Draw(staminaTexture, rectangle, new Color(Color.Yellow, 160));
                else spriteBatch.Draw(staminaTexture, rectangle, new Color(Color.Green, 160));

                //current run fatigue
                runFatigue.X = (int)(Game.Window.ClientBounds.Width * 0.7f);
                runFatigue.Y = (int)(alignDown * 1.1f);
                runFatigue.Width = (int)(fixedWidthFatigue * (float)(((Game)Game).Match.Player2.TopSpeed - ((Game)Game).Match.Player2.WalkingSpeed));
                runFatigue.Height = 10;
                if (runFatigue.Width < fixedWidthFatigue * 20)
                    spriteBatch.Draw(texture, runFatigue, new Color(Color.Red, 160));
                else if (runFatigue.Width < fixedWidthFatigue * 50)
                    spriteBatch.Draw(texture, runFatigue, new Color(Color.Yellow, 160));
                else spriteBatch.Draw(texture, runFatigue, new Color(Color.Green, 160));
            }

            radar.Draw(gameTime);


            spriteBatch.End();
        }
    }
}
