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
    
    public class Match : Microsoft.Xna.Framework.GameComponent
    {
        // Match settings
        public CameraType CameraType { get; set; }
        public Difficulty Difficulty { get; set; }
        public Weather Weather { get; set; }
        public int Duration { get; set; }

        public Panel Panel { get; set; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public Ball Ball { get; set; }
        public Field Field { get; set; }
        public Song[] Audience { get; set; }
        SoundEffect whistle;
        public SoundEffect PassSound { get; set; }
        public SoundEffect PostSound { get; set; }
        public SoundEffect NetSound { get; set; }
        public SoundEffect SaveOh { get; set; }
        public SoundEffect Goal { get; set; }
        public SoundEffect Humiliation { get; set; }
        public SoundEffect Unstoppable { get; set; }

        //current player for each team
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        //AI
        public bool Player2Cpu { get; set; }

        public int Player1Index { get; set; }
        public int Player2Index { get; set; }

        public Scenario2D Scenario { get; set; }
        public Scenario2DPainter Scenario2DPainter { get; set; }
        public Boolean Pause { get; set; }
        public Boolean hold;
        public Boolean DisplayRadar { get; set; }
        public Boolean KickOff { get; set; }
        public Boolean KickOffStart { get; set; }
        public Boolean HomeTeamStart { get; set; } //if true it's the homeTeam who's about to kick off
        public Boolean FreeKickStart { get; set; }
        public Team TmpTeam { get; set; }

        //camera border limits variables
        bool leftBorder = false, rightBorder = false, topBorder = false, bottomBorder = false;

        //Player passing/shoting constants
        public float MaxPassSpeed { get; set; }   //100 shot player's pass speed
        public float MaxErrorAngle { get; set; }   //0 offense player's max error angle
        public float PerfectPassFactor { get; set; }    //extra distance factor (adds speed to passes)
        public float timer {get; set;}
        float timeLeft = 0.0f;

        //Random seed
        public Random Rand { get; set; }

        public Match(Game game)
            : base(game)
        {
        }

        public Match(Game game, Team homeTeam, Team awayTeam, CameraType cameraType, Difficulty difficulty, Weather weather, int duration, bool player2Cpu)
            : base(game)
        {
            CameraType = cameraType;
            Difficulty = difficulty;
            Weather = weather;
            Duration = duration;

            HomeTeam = homeTeam;
            AwayTeam = awayTeam;
            AwayTeam.IsHomeTeam = false;
            Player1Index = 0;
            Player2Index = 0;
            Player2Cpu = player2Cpu;

            Scenario = new Scenario2D(Game);
            Scenario2DPainter = new Scenario2DPainter(Game.GraphicsDevice, Scenario);
            timer = -1;
            Pause = false;
            DisplayRadar = true;
            hold = true;
            KickOff = true;
            KickOffStart = true;
            HomeTeamStart = true;
            FreeKickStart = false;
            TmpTeam = null;

            Ball = new Ball(game, Weather);
            Field = new Field(game);

            Panel = new Panel(game);
            //passing
            MaxPassSpeed = 800;
            MaxErrorAngle = 15;
            PerfectPassFactor = 2.8f;
            Rand = new Random();
            Audience = new Song[3];
        }

        public override void Initialize()
        {
            //depends on cameratype, not definitive
            Scenario.ViewWidth = 1024;
            Scenario.ViewHeight = 768;
            Scenario.CameraPosition = new Vector2(0, 0);
            Panel.Initialize();
            base.Initialize();
        }

        public void LoadContent()
        {
            Field.LoadContent();
            Ball.LoadContent();
            HomeTeam.LoadContent();
            AwayTeam.LoadContent();

            Audience[0] = ((Game)Game).Content.Load<Song>("audience1");
            Audience[1] = ((Game)Game).Content.Load<Song>("audience2");
            Audience[2] = ((Game)Game).Content.Load<Song>("audience3");
            whistle = ((Game)Game).Content.Load<SoundEffect>("whistleshort");
            PassSound = ((Game)Game).Content.Load<SoundEffect>("pass");
            PostSound = ((Game)Game).Content.Load<SoundEffect>("post");
            NetSound = ((Game)Game).Content.Load<SoundEffect>("nets");
            SaveOh = ((Game)Game).Content.Load<SoundEffect>("gkSaveOh");
            Goal = ((Game)Game).Content.Load<SoundEffect>("goal");
            Humiliation = ((Game)Game).Content.Load<SoundEffect>("humiliation");
            Unstoppable = ((Game)Game).Content.Load<SoundEffect>("unstoppable");

            MediaPlayer.Play(Audience[Rand.Next(0, 3)]);
            MediaPlayer.IsRepeating = true;

            whistle.Play();

            //select the closest player to the ball to start
            Player1 = Ball.GetClosestPlayer(HomeTeam);
            Player2 = Ball.GetClosestPlayer(AwayTeam);

            Panel.LoadContent();
            Player1.LoadContent();
            Player2.LoadContent();
        }

        public void UnloadContent()
        {
            Ball.CurrentPlayer = null;
            Ball = null;
            Player1 = null;
            Player2 = null;
            Field = null;
            Panel = null;
            HomeTeam.UnloadContent();
            HomeTeam = null;
            AwayTeam.UnloadContent();
            AwayTeam = null;
            TmpTeam = null;
        }

        public override void Update(GameTime gameTime)
        {
            if (timer > 0)
                if (timeLeft > timer)
                {
                    Pause = false;
                    timer = timeLeft = 0;

                    if (Panel.Status == "Full-Time" || Panel.Status == "Humiliation" || Panel.Status == "Unstoppable")
                    {
                        MediaPlayer.Stop();

                        ((Game)Game).gameStarted = false;
                        ((Game)Game).Reinitialize();
                        return;
                    }
                    else
                        Panel.Status = null;
                }
                else timeLeft += gameTime.ElapsedGameTime.Milliseconds;
                
            if (((Game)Game).CurrentKeyboardState.IsKeyDown(Keys.R) && ((Game)Game).PrevKeyboardState.IsKeyUp(Keys.R))
                DisplayRadar = !DisplayRadar;
            if(Keyboard.GetState().IsKeyDown(Keys.Enter) && !hold)
            {
                Pause = !Pause;
                hold = true;
                if (Pause)
                {
                    MediaPlayer.Pause();
                    ((Game)Game).Match.Panel.Status = "Paused";
                }
                else
                {
                    MediaPlayer.Resume();
                    ((Game)Game).Match.Panel.Status = "";
                }
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Enter) && hold)
                hold = false;

            if (!Pause)
            {
                if (!KickOff && !Ball.FreeKick)
                {
                    Player1.CheckControls(gameTime);//player1
                    if(!Player2Cpu)
                        Player2.CheckControls(gameTime);//player2
                }
                PlayerChanges();

                HandleOutOfPlay();

                // * CAMERA *
                ChangeCameraHeight();

                //check which borders are the camera currently being limited by
                CheckBorders();

                //make the camera follow the ball
                FollowBall();
                // * *

                if (KickOff)
                {
                    if (HomeTeamStart)
                        Player1.CheckControls(gameTime);//player1
                    else if (!Player2Cpu)
                        Player2.CheckControls(gameTime);//player2
                     
                    //reset the teams position
                    if (KickOffStart)
                    {
                        HomeTeam.OrganizePlayers(true);
                        AwayTeam.OrganizePlayers(true);
                        HomeTeam.Update(gameTime);
                        AwayTeam.Update(gameTime);
                        KickOffStart = false;
                        Ball.Reset();
                    }
                    Player starter, starter2;
                    if (HomeTeamStart)
                    {
                        starter = HomeTeam.GetClosestPlayer(Ball.Position, null);
                        starter2 = HomeTeam.GetClosestPlayer(Ball.Position, starter);
                    }
                    else
                    {
                        starter = AwayTeam.GetClosestPlayer(Ball.Position, null);
                        starter2 = AwayTeam.GetClosestPlayer(Ball.Position, starter);
                        if (Player2Cpu)
                        {
                            starter.Update(gameTime);
                            starter2.Update(gameTime);
                            if (!HomeTeamStart)
                            {
                                if (starter.HasBall && (int)starter2.Position.Y == 0)
                                {
                                    starter.Pass(gameTime);
                                }
                            }
                        }
                    }
                    starter.GoTo(new Vector2(-15, 0), false, true, false);
                    starter2.GoTo(new Vector2(45, 0), false, true, false);
                    if (!Player2Cpu || (HomeTeamStart && Player2Cpu))
                    {
                        starter.Update(gameTime);
                        starter2.Update(gameTime);
                    }
                    if (Ball.Pass || Ball.Shot)
                    {
                        KickOff = false;
                    }
                }
                else if (Ball.FreeKick)
                {
                    if (FreeKickStart)
                    {
                        if (Ball.CurrentPlayer != null)
                        {
                            if (Ball.CurrentPlayer.Team == Player1.Team)
                                TmpTeam = Player2.Team;
                            else
                                TmpTeam = Player1.Team;
                            Ball.CurrentPlayer.Team.Update(gameTime);
                            FreeKickStart = false;
                        }
                    }
                    if(TmpTeam == Player1.Team)
                        Player1.CheckControls(gameTime);//player1
                    else if (!Player2Cpu) Player2.CheckControls(gameTime);//player2

                    if(TmpTeam != null)
                        TmpTeam.Update(gameTime);
                }
                else
                {
                    HomeTeam.ResetFreeKick();
                    AwayTeam.ResetFreeKick();
                    //update all the players
                    HomeTeam.Update(gameTime);
                    AwayTeam.Update(gameTime);
                }
                Ball.Update(gameTime);

            }
            if (timer < 0)
            {
                Pause = true;
                timer = 2000;
                Panel.Status = "Kick Off";
            }
            Panel.Update(gameTime);
            base.Update(gameTime);
        }

        private void HandleOutOfPlay()
        {
            if (Ball.OutOfPlay())
            {
                    if (Player1.HasBall)
                        Player1.HasBall = false;
                    else if (Player2.HasBall)
                        Player2.HasBall = false;

                    //select the closest player to the ball to start
                    if (!Player1.TacticalPosition.Equals(TacticalPosition.goalkeeper))
                        Player1 = Ball.GetClosestPlayer(HomeTeam);
                    if (!Player2.TacticalPosition.Equals(TacticalPosition.goalkeeper))
                        Player2 = Ball.GetClosestPlayer(AwayTeam);
            }
        }

        public void Draw(GameTime gameTime)
        {
            Scenario2DPainter.Begin();
            Field.Draw(gameTime);
            Player1.Draw(gameTime);
            HomeTeam.Draw(gameTime);
            AwayTeam.Draw(gameTime);
            Ball.Draw(gameTime);
            Scenario2DPainter.End();

            Panel.Draw(gameTime);
        }

        public void CheckBorders()
        {
            int margin = 150 * (1 + (int)Scenario.CameraFactor * (int)Scenario.ViewWidth);
            if (Ball.Position.X < -(Field.Width * Field.Scale / 2) + Scenario.ViewWidth / 2 - margin)  //make sure it stops at the left border of the pitch
                leftBorder = true;
            else leftBorder = false;
            if (Ball.Position.X > (Field.Width * Field.Scale / 2) - Scenario.ViewWidth / 2 + margin)  //make sure it stops at the right border of the pitch
                rightBorder = true;
            else rightBorder = false;
            if (Ball.Position.Y < -(Field.Height * Field.Scale / 2) + Scenario.ViewHeight / 2 - margin)  //make sure it stops at the top of the pitch
                topBorder = true;
            else topBorder = false;
            if (Ball.Position.Y > (Field.Height * Field.Scale / 2) - Scenario.ViewHeight / 2 + margin)  //make sure it stops at the bottom of the pitch
                bottomBorder = true;
            else bottomBorder = false;
        }

        public void ChangeCameraHeight(){
            if (Keyboard.GetState().IsKeyDown(Keys.Add))
            {
                Scenario.ViewHeight *= 0.98f;
                Scenario.ViewWidth *= 0.98f;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Subtract))
            {
                Scenario.ViewHeight *= 1.02f;
                Scenario.ViewWidth *= 1.02f;
            }
            FollowBall();
        }

        //changes player control to the team's player which is closer to the ball
        public void PlayerChanges()
        {
            Player tmp;
            if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.ChangePlayer) && ((Game)Game).PrevKeyboardState.IsKeyUp(((Game)Game).Cmd1.ChangePlayer) && !Player1.HasBall)  //player 1
            {
                if ((((Game)Game).Match.HomeTeam.GetOnScreenPlayers(2)).Count != 0)
                {
                    if (Player1Index >= (((Game)Game).Match.HomeTeam.GetOnScreenPlayers(2)).Count)
                        Player1Index = 0;
                    if ((tmp = (Player)(((Game)Game).Match.HomeTeam.GetOnScreenPlayers(2))[Player1Index]) != Player1)
                        Player1 = tmp;
                    else
                    {
                        Player1Index++;
                        if ((((Game)Game).Match.HomeTeam.GetOnScreenPlayers(2)).Count == Player1Index)
                            Player1Index = 0;
                        Player1 = (Player)(((Game)Game).Match.HomeTeam.GetOnScreenPlayers(2))[Player1Index];
                    }
                }
                else
                {
                    Player1 = ((Game)Game).Match.Ball.GetClosestPlayer(HomeTeam);
                }
            }
            if (!Player2Cpu)
            {
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.ChangePlayer) && ((Game)Game).PrevKeyboardState.IsKeyUp(((Game)Game).Cmd2.ChangePlayer) && !Player2.HasBall)  //player 2
                {
                    if ((((Game)Game).Match.AwayTeam.GetOnScreenPlayers(2)).Count != 0)
                    {
                        if (Player2Index >= (((Game)Game).Match.AwayTeam.GetOnScreenPlayers(2)).Count)
                            Player2Index = 0;
                        if ((tmp = (Player)(((Game)Game).Match.AwayTeam.GetOnScreenPlayers(2))[Player2Index]) != Player2)
                            Player2 = tmp;
                        else
                        {
                            Player2Index++;
                            if ((((Game)Game).Match.AwayTeam.GetOnScreenPlayers(2)).Count == Player2Index)
                                Player2Index = 0;
                            Player2 = (Player)(((Game)Game).Match.AwayTeam.GetOnScreenPlayers(2))[Player2Index];
                        }
                    }
                    else
                    {
                        Player2 = ((Game)Game).Match.Ball.GetClosestPlayer(AwayTeam);
                    }
                }
            }
        }

        public void FollowBall()
        {
            if (!topBorder && !bottomBorder)
            {
                if(Ball.Position.Y - Scenario.CameraPosition.Y > Scenario.ViewHeight*Scenario.CameraFactor)  //down
                    Scenario.CameraPosition.Y = Ball.Position.Y - Scenario.ViewHeight * Scenario.CameraFactor;
                else if (Scenario.CameraPosition.Y - Ball.Position.Y > Scenario.ViewHeight * Scenario.CameraFactor)  //up
                    Scenario.CameraPosition.Y = Ball.Position.Y + Scenario.ViewHeight * Scenario.CameraFactor;
            }
            if (!rightBorder && !leftBorder)
            {
                if (Ball.Position.X - Scenario.CameraPosition.X > Scenario.ViewWidth * Scenario.CameraFactor)    //left
                    Scenario.CameraPosition.X = Ball.Position.X - Scenario.ViewWidth * Scenario.CameraFactor;
                else if (Scenario.CameraPosition.X - Ball.Position.X > Scenario.ViewWidth * Scenario.CameraFactor)   //right
                    Scenario.CameraPosition.X = Ball.Position.X + Scenario.ViewWidth * Scenario.CameraFactor;
            }
        }

        public void HandleFreeKicks(GameTime gameTime)
        {
            if (KickOff)
            {
                if (HomeTeamStart)
                    Player1.CheckControls(gameTime);//player1
                else if (!Player2Cpu)
                    Player2.CheckControls(gameTime);//player2

                //reset the teams position
                if (KickOffStart)
                {
                    HomeTeam.OrganizePlayers(true);
                    AwayTeam.OrganizePlayers(true);
                    HomeTeam.Update(gameTime);
                    AwayTeam.Update(gameTime);
                    KickOffStart = false;
                    Ball.Reset();
                }
                Player starter, starter2;
                if (HomeTeamStart)
                {
                    starter = HomeTeam.GetClosestPlayer(Ball.Position, null);
                    starter2 = HomeTeam.GetClosestPlayer(Ball.Position, starter);
                }
                else
                {
                    starter = AwayTeam.GetClosestPlayer(Ball.Position, null);
                    starter2 = AwayTeam.GetClosestPlayer(Ball.Position, starter);
                    if (Player2Cpu)
                    {
                        starter.Update(gameTime);
                        starter2.Update(gameTime);
                        if (!HomeTeamStart)
                        {
                            if (starter.HasBall && (int)starter2.Position.Y == 0)
                            {
                                starter.Pass(gameTime, starter2);
                            }
                        }
                    }
                }
                starter.GoTo(new Vector2(-15, 0), false, true, false);
                starter2.GoTo(new Vector2(45, 0), false, true, false);
                if (!Player2Cpu || (HomeTeamStart && Player2Cpu))
                {
                    starter.Update(gameTime);
                    starter2.Update(gameTime);
                }
                if (Ball.Pass || Ball.Shot)
                {
                    KickOff = false;
                }
            }
            else if (Ball.FreeKick)
            {
                if (FreeKickStart)
                {
                    if (Ball.CurrentPlayer != null)
                    {
                        if (Ball.CurrentPlayer.Team == Player1.Team)
                            TmpTeam = Player2.Team;
                        else
                            TmpTeam = Player1.Team;
                        Ball.CurrentPlayer.Team.Update(gameTime);
                        FreeKickStart = false;
                    }
                }
                if (TmpTeam == Player1.Team)
                    Player1.CheckControls(gameTime);//player1
                else if (!Player2Cpu) Player2.CheckControls(gameTime);//player2

                if (TmpTeam != null)
                    TmpTeam.Update(gameTime);
            }
        }

        public void setState(String State, int MiliSeconds)
        {
            if (State == "Full-Time")
            {
                if(HomeScore - AwayScore >= 3 && Player2Cpu){   //ganhar por 3 ou mais ao cpu
                    Unstoppable.Play();
                    Panel.Status = "Unstoppable";
                }
                else if (Math.Abs(HomeScore - AwayScore) >= 3)  //ganhar/perder por 3 ou mais no 1 vs 1 || perder por 3 ou mais contra o cpu
                {
                    Humiliation.Play();
                    Panel.Status = "Humiliation";
                }
                else
                {
                    Panel.Status = State;
                    whistle.Play();
                }
            }
            else
            {
                Panel.Status = State;
                whistle.Play();
            }

            timer = MiliSeconds;
            Pause = true;

            
        }
    }
}