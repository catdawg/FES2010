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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Player : Microsoft.Xna.Framework.GameComponent, IComparable
    {
        public String Name { get; set; }
        int number;
        public TacticalPosition TacticalPosition { get; set; }
        public Side Side { get; set; }
        public Team Team { get; set; }

        //textures, animation
        Texture2D texture;
        Texture2D selectedTexture;
        public bool IsAnimated { get; set; } //indica se o jogador se está a movimentar para algum sitio sozinho
        public bool IsPressedAnimation { get; set; }  //indica que apesar de o jogador n tar seleccionado ta a ser animado por uma tecla premida
        public Vector2 DestAnimation { get; set; }
        public float ReflexTimer  { get; set; }
        public float ReflexTime { get; set; }   //tempo default de resposta do GK (pode variar com os skills dele)

        //pass/shot
        public float FreePassForce { get; set; }
        public float PassForce { get; set; }

        //bound box
        Rectangle rectangle;

        //animation variables
        Vector2 position { get; set; }
        public float InitPositionX { get; set; }
        

        //properties
        int size = 30;              //representative square size
        public float TopSpeed { get; set; }             //top speed which decreases while running with a rate related to the stamina stat
        public float TopSpeedFactor { get; set; }       //factor that decreases with each run limiting the new top speed
        float speedFactor;          //actual speed of the player 
        public bool FreeKick { get; set; } 

        public int WalkingSpeed { get; set; }     //walking speed for all the players
        public bool HasBall { get; set; }           //has the ball in his posession
        public bool Numb { get; set; }  //after losing the ball the player gets numb for a small amount of time
        public float NumbTime { get; set; } //the amount of time
        public float timer { get; set; }

        bool running;

        // Attributes
        int defense;
        int goalkeeping;
        public int Offense { get; set; }
        public int Shot { get; set; }
        public int Speed { get; set; }
        int stamina;


        public Player(Game game, Team team, String name, int number, TacticalPosition tacticalPosition, Side side, int defense, int goalkeeping, int offense,
            int shot, int speed, int stamina, float x, float y)
            : base(game)
        {
            this.Team = team;
            this.Name = name;
            this.number = number;
            this.TacticalPosition = tacticalPosition;
            this.Side = side;
            this.defense = defense;
            this.goalkeeping = goalkeeping;
            this.Offense = offense;
            this.Shot = shot;
            this.Speed = speed;
            this.stamina = stamina;

            if(this.TacticalPosition.Equals(TacticalPosition.goalkeeper))
                this.WalkingSpeed = 150;
            else this.WalkingSpeed = 250;

            speedFactor = WalkingSpeed;
            TopSpeedFactor = 1.8f;
            TopSpeed = (WalkingSpeed * TopSpeedFactor) * (speed / 100f);    //gets the player's max (initial) running speed from his speed stat

            FreePassForce = 0.0f;
            PassForce = 0.0f;
            FreeKick = false;

            HasBall = false;
            running = false; IsAnimated = false; IsPressedAnimation = false;
            Numb = false;
            NumbTime = 500f;
            timer = 0f;
            
            ReflexTimer = 0f;
            ReflexTime = (float)(500.0f - (goalkeeping * 250.0f / 100.0f)); //250 valor min; 500 valor max;

            //set the initial position and acceleration
            Reset(x, y);

        }


        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            //update bound box to follow the player's movement
            rectangle.Location = new Point((int)Position.X, (int)Position.Y);

            //reset Numb
            if (Numb)
            {
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (timer >= NumbTime)
                {
                    timer = 0f;
                    Numb = false;
                }
            }

            //not running - recharging stamina
            if (!running)  
            {
                if (TopSpeed < (WalkingSpeed * TopSpeedFactor) * (Speed / 100f))
                {
                    TopSpeed++;    //recharging with a new slighty smaller top speed
                }
            }

            //animate player if that's the case
            if (IsAnimated)
            {
                GoTo(DestAnimation, false, false, false);
            }

            //check if the player has become in contact with the ball
            CheckCollisions();

            base.Update(gameTime);
        }

        //tells if the given player's in a box
        public bool InTheBox(Player player)
        {
            if (player.position.Y > ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.BoxHeight ||
                player.position.Y < ((Game)Game).Match.Field.Measures.Top + ((Game)Game).Match.Field.Measures.BoxHeight)
            {
                if (player.position.X > -((Game)Game).Match.Field.Measures.HalfBoxWidth &&
                    player.position.X < ((Game)Game).Match.Field.Measures.HalfBoxWidth)
                    return true;
            }
            return false;
        }

        private void CheckCollisions()
        {
            if (((Game)Game).Match.Ball.Rectangle.Intersects(new Rectangle((int)Position.X, (int)Position.Y, size / 2, size / 2)) && !HasBall && !Numb && !IsAnimated)
            {
                //if it's the keeper who has the ball and he's in the box, he can't be desarmed
                if (((Game)Game).Match.Ball.CurrentPlayer != null &&
                    ((Game)Game).Match.Ball.CurrentPlayer.TacticalPosition == TacticalPosition.goalkeeper &&
                    ((Game)Game).Match.Ball.CurrentPlayer.HasBall)
                {
                    if (InTheBox(((Game)Game).Match.Ball.CurrentPlayer))
                        return;
                }
                HasBall = true;
                if (((Game)Game).Match.Ball.CurrentPlayer != null && ((Game)Game).Match.Ball.CurrentPlayer != this)
                {
                    ((Game)Game).Match.Ball.CurrentPlayer.HasBall = false;
                    ((Game)Game).Match.Ball.CurrentPlayer.Numb = true;
                }
                ((Game)Game).Match.Ball.CurrentPlayer = this;

                //auto select the player who catches the ball
                if (Team.IsHomeTeam) ((Game)Game).Match.Player1 = this;
                else ((Game)Game).Match.Player2 = this;

                ((Game)Game).Match.Ball.Stop();
            }
        }

        public void Draw(GameTime gameTime)
        {
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            ((Game)Game).Match.Scenario2DPainter.DrawInScenario(texture, rectangle, null, Team.Color, 0.0f, origin, SpriteEffects.None, 1);
            if (this.Equals(((Game)Game).Match.Player1) || this.Equals(((Game)Game).Match.Player2))
            {
                ((Game)Game).Match.Scenario2DPainter.DrawInScenario(selectedTexture, rectangle, null, InvertColor(Team.Color), 0.0f, origin, SpriteEffects.None, 1);
            }
        }

        public void LoadContent()
        {
            //load texture
            texture = Game.Content.Load<Texture2D>("player");
            selectedTexture = Game.Content.Load<Texture2D>("selectedPlayer");
        }

        public void Reset(float x, float y)
        {
            //initial position..
            this.Position = new Vector2(x, y);
            //bound box rectangle
            this.rectangle = new Rectangle((int)Position.X, (int)Position.Y, size, size);
        }

        public void Movement(GameTime gameTime, Vector2 add)
        {
            if (checkLimits(add))
            {
                //keep the player from accidentally making an owngoal while controlling his gkeeper
                if (TacticalPosition == TacticalPosition.goalkeeper)
                {
                    if (HasBall && (add.Y > 0 && Team.IsHomeTeam && Position.Y > ((Game)Game).Match.Field.Measures.Bottom - 30 ||
                                    add.Y < 0 && !Team.IsHomeTeam && Position.Y < ((Game)Game).Match.Field.Measures.Top + 30))
                        return; //ignore this movement
                }

                add.Normalize();
                Position += add * speedFactor * (float)gameTime.ElapsedGameTime.TotalSeconds;   //move player
                if (HasBall && !Numb)   //has the ball
                    ((Game)Game).Match.Ball.Position = Position + add * (size / 2f);    //move ball
                else if (HasBall) //numb
                    HasBall = false;
            }
        }

        public Boolean CheckVerticalLimits(bool up)
        {
            if (Position.Y <= -((Game)Game).Match.Field.HalfSize.Y + 20 && up || Position.Y >= ((Game)Game).Match.Field.HalfSize.Y - 20 && !up)   //out of bounds
            {
                HasBall = false;
                return true;
            }
            return false;
        }
        public Boolean CheckHorizontalLimits(bool left)
        {
            if (Position.X <= ((Game)Game).Match.Field.Measures.Left - 40 && left || !left && Position.X >= ((Game)Game).Match.Field.HalfSize.X + 50)   //out of bounds
            {
                HasBall = false;
                return true;
            }
            return false;
        }

        //to a given movement direction assures that the destination position is valid
        public Boolean checkLimits(Vector2 add)
        {
            if ((add.Y == -1 && !CheckVerticalLimits(true) && add.X == 1 && !CheckHorizontalLimits(false)) ||
                (add.Y == -1 && !CheckVerticalLimits(true) && add.X == -1 && !CheckHorizontalLimits(true)) ||
                (add.Y == -1 && !CheckVerticalLimits(true) && add.X == 0) ||
                (add.Y == 1 && !CheckVerticalLimits(false) && add.X == 1 && !CheckHorizontalLimits(false)) ||
                (add.Y == 1 && !CheckVerticalLimits(false) && add.X == -1 && !CheckHorizontalLimits(true)) ||
                (add.Y == 1 && !CheckVerticalLimits(false) && add.X == 0) ||
                (add.X == 1 && !CheckHorizontalLimits(false) && add.Y == -1 && !CheckVerticalLimits(true)) ||
                (add.X == 1 && !CheckHorizontalLimits(false) && add.Y == 1 && !CheckVerticalLimits(false)) ||
                (add.X == 1 && !CheckHorizontalLimits(false) && add.Y == 0) ||
                (add.X == -1 && !CheckHorizontalLimits(true) && add.Y == -1 && !CheckVerticalLimits(true)) ||
                (add.X == -1 && !CheckHorizontalLimits(true) && add.Y == 1 && !CheckVerticalLimits(false)) ||
                (add.X == -1 && !CheckHorizontalLimits(true) && add.Y == 0))
            {
                return true;    //valid
            }
            return false;
        }

        public void CheckControls(GameTime gameTime)
        {
            if (Team.IsHomeTeam)
            {
                if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Shoot))
                {
                    Shoot(gameTime,false);
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Pass))
                {
                    PassForce++;
                }
                else if (((Game)Game).CurrentKeyboardState.IsKeyUp(((Game)Game).Cmd1.Pass) && PassForce >= 1)
                {
                    Pass(gameTime);
                    if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.ChangePlayer))  //Pass and run
                    {
                        if (this != ((Game)Game).Match.Player1)
                        {
                            if (Math.Abs(((Game)Game).Match.Field.Measures.Top - Position.Y) > 300)
                                GoTo(new Vector2(Position.X, Position.Y - 300), false, false, false);
                        }
                    }
                }
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.FreePass))
                {
                    FreePassForce++;
                }
                else if (((Game)Game).CurrentKeyboardState.IsKeyUp(((Game)Game).Cmd1.FreePass) && FreePassForce >= 1)
                {
                    FreePass(gameTime, FreePassForce);
                }
                if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Up) && Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Left))
                {
                    Movement(gameTime, new Vector2(-1, -1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Up) && Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Right))
                {
                    Movement(gameTime, new Vector2(1,-1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Down) && Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Right))
                {
                    Movement(gameTime, new Vector2(1,1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Down) && Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Left))
                {
                    Movement(gameTime, new Vector2(-1, 1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Up))
                {
                    Movement(gameTime, new Vector2(0, -1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Down))
                {
                     Movement(gameTime, new Vector2(0,1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Left))
                {
                     Movement(gameTime, new Vector2(-1,0));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Right))
                {
                    Movement(gameTime, new Vector2(1,0));
                }
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.GoalerOut) && !((Game)Game).Match.Player1.HasBall)
                {
                    Team.GetGK().GoTo(((Game)Game).Match.Ball.Position, false, true, false);
                }
                else if (((Game)Game).CurrentKeyboardState.IsKeyUp(((Game)Game).Cmd1.GoalerOut)
                    || ((Game)Game).Match.Player1.HasBall)  //gk returning to its goal
                {
                    if (!Team.GetGK().HasBall && ((Game)Game).Match.Player1 != Team.GetGK())
                    {
                        if (Team.IsHomeTeam) Team.GetGK().GoTo(new Vector2(Team.GetGK().Position.X, ((Game)Game).Match.Field.Measures.Bottom - 5), false, false, false);
                        else Team.GetGK().GoTo(new Vector2(Team.GetGK().Position.X, ((Game)Game).Match.Field.Measures.Top + 5), false, false, false);
                    }
                }
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.Pressure) && !((Game)Game).Match.Player1.HasBall)
                {
                    Pressure();
                }
                if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd1.Sprint) && !running)  //starts to run
                {
                    running = true;
                    speedFactor = TopSpeed;
                    TopSpeedFactor -= 0.02f;    //diminuish the max possible top speed
                }
                else if (running && Keyboard.GetState().IsKeyUp(((Game)Game).Cmd1.Sprint))    //ends the run
                {
                    running = false;
                    speedFactor = WalkingSpeed;
                    if (TopSpeedFactor <= 1f)
                        TopSpeed = WalkingSpeed;
                }
                else if (running)   //is running and the topSpeed decreases
                {
                    if (TopSpeed > WalkingSpeed)
                    {
                        TopSpeed -= (3f - (stamina * 2 / 100f));  //current run top speed
                        speedFactor = TopSpeed;
                    }
                    else speedFactor = WalkingSpeed;
                }
            }
            else 
            {
                //player 2
                if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Shoot))
                {
                    Shoot(gameTime,false);
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Pass))
                {
                    PassForce++;
                }
                else if (((Game)Game).CurrentKeyboardState.IsKeyUp(((Game)Game).Cmd2.Pass) && PassForce >= 1)
                {
                    Pass(gameTime);
                    if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.ChangePlayer))  //Pass and run
                    {
                        if (this != ((Game)Game).Match.Player2)
                        {
                            if(((Game)Game).Match.Field.Measures.Bottom - Position.Y > 300)
                                GoTo(new Vector2(Position.X, Position.Y + 300), false, false, false);
                        }
                    }
                }
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.FreePass))
                {
                    FreePassForce++;
                }
                else if (((Game)Game).CurrentKeyboardState.IsKeyUp(((Game)Game).Cmd2.FreePass) && FreePassForce >= 1)
                {
                    FreePass(gameTime, FreePassForce);
                }
                if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Up) && Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Left))
                {
                    Movement(gameTime, new Vector2(-1, -1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Up) && Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Right))
                {
                    Movement(gameTime, new Vector2(1, -1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Down) && Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Right))
                {
                    Movement(gameTime, new Vector2(1, 1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Down) && Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Left))
                {
                    Movement(gameTime, new Vector2(-1, 1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Up))
                {
                    Movement(gameTime, new Vector2(0, -1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Down))
                {
                    Movement(gameTime, new Vector2(0, 1));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Left))
                {
                    Movement(gameTime, new Vector2(-1, 0));
                }
                else if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Right))
                {
                    Movement(gameTime, new Vector2(1, 0));
                }
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.GoalerOut) && !((Game)Game).Match.Player2.HasBall)
                {
                    Team.GetGK().GoTo(((Game)Game).Match.Ball.Position, false, true, false);
                }
                else if (((Game)Game).CurrentKeyboardState.IsKeyUp(((Game)Game).Cmd2.GoalerOut) 
                         || ((Game)Game).Match.Player2.HasBall)  //gk returs to its goal
                {
                    if (!Team.GetGK().HasBall && ((Game)Game).Match.Player2 != Team.GetGK())
                    {
                        if (Team.IsHomeTeam) Team.GetGK().GoTo(new Vector2(Team.GetGK().Position.X, ((Game)Game).Match.Field.Measures.Bottom - 5), false, false, false);
                        else Team.GetGK().GoTo(new Vector2(Team.GetGK().Position.X, ((Game)Game).Match.Field.Measures.Top + 5), false, false, false);
                    }
                }
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.Pressure) && !((Game)Game).Match.Player2.HasBall) //pressure
                {
                    Pressure();
                }
                if (Keyboard.GetState().IsKeyDown(((Game)Game).Cmd2.Sprint) && !running)  //starts to run
                {
                    running = true;
                    speedFactor = TopSpeed;
                    TopSpeedFactor -= 0.02f;    //diminuish the max possible top speed
                }
                else if (running && Keyboard.GetState().IsKeyUp(((Game)Game).Cmd2.Sprint))    //ends the run
                {
                    running = false;
                    speedFactor = WalkingSpeed;
                    if(TopSpeedFactor <= 1f)
                        TopSpeed = WalkingSpeed;
                }
                else if (running)   //is running and the topSpeed decreases
                {
                    if (TopSpeed > WalkingSpeed)
                    {
                        TopSpeed -= (3f - (stamina * 2 / 100f));  //current run top speed
                        speedFactor = TopSpeed;
                    }
                    else speedFactor = WalkingSpeed;
                }
            }
        }

        



        public void Shoot(GameTime gameTime, bool cpu)
        {
            if (HasBall && !Numb)
            {
                if (FreeKick) FreeKick = false;
                HasBall = false;

                Vector2 goal;//perfect shots
                if (Team.IsHomeTeam)
                {
                    if (!cpu)
                    {
                        if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.Right)) //right corner
                            goal = new Vector2(((Game)Game).Match.Field.Measures.GoalEnd - ((Game)Game).Match.Ball.Size * 1.6f, ((Game)Game).Match.Field.Measures.Top);
                        else if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.Left))    //left corner
                            goal = new Vector2(((Game)Game).Match.Field.Measures.GoalStart + ((Game)Game).Match.Ball.Size * 1.6f, ((Game)Game).Match.Field.Measures.Top);
                        else goal = new Vector2(0, ((Game)Game).Match.Field.Measures.Top);  //middle
                    }
                    else //cpu
                    {
                        int rand = ((Game)Game).Match.Rand.Next(3);
                        if (rand == 0)
                            goal = new Vector2(((Game)Game).Match.Field.Measures.GoalEnd - ((Game)Game).Match.Ball.Size * 1.6f, ((Game)Game).Match.Field.Measures.Top);
                        else if(rand == 1)
                            goal = new Vector2(((Game)Game).Match.Field.Measures.GoalStart + ((Game)Game).Match.Ball.Size * 1.6f, ((Game)Game).Match.Field.Measures.Top);
                        else 
                            goal = new Vector2(0, ((Game)Game).Match.Field.Measures.Top);  //middle
                    }
                }
                else
                {
                    if (!cpu)
                    {
                        if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.Right))
                            goal = new Vector2(((Game)Game).Match.Field.Measures.GoalEnd - ((Game)Game).Match.Ball.Size * 1.6f, ((Game)Game).Match.Field.Measures.Bottom);
                        else if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.Left))
                            goal = new Vector2(((Game)Game).Match.Field.Measures.GoalStart + ((Game)Game).Match.Ball.Size * 1.6f, ((Game)Game).Match.Field.Measures.Bottom);
                        else goal = new Vector2(0, ((Game)Game).Match.Field.Measures.Bottom);
                    }
                    else //cpu
                    {
                        int rand = ((Game)Game).Match.Rand.Next(3);
                        if (rand == 0)
                            goal = new Vector2(((Game)Game).Match.Field.Measures.GoalEnd - ((Game)Game).Match.Ball.Size * 1.6f, ((Game)Game).Match.Field.Measures.Bottom);
                        else if(rand == 1)
                            goal = new Vector2(((Game)Game).Match.Field.Measures.GoalStart + ((Game)Game).Match.Ball.Size * 1.6f, ((Game)Game).Match.Field.Measures.Bottom);
                        else
                            goal = new Vector2(0, ((Game)Game).Match.Field.Measures.Bottom);
                    }
                }

                Vector2 direction = goal - Position;

                float errorFactor = (100 - Offense) * (((Game)Game).Match.MaxErrorAngle / 100f);    //accuracy
                if (errorFactor != 0)
                {
                    int angle = ((Game)Game).Match.Rand.Next(0, (int)errorFactor);    //deviation angle due to lack of skill
                    Console.WriteLine("shot error angle: " + angle);

                    bool rand = (angle >= errorFactor / 2f) ? true : false;
                    direction.X += (rand) ? (float)Math.Cos(angle * Math.PI / 180f) * direction.X : -(float)Math.Cos((angle + 270) * Math.PI / 180f) * direction.X;
                    direction.Y -= (rand) ? (float)Math.Sin(angle * Math.PI / 180f) * direction.Y : -(float)Math.Sin(angle * Math.PI / 180f) * direction.Y;
                }
                else Console.WriteLine("shot error angle: 0");

                direction.Normalize();
                float shotFactor = 11;
                int shotPwr = ((Game)Game).Match.Rand.Next(Shot - 15, Shot);
                direction = direction * shotPwr * shotFactor;

                ((Game)Game).Match.PassSound.Play();
                ((Game)Game).Match.Ball.ApplyImpulse(direction);
                
                Numb = true;

                ((Game)Game).Match.Ball.Shot = true;

                //se o guarda redes chuta a bola muda de jogador para o GK voltar para a baliza
                if (TacticalPosition.Equals(TacticalPosition.goalkeeper))
                {
                    if (Team.IsHomeTeam) ((Game)Game).Match.Player1 = ((Game)Game).Match.Ball.GetClosestPlayer(Team, this);
                    else ((Game)Game).Match.Player2 = ((Game)Game).Match.Ball.GetClosestPlayer(Team, this);
                }
            }
        }

        public void Pass(GameTime gameTime)
        {
            if (HasBall && !Numb)
            {
                if (FreeKick) FreeKick = false;
                HasBall = false;

                //para onde o jogador ta a apontar
                Vector2 aim = PointingTo();

                //escolher jogador para que quer passar
                Player closestPlayer = Team.ChoosePassPlayer(this, PassForce, aim);

                //Player closestPlayer = (((Game)Game).Match.Ball.GetClosestPlayer(Team, this));
                
                Vector2 goal = closestPlayer.Position;

                Vector2 direction = goal - Position;
                
                //perfect pass
                
                float passDistance = (float)Math.Sqrt(Math.Pow(direction.X, 2) + Math.Pow(direction.Y, 2));
                float perfectPassFactor = ((Game)Game).Match.PerfectPassFactor;

                if (closestPlayer.TacticalPosition == TacticalPosition.goalkeeper)
                    perfectPassFactor = 1.8f;
                else if (passDistance <= 250)
                    perfectPassFactor = 3.5f;

                float speed = passDistance * perfectPassFactor;

                float passSpeed = Shot * (((Game)Game).Match.MaxPassSpeed / 100f); //this player's max pass speed

                if (speed > passSpeed)
                    speed = passSpeed;

                float errorFactor = (100 - Offense) * (((Game)Game).Match.MaxErrorAngle/100f);

                if (errorFactor != 0)   
                {
                    float angle = (float)((Game)Game).Match.Rand.Next(0, (int)errorFactor);    //deviation angle due to lack of skill
                    Console.WriteLine("error angle: " + angle);

                    bool rand = (angle >= errorFactor / 2f) ? true : false;
                    direction.X += (rand) ? (float)Math.Cos(angle * Math.PI / 180f) * direction.X : -(float)Math.Cos((angle + 270) * Math.PI / 180f) * direction.X;
                    direction.Y -= (rand) ? (float)Math.Sin(angle * Math.PI / 180f) * direction.Y : -(float)Math.Sin(angle * Math.PI / 180f) * direction.Y;
                }
                else Console.WriteLine("error angle: 0");

                direction.Normalize();
                direction *= speed;

                ((Game)Game).Match.PassSound.Play();
                ((Game)Game).Match.Ball.ApplyImpulse(direction);
                

                ((Game)Game).Match.Ball.Pass = true;

                if (Team.IsHomeTeam) ((Game)Game).Match.Player1 = closestPlayer;
                else ((Game)Game).Match.Player2 = closestPlayer;

                Numb = true;

                PassForce = 0f;
            }
        }


        public void Pass(GameTime gameTime, Player destination)
        {
            if (HasBall && !Numb)
            {
                HasBall = false;

                Vector2 direction = destination.Position - Position;

                //perfect pass

                float passDistance = (float)Math.Sqrt(Math.Pow(direction.X, 2) + Math.Pow(direction.Y, 2));
                float perfectPassFactor = ((Game)Game).Match.PerfectPassFactor;

                if (passDistance <= 250 && destination.TacticalPosition != TacticalPosition.goalkeeper)
                    perfectPassFactor = 3.5f;

                float speed = passDistance * perfectPassFactor;

                float passSpeed = Shot * (((Game)Game).Match.MaxPassSpeed / 100f); //this player's max pass speed

                if (speed > passSpeed)
                    speed = passSpeed;

                float errorFactor = (100 - Offense) * ((((Game)Game).Match.MaxErrorAngle-5.0f) / 100f);

                if (errorFactor != 0)
                {
                    float angle = (float)((Game)Game).Match.Rand.Next(0, (int)errorFactor);    //deviation angle due to lack of skill
                    Console.WriteLine("error angle: " + angle);

                    bool rand = (angle >= errorFactor / 2f) ? true : false;
                    direction.X += (rand) ? (float)Math.Cos(angle * Math.PI / 180f) * direction.X : -(float)Math.Cos((angle + 270) * Math.PI / 180f) * direction.X;
                    direction.Y -= (rand) ? (float)Math.Sin(angle * Math.PI / 180f) * direction.Y : -(float)Math.Sin(angle * Math.PI / 180f) * direction.Y;
                }
                else Console.WriteLine("error angle: 0");

                direction.Normalize();
                direction *= speed;

                ((Game)Game).Match.Ball.ApplyImpulse(direction);

                ((Game)Game).Match.Ball.Pass = true;

                if (Team.IsHomeTeam) ((Game)Game).Match.Player1 = destination;
                else ((Game)Game).Match.Player2 = destination;

                Numb = true;

                PassForce = 0f;
            }
        }

        public void FreePass(GameTime gameTime, float passForce)
        {
            if (HasBall && !Numb)
            {
                if (FreeKick) FreeKick = false;
                HasBall = false;

                float speed = passForce * 50;  //50 = time to speed ratio factor

                float passSpeed = Shot * (((Game)Game).Match.MaxPassSpeed / 100f); //this player's max pass speed

                if (speed > passSpeed)
                    speed = passSpeed;

                Vector2 direction = PointingTo();

                direction *= speed;

                ((Game)Game).Match.PassSound.Play();
                ((Game)Game).Match.Ball.ApplyImpulse(direction);

                ((Game)Game).Match.Ball.Pass = true;

                if (Team.IsHomeTeam) ((Game)Game).Match.Player1 = Team.GetClosestPlayer(((Game)Game).Match.Ball.Position + direction, null);
                else ((Game)Game).Match.Player2 = Team.GetClosestPlayer(((Game)Game).Match.Ball.Position + direction, null); 

                Numb = true;
                FreePassForce = 0f;
            }
        }

        public Vector2 PointingTo()
        {
            Vector2 direction = new Vector2(0f);
            if (Team.IsHomeTeam)
            {
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.Up)) direction.Y -= 1;   //virado pra cima
                else if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.Down)) direction.Y += 1;   //virado pra baixo
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.Left)) direction.X -= 1;   //virado pra esquerda
                else if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd1.Right)) direction.X += 1;   //virado pra direita

                if (direction.X == 0 && direction.Y == 0)   //jogador ta parado
                {
                    if (Position.Y > ((Game)Game).Match.Ball.Position.Y) direction.Y -= 1;   //virado pra cima
                    else if (Position.Y < ((Game)Game).Match.Ball.Position.Y) direction.Y += 1;   //virado pra baixo
                    if (Position.X > ((Game)Game).Match.Ball.Position.X) direction.X -= 1;   //virado pra esquerda
                    else if (Position.X < ((Game)Game).Match.Ball.Position.X) direction.X += 1;   //virado pra direita
                }
            }
            else
            {
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.Up)) direction.Y -= 1;   //virado pra cima
                else if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.Down)) direction.Y += 1;   //virado pra baixo
                if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.Left)) direction.X -= 1;   //virado pra esquerda
                else if (((Game)Game).CurrentKeyboardState.IsKeyDown(((Game)Game).Cmd2.Right)) direction.X += 1;   //virado pra direita

                if (direction.X == 0 && direction.Y == 0)   //jogador ta parado
                {
                    if (Position.Y > ((Game)Game).Match.Ball.Position.Y) direction.Y -= 1;   //virado pra cima
                    else if (Position.Y < ((Game)Game).Match.Ball.Position.Y) direction.Y += 1;   //virado pra baixo
                    if (Position.X > ((Game)Game).Match.Ball.Position.X) direction.X -= 1;   //virado pra esquerda
                    else if (Position.X < ((Game)Game).Match.Ball.Position.X) direction.X += 1;   //virado pra direita
                }
            }
            return direction;
        }
        public Color InvertColor(Color colorIn)
        {
            return new Color((byte)~colorIn.R, (byte)~colorIn.G, (byte)~colorIn.B);
        }

        public void GoTo(Vector2 destPosition, bool teleport, bool pressedKey, bool sprint)
        {
            float movingSpeed = 4.0f;

            if (this.TacticalPosition == TacticalPosition.defender) movingSpeed = 4.5f;
            else if (this.TacticalPosition == TacticalPosition.midfielder) movingSpeed = 4.2f;
            else if (this.TacticalPosition == TacticalPosition.goalkeeper)  //according to it's GK skill
            {
                movingSpeed = (float)(goalkeeping * 6.0f / 100.0f); //6.0 = max goalkeeper speed
            }
            else if (this.TacticalPosition == TacticalPosition.striker) movingSpeed = 3.9f;
            if (sprint)
                movingSpeed = 5.0f;
            if (teleport)
            {
                Position = destPosition;
                if(IsAnimated) IsAnimated = false;
                return;
            }
            else if (Math.Abs(Position.X - destPosition.X) > movingSpeed || Math.Abs(Position.Y - destPosition.Y) > movingSpeed)
            {
                if (DestAnimation != destPosition && !pressedKey)
                {
                    DestAnimation = destPosition;
                    IsAnimated = true;
                }
                Vector2 dest = destPosition - Position;
                dest.Normalize();
                Position += (dest * movingSpeed);
            }
            else
            {
                Position = destPosition;
                if(IsAnimated) IsAnimated = false;   //destination reached
            }
        }
        
        public void Pressure()
        {
            Player helper = ((Game)Game).Match.Ball.GetClosestPlayer(Team,this);    //closest cpu controlled player selected to help
            helper.GoTo(((Game)Game).Match.Ball.Position, false, true, false);
            if (!helper.IsPressedAnimation) 
                helper.IsPressedAnimation = true;
        }

        public int CompareTo(object obj)
        {
            if (obj is Player)
            {
                Player temp = (Player)obj;
                return ((float)Math.Sqrt(Math.Pow((Position.X - ((Game)Game).Match.Ball.Position.X), 2) + Math.Pow((Position.Y - ((Game)Game).Match.Ball.Position.Y), 2))).CompareTo(((float)Math.Sqrt(Math.Pow((temp.Position.X - ((Game)Game).Match.Ball.Position.X), 2) + Math.Pow((temp.Position.Y - ((Game)Game).Match.Ball.Position.Y), 2))));
            }

            throw new ArgumentException("object is not a Player");
        }

        public double CompareToExact(object obj)
        {
            if (obj is Player)
            {
                Player temp = (Player)obj;
                double playerDistance = Math.Sqrt(Math.Pow((Position.X - ((Game)Game).Match.Ball.Position.X), 2) + Math.Pow((Position.Y - ((Game)Game).Match.Ball.Position.Y), 2));
                double tempDistance = Math.Sqrt(Math.Pow((temp.Position.X - ((Game)Game).Match.Ball.Position.X), 2) + Math.Pow((temp.Position.Y - ((Game)Game).Match.Ball.Position.Y), 2));
                return playerDistance - tempDistance;
            }

            throw new ArgumentException("object is not a Player");
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                if (!FreeKick || IsAnimated)
                    position = value;
            }
        }


        
    }
}
