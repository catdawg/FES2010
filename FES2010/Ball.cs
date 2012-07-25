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

    public class Ball : Microsoft.Xna.Framework.GameComponent
    {
        //textures, animation
        //Texture2D texture;
        Texture2D spriteSheet;
        float timer = 0f;
        float rotationSpeedFactor = 10000f; //the bigger the smaller the ball spins at max speed
        int frameCount = 6;
        int currentFrame = 0;
        int spriteWidth = 24;
        int spriteHeight = 24;
        Rectangle sourceRect;

        //bound box
        public Rectangle Rectangle { get; set; }

        //animation variables
        Vector2 position {get; set;}
        Vector2 acceleration;
        Vector2 speed = new Vector2(0,0);  //current ball speed

        //properties
        public int Size { get; set; }      //ball size
        float frictionFactor; //ground's friction effect on the ball movement
        Player currentPlayer {get; set;}
        public bool Shot { get; set; }  //true if the ball was shot at goal
        public bool Pass { get; set; }
        public bool FreeKick { get; set; }
        
        

        public Ball(Game game, Weather weather)
            : base(game)
        {
            //size = texture.Width;
            Size = 15;
            Shot = false;
            Pass = false;
            FreeKick = true;

            //set the initial position and acceleration
            Reset();

            //bound box rectangle
            Rectangle = new Rectangle((int)Position.X, (int)Position.Y, Size, Size);

            //current player who has the ball
            CurrentPlayer = null;

            switch (weather)
            {
                case Weather.sunny:
                    frictionFactor = 0.9f; break;

                case Weather.rainy:
                    frictionFactor = 2.0f; break;
            }
        }

        public void ApplyImpulse(Vector2 value)
        {
            if (FreeKick) FreeKick = false;
            speed += value;
        }

        public void Stop()
        {
            speed.X *= 0;
            speed.Y *= 0;
            acceleration.X *= 0;
            acceleration.Y *= 0;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }


        public void LoadContent()
        {
            //load texture
            //texture = Game.Content.Load<Texture2D>("ball");
            //load animation sprite
            spriteSheet = Game.Content.Load<Texture2D>("ballAnim");
          
            //destinationRect = new Rectangle(0, 0, spriteWidth, spriteHeight);
        }

        public void Reset()
        {
            //center of the ball
            //Vector2 centerOfTheBall = new Vector2(size / 2.0f, size / 2.0f);

            //initial position..in the center of the field
            Position = new Vector2(0.0f, 0.0f);// -centerOfTheBall;

            Rectangle = new Rectangle(0, 0, Size, Size);
           
            CurrentPlayer = null;

            FreeKick = true;    //bola ao meio campo, só se mexe com um passe

            Stop();
        }

        public override void Update(GameTime gameTime)
        {
            //animation update
            Animate(gameTime);

            speed += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
            speed -= frictionFactor * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

             //update ball's positioning
            Position += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //update bound box to follow the ball's movement
            Rectangle = new Rectangle((int)Position.X, (int)Position.Y, Size, Size);
            
            base.Update(gameTime);
        }
        public void Animate(GameTime gameTime) 
        {
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timer > rotationSpeedFactor/getSpeed())
            {
                currentFrame++;
                if (currentFrame > frameCount - 1)
                {
                    currentFrame = 0;
                }
                timer = 0f;
            }
            sourceRect = new Rectangle(currentFrame * spriteWidth, 0, spriteWidth, spriteHeight);
        }
        public void Draw(GameTime gameTime)
        {
            // draw the ball
            //((Game)Game).Match.Scenario2DPainter.DrawInScenario(texture, Rectangle, null, Color.White, 0.0f, new Vector2(texture.Width/2f, texture.Height/2f), SpriteEffects.None, 1);
            ((Game)Game).Match.Scenario2DPainter.DrawInScenario(spriteSheet, Rectangle, sourceRect, Color.White, 0.0f, new Vector2(12f, 12f), SpriteEffects.None, 1);
        }

        //returns a value representing the ball's current speed
        public float getSpeed()
        {
            return (float)Math.Sqrt(Math.Pow(speed.X, 2) + Math.Pow(speed.Y, 2));
        }

        public Player CurrentPlayer
        {
            get
            {
                return currentPlayer;
            }
            set
            {
                currentPlayer = value;
                if (Shot)
                {
                    Shot = false;
                    if (currentPlayer != null)
                        if (currentPlayer.TacticalPosition == TacticalPosition.goalkeeper && !FreeKick)
                        {
                            if(((Game)Game).Match.Rand.Next(2) > 0)
                                ((Game)Game).Match.SaveOh.Play();
                        }
                    ((Game)Game).Match.HomeTeam.GetGK().ReflexTimer = 0f;
                    ((Game)Game).Match.AwayTeam.GetGK().ReflexTimer = 0f;
                }
                if (Pass)
                {
                    Pass = false;
                }
            }
        }

        public Boolean CheckVerticalLimits()
        {
            if (Position.Y <= -((Game)Game).Match.Field.HalfSize.Y + 40 || Position.Y >= ((Game)Game).Match.Field.HalfSize.Y - 40)   //out of bounds
                return true;
            return false;
        }
        public Boolean CheckHorizontalLimits()
        {
            if (Position.X <= -((Game)Game).Match.Field.HalfSize.X || Position.X >= ((Game)Game).Match.Field.HalfSize.X)   //out of bounds
                return true;
            return false;
        }

        public Boolean IsGoal()
        {

            if (Position.Y < ((Game)Game).Match.Field.Measures.Top && Position.Y > ((Game)Game).Match.Field.Measures.Top - Size
                && Position.X >= ((Game)Game).Match.Field.Measures.GoalStart
                && Position.X <= ((Game)Game).Match.Field.Measures.GoalEnd)  //top goal
            {
                ((Game)Game).Match.HomeScore++;
                ((Game)Game).Match.Panel.PanelColor = ((Game)Game).Match.Panel.HomeColor;
                ((Game)Game).Match.NetSound.Play();
                ((Game)Game).Match.Goal.Play();
                ((Game)Game).Match.HomeTeamStart = false;
                ((Game)Game).Match.KickOff = true;
                ((Game)Game).Match.KickOffStart = true;
                return true;
            }
            else if (Position.Y > ((Game)Game).Match.Field.Measures.Bottom && Position.Y < ((Game)Game).Match.Field.Measures.Bottom + Size
                && Position.X >= ((Game)Game).Match.Field.Measures.GoalStart
                && Position.X <= ((Game)Game).Match.Field.Measures.GoalEnd)  //bottom goal
            {
                ((Game)Game).Match.AwayScore++;
                ((Game)Game).Match.Panel.PanelColor = ((Game)Game).Match.Panel.AwayColor;
                ((Game)Game).Match.NetSound.Play();
                ((Game)Game).Match.Goal.Play();
                ((Game)Game).Match.HomeTeamStart = true;
                ((Game)Game).Match.KickOff = true;
                ((Game)Game).Match.KickOffStart = true;
                return true;
            }

            return false;
        }

        public Boolean CheckGoalBorders()
        {
            float lineWidth = 5;
            float postWidth = 8;
            float distFactor = 10; //GK moves when the ball's this dist off post

            //ball hits the post 
            if ((Position.Y > ((Game)Game).Match.Field.Measures.Bottom - Size - lineWidth
                && Position.Y <= ((Game)Game).Match.Field.Measures.Bottom - Size)
                || (Position.Y < ((Game)Game).Match.Field.Measures.Top + Size + lineWidth
                && Position.Y >= ((Game)Game).Match.Field.Measures.Top + Size))
            {
                
                //from the front
                if ((Position.X >= ((Game)Game).Match.Field.Measures.GoalStart - postWidth - distFactor
                        && Position.X <= ((Game)Game).Match.Field.Measures.GoalStart)
                    || Position.X >= ((Game)Game).Match.Field.Measures.GoalEnd
                        && Position.X <= ((Game)Game).Match.Field.Measures.GoalEnd + postWidth + distFactor)
                {
                    if(Position.Y < 0)
                        Position = new Vector2(Position.X, ((Game)Game).Match.Field.Measures.Top + Size + lineWidth); 
                    else Position = new Vector2(Position.X, ((Game)Game).Match.Field.Measures.Bottom - Size - lineWidth);
                    speed.Y *= -0.8f;
                    ((Game)Game).Match.PostSound.Play();
                    return true;
                }
                //from the sides
                else if ((Position.X >= ((Game)Game).Match.Field.Measures.GoalStart - postWidth - distFactor
                        && Position.X <= ((Game)Game).Match.Field.Measures.GoalStart - postWidth)
                    || Position.X >= ((Game)Game).Match.Field.Measures.GoalEnd + postWidth
                        && Position.X <= ((Game)Game).Match.Field.Measures.GoalEnd + postWidth + distFactor
                    || Position.X >= ((Game)Game).Match.Field.Measures.GoalStart
                        && Position.X <= ((Game)Game).Match.Field.Measures.GoalStart + distFactor
                    || Position.X >= ((Game)Game).Match.Field.Measures.GoalEnd - distFactor
                        && Position.X <= ((Game)Game).Match.Field.Measures.GoalEnd)
                {
                    speed.X *= -0.3f;
                    ((Game)Game).Match.PostSound.Play();
                    return true;
                }

            }
            return false;
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                if (!FreeKick)
                    position = value;
            }
        }

        public Boolean OutOfPlay() 
        {
            if (CheckGoalBorders())
                return false;
            if (CheckVerticalLimits())
            {
                if (IsGoal())   //goal
                {
                    ((Game)Game).Match.setState("GOAL!", 2000);
                    Reset();
                    ((Game)Game).Match.Scenario.CameraPosition = new Vector2(0);
                    //reset the teams position
                    ((Game)Game).Match.HomeTeam.OrganizePlayers(true);
                    ((Game)Game).Match.AwayTeam.OrganizePlayers(true);                    
                }
                else            //goal kick or corner
                {
                    if (Position.Y < 0)
                    {
                        if (CurrentPlayer.Team.IsHomeTeam)  //goalkick
                        {
                            ((Game)Game).Match.setState("Goal Kick", 1000);
                            Position = new Vector2(((Game)Game).Match.Field.Measures.GoalKickX2, -((Game)Game).Match.Field.HalfSize.Y + 130);
                            ((Game)Game).Match.AwayTeam.SelectGK();
                            ((Game)Game).Match.Player2.GoTo(((Game)Game).Match.Ball.Position, false, false, false);
                            ((Game)Game).Match.HomeTeam.OrganizePlayers(true);
                            ((Game)Game).Match.AwayTeam.OrganizePlayers(true);
                            ((Game)Game).Match.Player2.FreeKick = true;
                        }
                        else    //corner kick
                        {
                            Team tmp = ((Game)Game).Match.Ball.CurrentPlayer.Team;
                            Player tmpP = null;
                            ((Game)Game).Match.setState("Corner Kick", 1000);
                            if (Position.X < 0)    //left corner
                            {
                                if (((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position.X < ((Game)Game).Match.Field.Measures.Left + 300)
                                    ((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position = new Vector2(((Game)Game).Match.Field.Measures.Left + 300, ((Game)Game).Match.Ball.CurrentPlayer.Position.Y);
                                Position = new Vector2(((Game)Game).Match.Field.Measures.LeftCornerX, ((Game)Game).Match.Field.Measures.TopCornerY);

                                if (tmp == ((Game)Game).Match.Player1.Team)
                                    tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player2.Team);
                                else if (tmp == ((Game)Game).Match.Player2.Team)
                                    tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player1.Team);
                                
                                tmpP.GoTo(new Vector2(Position.X - 8, Position.Y), false, false, false);
                                tmpP.FreeKick = true;
                            }
                            else    //right corner
                            {
                                if (((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position.X > -((Game)Game).Match.Field.Measures.Left - 300)
                                    ((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position = new Vector2(-((Game)Game).Match.Field.Measures.Left - 300, ((Game)Game).Match.Ball.CurrentPlayer.Position.Y);
                                Position = new Vector2(((Game)Game).Match.Field.Measures.RightCornerX, ((Game)Game).Match.Field.Measures.TopCornerY);

                                if (tmp == ((Game)Game).Match.Player1.Team)
                                    tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player2.Team);
                                else if (tmp == ((Game)Game).Match.Player2.Team)
                                    tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player1.Team);
                                
                                tmpP.GoTo(new Vector2(Position.X + 8, Position.Y), false, false, false);
                                tmpP.FreeKick = true;
                            }
                        }
                    }
                    else if (Position.Y > 0)
                    {
                        if (!CurrentPlayer.Team.IsHomeTeam)  //goalkick
                        {
                            ((Game)Game).Match.setState("Goal Kick", 1000);
                            Position = new Vector2(((Game)Game).Match.Field.Measures.GoalKickX1, ((Game)Game).Match.Field.HalfSize.Y - 130);
                            ((Game)Game).Match.HomeTeam.SelectGK();
                            ((Game)Game).Match.Player1.GoTo(((Game)Game).Match.Ball.Position, false, false, false);
                            ((Game)Game).Match.HomeTeam.OrganizePlayers(true);
                            ((Game)Game).Match.AwayTeam.OrganizePlayers(true);
                            ((Game)Game).Match.Player1.FreeKick = true;
                        }
                        else    //corner kick
                        {
                            Team tmp = ((Game)Game).Match.Ball.CurrentPlayer.Team;
                            Player tmpP = null;
                            ((Game)Game).Match.setState("Corner Kick", 1000);
                            if (Position.X < 0)    //left corner
                            {
                                if (((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position.X < ((Game)Game).Match.Field.Measures.Left + 300)
                                    ((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position = new Vector2(((Game)Game).Match.Field.Measures.Left + 300, ((Game)Game).Match.Ball.CurrentPlayer.Position.Y);
                                Position = new Vector2(((Game)Game).Match.Field.Measures.LeftCornerX, ((Game)Game).Match.Field.Measures.BottomCornerY);

                                if (tmp == ((Game)Game).Match.Player1.Team)
                                    tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player2.Team);
                                else if (tmp == ((Game)Game).Match.Player2.Team)
                                    tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player1.Team);
                                
                                tmpP.GoTo(new Vector2(Position.X - 8, Position.Y), false, false, false);
                                tmpP.FreeKick = true;
                            }
                            else    //right corner
                            {
                                if (((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position.X > -((Game)Game).Match.Field.Measures.Left - 300)
                                    ((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position = new Vector2(-((Game)Game).Match.Field.Measures.Left - 300, ((Game)Game).Match.Ball.CurrentPlayer.Position.Y);
                                Position = new Vector2(((Game)Game).Match.Field.Measures.RightCornerX, ((Game)Game).Match.Field.Measures.BottomCornerY);

                                if (tmp == ((Game)Game).Match.Player1.Team)
                                    tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player2.Team);
                                else if (tmp == ((Game)Game).Match.Player2.Team)
                                    tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player1.Team);
                                
                                tmpP.GoTo(new Vector2(Position.X + 8, Position.Y), false, false, false);
                                tmpP.FreeKick = true;
                            }
                        }
                    }

                    speed = new Vector2(0, 0);
                    ((Game)Game).Match.FollowBall();
                }
                FreeKick = true;
                ((Game)Game).Match.FreeKickStart = true;
                return true;
            }
            if (CheckHorizontalLimits())    //throw-ins
            {
                Team tmp = ((Game)Game).Match.Ball.CurrentPlayer.Team;
                Player tmpP = null;
                ((Game)Game).Match.setState("Throw In", 1000);
                if (Position.X < 0) //lado esquerdo
                {
                    ((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position = new Vector2(((Game)Game).Match.Ball.CurrentPlayer.Position.X + 200, ((Game)Game).Match.Ball.CurrentPlayer.Position.Y);
                    if (tmp == ((Game)Game).Match.Player1.Team)
                        tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player2.Team);
                    else if (tmp == ((Game)Game).Match.Player2.Team)
                        tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player1.Team);
                    tmpP.GoTo(new Vector2(-((Game)Game).Match.Field.HalfSize.X - 8, Position.Y), false,false, false);
                    tmpP.FreeKick = true;
                    Position = new Vector2(-((Game)Game).Match.Field.HalfSize.X + 5, Position.Y);
                }
                else if (Position.X > 0)    //lado direito
                {
                    ((Game)Game).Match.Ball.GetClosestPlayer(tmp).Position = new Vector2(((Game)Game).Match.Ball.CurrentPlayer.Position.X - 200, ((Game)Game).Match.Ball.CurrentPlayer.Position.Y);
                    if (tmp == ((Game)Game).Match.Player1.Team)
                        tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player2.Team);
                    else if (tmp == ((Game)Game).Match.Player2.Team)
                        tmpP = ((Game)Game).Match.Ball.GetClosestPlayer(((Game)Game).Match.Player1.Team);
                    tmpP.GoTo(new Vector2(((Game)Game).Match.Field.HalfSize.X + 8, Position.Y), false, false, false);
                    tmpP.FreeKick = true;
                    Position = new Vector2(((Game)Game).Match.Field.HalfSize.X - 5, Position.Y);
                }

                speed = new Vector2(0, 0);

                FreeKick = true;
                ((Game)Game).Match.FreeKickStart = true;
                return true;
            }
            
            return false;
        }

        public Player GetClosestPlayer(Team team)
        {
            int closest = 0;    //current closest
            float tempDist = float.MaxValue;
            float newDist = 0f;
            for(int i=0;i<team.Players.Count;i++)
            {
                Player p = (Player)team.Players[i];
                newDist = (float)Math.Sqrt(Math.Pow((p.Position.X - Position.X), 2) + Math.Pow((p.Position.Y - Position.Y), 2));
                if (newDist <= tempDist)
                {
                    closest = i;
                    tempDist = newDist;
                }
            }
            return (Player)team.Players[closest];
        }
        public Player GetClosestPlayer(Team team, Player ignore)
        {
            int closest = 0;    //current closest
            float tempDist = float.MaxValue;
            float newDist = 0f;
            for (int i = 0; i < team.Players.Count; i++)
            {
                Player p = (Player)team.Players[i];
                if (p.Equals(ignore))
                    continue;
                newDist = (float)Math.Sqrt(Math.Pow((p.Position.X - Position.X), 2) + Math.Pow((p.Position.Y - Position.Y), 2));
                if (newDist <= tempDist)
                {
                    closest = i;
                    tempDist = newDist;
                }
            }
            return (Player)team.Players[closest];
        }
    }
}