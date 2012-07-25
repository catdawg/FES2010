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
using System.Collections;


namespace FES2010
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TeamAI : Microsoft.Xna.Framework.GameComponent
    {
        public Team Team{get; set;}
        public ArrayList Players { get; set; }
        public bool IsPassing = false;
        public AIPlayer PlayerPassing { get; set; }
        public TeamAI(Game game, Team team)
            : base(game)
        {
            Team = team;
        }
        public AIPlayer getPlayer(Player player)
        {
            foreach(AIPlayer p in Players)
            {
                if (p.Player == player)
                    return p;
            }
            return null;

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

       

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (
                (((Game)Game).Match.Player1.HasBall && ((Game)Game).Match.Player1.Team != Team)
                ||
                (((Game)Game).Match.Player2.HasBall && ((Game)Game).Match.Player2.Team != Team)
                )
                IsPassing = false;
            foreach (AIPlayer p in Players)
            {

                p.Update(gameTime);
            }
            base.Update(gameTime);
        }

        public void LoadContent()
        {
            Players = new ArrayList();
            foreach (Player p in Team.Players)
            {
                Players.Add(new AIPlayer((Game)Game, p));
            }
        }

        public void UnloadContent()
        {
            Team = null;

            for (int i = 0; i != Players.Count; i++)
                Players[i] = null;

            Players.Clear();

            PlayerPassing = null;
        }

        public enum AIPlayerState { MoveUpField, TryToShoot, TryToPass, OffensiveWander, DefensiveWander, TryToGetPass, TryToGetBall, ReceivingPass }

        public class AIPlayer : Microsoft.Xna.Framework.GameComponent
        {

            public AIPlayerState AIPlayerState { get; set; }
            public Player Player { get; set; }
            public AIPlayer(Game game, Player player)
                : base(game)
            {
                Player = player;
            }
            private bool isEscaping = false;
            private bool isEscapingToTheLeft = false;
            private double passWaitTime = 0;
            private bool isPassing = false;
            public bool IsReceivingPass {get; set; }
            public int OffsetForMovingUp { get; set; }
            public AIPlayer PlayerPassing { get; set; }

            public void StateChanger(GameTime gameTime)
            {
                if (((Game)Game).Match.Ball.FreeKick || ((Game)Game).Match.KickOff)
                {
                    AIPlayerState = AIPlayerState.DefensiveWander;
                    if (!isPassing)
                    {
                        isPassing = true;
                        passWaitTime = 1000f;
                    }
                    else
                    {
                        passWaitTime -= gameTime.ElapsedGameTime.Milliseconds;
                        if (passWaitTime < 0)
                        {
                            isPassing = false;
                            Player.Pass(gameTime);
                        }
                    }
                    return;
                }
                if (
                    (((Game)Game).Match.Player1.HasBall && (((Game)Game).Match.Player1.TacticalPosition == TacticalPosition.goalkeeper)
                    && ((Game)Game).Match.Player1.Position.Y > ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.BoxHeight)
                    ||
                    (((Game)Game).Match.Player2.HasBall && (((Game)Game).Match.Player2.TacticalPosition == TacticalPosition.goalkeeper)
                    && ((Game)Game).Match.Player2.Position.Y < ((Game)Game).Match.Field.Measures.Top + ((Game)Game).Match.Field.Measures.BoxHeight)
                  )
                {
                    AIPlayerState = AIPlayerState.DefensiveWander;
                    return;
                }
                    
                if (Player.HasBall)
                {
                    
                    if (IsGoalInRange())
                    {
                        AIPlayerState = AIPlayerState.TryToShoot;
                        return;
                    }
                    
                    Player p = IncomingEnemy();
                    if (p != null)
                    {
                        AIPlayerState = AIPlayerState.TryToPass;
                        return;
                    }
                    AIPlayerState = AIPlayerState.MoveUpField;
                    return;
                }
                else
                {
                    if (IsReceivingPass == true && Player.Team.AI.IsPassing == false)
                        IsReceivingPass = false;
                    else if (Player.Team.AI.IsPassing && IsReceivingPass && PlayerPassing == Player.Team.AI.PlayerPassing)
                    {
                        AIPlayerState = AIPlayerState.ReceivingPass;
                        return;
                    }
                    //Team Has The Ball
                    if (
                        (((Player.Team == ((Game)Game).Match.Player1.Team)) && ((Game)Game).Match.Player1.HasBall)
                        ||
                        (((Player.Team == ((Game)Game).Match.Player2.Team)) && ((Game)Game).Match.Player2.HasBall)
                        ||
                        Player.Team.AI.IsPassing
                        )
                    {
                        AIPlayerState = AIPlayerState.OffensiveWander;
                        if (ShouldTryToGetPass())
                            AIPlayerState = AIPlayerState.TryToGetPass; 
                    }
                    else
                    {
                        AIPlayerState = AIPlayerState.DefensiveWander;
                        if (ShouldTryToGetBall())
                            AIPlayerState = AIPlayerState.TryToGetBall;
                    }
                }

            }

            private bool ShouldTryToGetBall()
            {
                bool isAhead = false;
                if (
                        ((Player.Team.IsHomeTeam) && (Player.Position.Y + 50 >= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!Player.Team.IsHomeTeam) && (Player.Position.Y - 50 <= ((Game)Game).Match.Ball.Position.Y)))
                    isAhead = true;
                int playerscloserandbehind = 0;
                int playersfartherandahead = 0;
                int playerscloserandahead = 0;
                int playersfartherandbehind = 0;

                foreach (Player p in Player.Team.Players)
                {
                    
                    if (Player == p)
                        continue;
                    if (p.TacticalPosition == TacticalPosition.goalkeeper)
                        continue;

                    if (Player.CompareTo(p) >= 0)
                    {

                        if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y + 50 >= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y - 50<= ((Game)Game).Match.Ball.Position.Y)))
                            playerscloserandahead++;
                        else
                            playerscloserandbehind++;
                         
                    }
                        
                    else
                    {
                        if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y + 50 >= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y - 50<= ((Game)Game).Match.Ball.Position.Y)))
                            playersfartherandahead++;
                        else
                            playersfartherandbehind++;
                    }
                    

                        
                }
                if (isAhead && playerscloserandahead >= 1)
                    return false;
                if (isAhead)
                    return true;
                if (playerscloserandahead >= 1)
                    return false;
                if (playerscloserandbehind >= 1)
                    return false;
                if (playersfartherandahead >= 1)
                    return false;
                return true;
            }

            private bool ShouldTryToGetPass()
            {
                bool isAhead = false;
                if (
                        ((Player.Team.IsHomeTeam) && (Player.Position.Y - 50 <= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!Player.Team.IsHomeTeam) && (Player.Position.Y + 50 >= ((Game)Game).Match.Ball.Position.Y)))
                    isAhead = true;
                int playerscloserandbehind = 0;
                int playersfartherandahead = 0;
                int playerscloserandahead = 0;
                int playersfartherandbehind = 0;

                foreach (Player p in Player.Team.Players)
                {

                    if (Player == p)
                        continue;

                    if (p.TacticalPosition == TacticalPosition.goalkeeper)
                        continue;
                    if (Player.CompareTo(p) >= 0)
                    {

                        if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y - 50 <= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y + 50 >= ((Game)Game).Match.Ball.Position.Y)))
                            playerscloserandahead++;
                        else
                            playerscloserandbehind++;

                    }

                    else
                    {
                        if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y - 50 <= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y + 50 >= ((Game)Game).Match.Ball.Position.Y)))
                            playersfartherandahead++;
                        else
                            playersfartherandbehind++;
                    }



                }
                if (isAhead && playerscloserandahead >= 1)
                    return false;
                if (isAhead)
                    return true;
                if (playerscloserandahead >= 1)
                    return false;
                if (playerscloserandbehind >= 1)
                    return false;
                if (playersfartherandahead >= 1)
                    return false;
                return true;
            }

            private Player IncomingEnemy()
            {
                //TODO have in account the running speed of the player and of the pursuing players, 
                //using just a fixed distance to determine if the enemy is incoming
                int distance = 100;

                //get enemy Players within Range and In Front
                return GetEnemyPlayerInFrontAndWithinRange(distance);
                

            }
            private Player GetEnemyPlayerInFrontAndWithinRange(int distance)
            {
                ArrayList enemyPlayers = new ArrayList();
                if (Player.Team.Equals(((Game)Game).Match.AwayTeam))
                    enemyPlayers = ((Game)Game).Match.HomeTeam.Players;
                else
                {
                    enemyPlayers = ((Game)Game).Match.AwayTeam.Players;
                }
                foreach (Player p in enemyPlayers)
                {
                    if (p.CompareToExact(Player) < distance)
                        if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y >= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y <= ((Game)Game).Match.Ball.Position.Y)))
                            return p;
                }
                return null;
            }
            private ArrayList GetEnemyPlayersInFrontAndWithinRange(int distance)
            {
                ArrayList enemyPlayers = new ArrayList();
                if (Player.Team.Equals(((Game)Game).Match.AwayTeam))
                    enemyPlayers = ((Game)Game).Match.HomeTeam.Players;
                else
                {
                    enemyPlayers = ((Game)Game).Match.AwayTeam.Players;
                }
                ArrayList enemyPlayersInFrontAndInRange = new ArrayList();
                foreach (Player p in enemyPlayers)
                {
                    if (p.CompareToExact(Player) > distance)
                        if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y >= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y <= ((Game)Game).Match.Ball.Position.Y)))
                            enemyPlayersInFrontAndInRange.Add(p);
                }
                return enemyPlayersInFrontAndInRange;
            }
            private ArrayList GetEnemyPlayersWithinRange(int distance)
            {
                ArrayList enemyPlayers = new ArrayList();
                if (Player.Team.Equals(((Game)Game).Match.AwayTeam))
                    enemyPlayers = ((Game)Game).Match.HomeTeam.Players;
                else
                {
                    enemyPlayers = ((Game)Game).Match.AwayTeam.Players;
                }
                ArrayList enemyPlayersInRange = new ArrayList();
                foreach (Player p in enemyPlayers)
                {
                    double distanceToP = p.CompareToExact(Player);
                    if (distanceToP < distance)
                            enemyPlayersInRange.Add(p);
                }
                return enemyPlayersInRange;
            }
            private bool IsGoalInRange()
            {
                //TODO determine by shot power, using fixed distance
                float shot = 400;
                Vector2 goal;
                if (Player.Team.IsHomeTeam)
                    goal = new Vector2(0, ((Game)Game).Match.Field.Measures.Top);
                else
                {
                    goal = new Vector2(0, ((Game)Game).Match.Field.Measures.Bottom);
                }
                float distance = (float)Math.Sqrt(Math.Pow((Player.Position.X - goal.X), 2) + Math.Pow((Player.Position.Y - goal.Y), 2));
                if (shot > distance)
                    return true;
                return false;
             
            }
            public void StateAction(GameTime gameTime)
            {
                switch(AIPlayerState)
                {
                    case AIPlayerState.MoveUpField:
                        MoveUpField(gameTime);
                        break;
                    case AIPlayerState.TryToPass:
                        TryToPass(gameTime);
                        break;
                    case AIPlayerState.TryToShoot:
                        TryToShoot(gameTime);
                        break;
                    case AIPlayerState.OffensiveWander:
                        OffensiveWander(gameTime);
                        break;
                    case AIPlayerState.DefensiveWander:
                        DefensiveWander(gameTime);
                        break;
                    case AIPlayerState.TryToGetPass:
                        TryToGetPass(gameTime);
                        break;
                    case AIPlayerState.TryToGetBall:
                        TryToGetBall(gameTime);
                        break;
                    case AIPlayerState.ReceivingPass:
                        ReceivePass(gameTime);
                        break;
                }
                

            }

            private void ReceivePass(GameTime gameTime)
            {
                Player.GoTo(((Game)Game).Match.Ball.Position, false, true, false);
            }

            private void TryToGetBall(GameTime gameTime)
            {
                

                Player.GoTo(((Game)Game).Match.Ball.Position, false, true, false);
            }

            private void TryToGetPass(GameTime gameTime)
            {
                //TODO
                if(Player.Team.AI.IsPassing)
                    Player.GoTo(((Game)Game).Match.Ball.Position, false, true, false);
                else OffensiveWander(gameTime);
            }


            private void DefensiveWander(GameTime gameTime)
            {
                // TODO Get In Front 

                float X = 0;
                float Y = 0;
                float lowerLimit, upperLimit, ballPosField;
                float ballPosY = ((Game)Game).Match.Ball.Position.Y;
                int pos = (int)Player.TacticalPosition;
                if (((Game)Game).Match.HomeTeam.Equals(this.Player.Team))
                {
                    pos = (int)Player.TacticalPosition;
                    lowerLimit = ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.Positions[pos] / 2.0f + ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                    upperLimit = ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.Positions[pos] / 1.0f - ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                    ballPosField = (((Game)Game).Match.Field.Measures.Bottom - ballPosY) / ((Game)Game).Match.Field.Measures.FieldHeight;
                }
                else
                {
                    pos = (int)Player.TacticalPosition;
                    upperLimit = ((Game)Game).Match.Field.Measures.Top + ((Game)Game).Match.Field.Measures.Positions[pos] / 2.0f - ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                    lowerLimit = ((Game)Game).Match.Field.Measures.Top + ((Game)Game).Match.Field.Measures.Positions[pos] / 1.0f + ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                    ballPosField = (((Game)Game).Match.Field.Measures.Top - ballPosY) / ((Game)Game).Match.Field.Measures.FieldHeight;
                }

                float fullStep = upperLimit - lowerLimit;

                
                // TODO could be optimized: player saves this chunk of variables
                // TODO correct sides
                switch (Player.TacticalPosition)
                {
                    case TacticalPosition.defender:
                    case TacticalPosition.dfmidfielder:
                    case TacticalPosition.midfielder:
                        // defender, midfielder goes from lower limit position to upper limit position
                        if (Player.Team == ((Game)Game).Match.HomeTeam)
                        {
                            X = Player.InitPositionX;
                            Y = Math.Max(lowerLimit + fullStep * ballPosField, upperLimit);
                        }
                        else
                        {
                            X = Player.InitPositionX;
                            Y = Math.Min(upperLimit + fullStep * ballPosField, lowerLimit);
                        }
                        break;
                    case TacticalPosition.ofmidfielder:
                        // ofmidfielder goes a little bit farther
                        if (Player.Team == ((Game)Game).Match.HomeTeam)
                        {
                            X = Player.InitPositionX;
                            Y = Math.Max(lowerLimit + fullStep * ballPosField * 1.2f, upperLimit * 1.2f);
                        }
                        else
                        {
                            X = Player.InitPositionX;
                            Y = Math.Min(upperLimit + fullStep * ballPosField * 1.2f, lowerLimit * 1.2f);
                        }
                        break;
                    case TacticalPosition.striker:
                        // striker goes from lower limit attacking position to almost the final line

                        if (Player.Team == ((Game)Game).Match.HomeTeam)
                            upperLimit = ((Game)Game).Match.Field.Measures.Top + ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                        else
                            lowerLimit = ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;

                        fullStep = upperLimit - lowerLimit;

                        if (Player.Team == ((Game)Game).Match.HomeTeam)
                        {
                            X = Player.InitPositionX;
                            Y = Math.Max(lowerLimit + fullStep * ballPosField, upperLimit);
                        }
                        else
                        {
                            X = Player.InitPositionX;
                            Y = Math.Min(upperLimit + fullStep * ballPosField, lowerLimit);
                        }
                        break;
                }
                if (
                    (
                        ((Game)Game).Match.Player1.HasBall
                        &&
                        (
                            ((Game)Game).Match.Player1.TacticalPosition == TacticalPosition.goalkeeper
                        )
                    )
                    ||

                    (
                        ((Game)Game).Match.Player2.HasBall
                        &&
                        (
                            ((Game)Game).Match.Player2.TacticalPosition == TacticalPosition.goalkeeper
                        )
                    )
                 )
                {
                    if (Math.Abs(Y) > (((Game)Game).Match.Field.Height - 500))
                    {
                        if( Y > 0)
                         Y = ((Game)Game).Match.Field.Height - 500;
                        else
                         Y = -(((Game)Game).Match.Field.Height - 500);
                    }
                }
                        
                 

                //get in front of nearest player
                ArrayList enemyPlayers = new ArrayList();
                if (Player.Team.Equals(((Game)Game).Match.AwayTeam))
                    enemyPlayers = ((Game)Game).Match.HomeTeam.Players;
                else
                {
                    enemyPlayers = ((Game)Game).Match.AwayTeam.Players;
                }
                
                Player closerPlayer = null;
                double distance = double.MaxValue;
                foreach(Player e in enemyPlayers)
                {
                    if (e.TacticalPosition == TacticalPosition.goalkeeper)
                        continue;
                    double distanceTemp = Math.Sqrt(Math.Pow((X - e.Position.X), 2) + Math.Pow((Y - e.Position.Y), 2));
                    if(distanceTemp < distance)
                    {
                        distance = distanceTemp;
                        closerPlayer = e;
                    }
                }


                if (closerPlayer != null && distance < 300 && 
                        (
                            (
                                ((Game)Game).Match.Player1.HasBall 
                                &&
                                (
                                    ((Game)Game).Match.Player1.TacticalPosition != TacticalPosition.goalkeeper
                                )
                            )
                            ||
     
                            (
                                ((Game)Game).Match.Player2.HasBall 
                                &&
                                (
                                    ((Game)Game).Match.Player2.TacticalPosition != TacticalPosition.goalkeeper
                                )
                            )
                        )
                    )
                {
                    //getting in front
                    Vector2 vec = new Vector2(((Game)Game).Match.Ball.Position.X - closerPlayer.Position.X, ((Game)Game).Match.Ball.Position.Y - closerPlayer.Position.Y);
                    vec.Normalize();
                    vec *= 50;
                    Player.GoTo(new Vector2(closerPlayer.Position.X + vec.X, closerPlayer.Position.Y + vec.Y), false, true, false);
                }
                else
                {
                    Player.GoTo(new Vector2(X, Y), false, true, false);
                }


            }
            

            private void OffensiveWander(GameTime gameTime)
            {
                float X = 0;
                float Y = 0;
                float lowerLimit, upperLimit, ballPosField;
                float ballPosY = ((Game)Game).Match.Ball.Position.Y;
                int pos = (int)Player.TacticalPosition;
                if (((Game)Game).Match.HomeTeam.Equals(this.Player.Team))
                {
                    pos = (int)Player.TacticalPosition;
                    lowerLimit = ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.Positions[pos] / 2.0f + ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                    upperLimit = ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.Positions[pos] / 1.0f - ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                    ballPosField = (((Game)Game).Match.Field.Measures.Bottom - ballPosY) / ((Game)Game).Match.Field.Measures.FieldHeight;
                }
                else
                {
                    pos = (int)Player.TacticalPosition;
                    upperLimit = ((Game)Game).Match.Field.Measures.Top + ((Game)Game).Match.Field.Measures.Positions[pos] / 2.0f - ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                    lowerLimit = ((Game)Game).Match.Field.Measures.Top + ((Game)Game).Match.Field.Measures.Positions[pos] / 1.0f + ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                    ballPosField = (((Game)Game).Match.Field.Measures.Top - ballPosY) / ((Game)Game).Match.Field.Measures.FieldHeight;
                }

                float fullStep = upperLimit - lowerLimit;


                // TODO could be optimized: player saves this chunk of variables
                // TODO correct sides
                switch (Player.TacticalPosition)
                {
                    case TacticalPosition.defender:
                    case TacticalPosition.dfmidfielder:
                    case TacticalPosition.midfielder:
                        // defender, midfielder goes from lower limit position to upper limit position
                        if (Player.Team == ((Game)Game).Match.HomeTeam)
                        {
                            X = Player.InitPositionX;
                            Y = Math.Max(lowerLimit + fullStep * ballPosField, upperLimit);
                        }
                        else
                        {
                            X = Player.InitPositionX;
                            Y = Math.Min(upperLimit + fullStep * ballPosField, lowerLimit);
                        }
                        break;
                    case TacticalPosition.ofmidfielder:
                        // ofmidfielder goes a little bit farther
                        if (Player.Team == ((Game)Game).Match.HomeTeam)
                        {
                            X = Player.InitPositionX;
                            Y = Math.Max(lowerLimit + fullStep * ballPosField * 1.4f, upperLimit * 1.4f);
                        }
                        else
                        {
                            X = Player.InitPositionX;
                            Y = Math.Min(upperLimit + fullStep * ballPosField * 1.4f, lowerLimit * 1.4f);
                        }
                        break;
                    case TacticalPosition.striker:
                        // striker goes from lower limit attacking position to almost the final line

                        if (Player.Team == ((Game)Game).Match.HomeTeam)
                            upperLimit = ((Game)Game).Match.Field.Measures.Top + ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;
                        else
                            lowerLimit = ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.FieldHeight / 40.0f;

                        fullStep = upperLimit - lowerLimit;

                        if (Player.Team == ((Game)Game).Match.HomeTeam)
                        {
                            X = Player.InitPositionX;
                            Y = Math.Max(lowerLimit + fullStep * ballPosField, upperLimit);
                        }
                        else
                        {
                            X = Player.InitPositionX;
                            Y = Math.Min(upperLimit + fullStep * ballPosField, lowerLimit);
                        }
                        break;
                }
                // Sprint to receive the ball
                if ((Player.TacticalPosition == TacticalPosition.striker || Player.TacticalPosition == TacticalPosition.ofmidfielder) 
                    &&
                    (Math.Sqrt(Math.Pow((X - ((Game)Game).Match.Ball.Position.X), 2) + Math.Pow((Y - ((Game)Game).Match.Ball.Position.Y), 2)) < 300)
                   )
                {
                    if (!isEscaping)
                    {
                        if ((Player.Position.X > ((Game)Game).Match.Ball.Position.X))
                        {
                            int val = 400;
                            if (Math.Abs(X - 400) > ((Game)Game).Match.Field.Width)
                                val = ((Game)Game).Match.Field.Width - (int)Math.Abs(X);
                            Player.GoTo(new Vector2(X - val, Y), false, true, true);
                            isEscaping = true;
                            isEscapingToTheLeft = true;
                        }
                        else
                        {
                            int val = 400;
                            if (Math.Abs(X + 400) > ((Game)Game).Match.Field.Width)
                                val = ((Game)Game).Match.Field.Width - (int)Math.Abs(X);
                            isEscaping = true;
                            Player.GoTo(new Vector2(X + val, Y), false, true, true);
                            isEscapingToTheLeft = false;
                        }
                    }
                    else
                    {
                        if (isEscapingToTheLeft)
                        {
                            int val = 400;
                            if (Math.Abs(X - 400) > ((Game)Game).Match.Field.Width)
                                val = ((Game)Game).Match.Field.Width - (int)Math.Abs(X);
                            Player.GoTo(new Vector2(X - val, Y), false, true, true);
                        }
                        else
                        {
                            int val = 400;
                            if (Math.Abs(X + 400) > ((Game)Game).Match.Field.Width)
                                val = ((Game)Game).Match.Field.Width - (int)Math.Abs(X);
                            Player.GoTo(new Vector2(X + val, Y), false, true, true);
                        }

                    }

                    
                }
                else
                {
                    isEscaping = false;
                    Player.GoTo(new Vector2(X, Y), false, true, false);
                }

            }

            private void TryToShoot(GameTime gameTime)
            {
                
                //determine shot possible
                //TODO use shot calculation
                int distance = 100;
                if (CanShoot(distance))
                    //shoot
                    Player.Shoot(gameTime, true);
                else
                    TryToPass(gameTime);

            }

            private bool CanShoot(int distance)
            {

                //TODO check distance, using fixed var
                int tunnelDistance = 40;

                Vector2 goal;
                if (Player.Team.IsHomeTeam)
                    goal = new Vector2(0, ((Game)Game).Match.Field.Measures.Top);
                else
                {
                    goal = new Vector2(0, ((Game)Game).Match.Field.Measures.Bottom);
                }
                
                ArrayList withinRange = new ArrayList();

                //get Players within Range
                foreach (Player p in Player.Team.Players)
                {
                    if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y <= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y >= ((Game)Game).Match.Ball.Position.Y)))
                        withinRange.Add(p);
                }
                ArrayList enemyPlayers;
                if(Player.Team.IsHomeTeam)
                    enemyPlayers = ((Game)Game).Match.AwayTeam.Players;
                else
                    enemyPlayers = ((Game)Game).Match.HomeTeam.Players;
                foreach(Player p in enemyPlayers)
                {
                    if (p.TacticalPosition.Equals(TacticalPosition.goalkeeper))
                        continue;
               
                    if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y >= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y <= ((Game)Game).Match.Ball.Position.Y)))
                        withinRange.Add(p);
                }

                float a = (goal.Y - ((Game)Game).Match.Ball.Position.Y)/(goal.X - ((Game)Game).Match.Ball.Position.X);
                float b = -1;
                float c = ((Game)Game).Match.Ball.Position.Y - ((Game)Game).Match.Ball.Position.X * (goal.Y - ((Game)Game).Match.Ball.Position.Y) / (goal.X - ((Game)Game).Match.Ball.Position.X);
                foreach (Player p in withinRange)
                {
                    double actualTunnelDistance = (
                        Math.Abs(a * p.Position.X + b * p.Position.Y + c)
                        /
                        Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2))
                        );
                    if (
                        actualTunnelDistance
                        <
                        tunnelDistance
                       )
                    {
                        return false;
                    }
                }
                return true;
            }

            private void TryToPass(GameTime gameTime)
            {
                
                Player playerToPass = FindFriendlyPlayerWithLoS();
                if (playerToPass == null)
                {
                 
                    MoveAway(gameTime);
                    return;
                
                }
                Player.Team.AI.IsPassing = true;
                Player.Team.AI.PlayerPassing = this;
                AIPlayer destinationAI = Player.Team.AI.getPlayer(playerToPass);
                destinationAI.IsReceivingPass = true;
                destinationAI.PlayerPassing = this;
                Player.Pass(gameTime, playerToPass);

                 
            }

            private void MoveAway(GameTime gameTime)
            {
                //find closest EnemyPlayer
                ArrayList enemies = GetEnemyPlayersWithinRange(300);
                ArrayList vectors = new ArrayList();
                Vector2 goal;
                if (Player.Team.IsHomeTeam)
                    goal = new Vector2(OffsetForMovingUp, ((Game)Game).Match.Field.Measures.Top);
                else
                {
                    goal = new Vector2(OffsetForMovingUp, ((Game)Game).Match.Field.Measures.Bottom);
                }
                Vector2 toGoal = goal - Player.Position;
                if (Player.Position.X == OffsetForMovingUp)
                    OffsetForMovingUp = ((Game)Game).Match.Rand.Next(400) - 800;
                toGoal.Normalize();
                vectors.Add(toGoal);

                double angleWithGoalVec1;
                double angleWithGoalVec2;
                double angleWithGoalVec3;

                foreach (Player p in enemies)
                {
                    Vector2 vec1 = new Vector2(-p.Position.Y, p.Position.X); vec1.Normalize();
                    Vector2 vec2 = new Vector2(-p.Position.X, -p.Position.Y); vec2.Normalize();
                    Vector2 vec3 = new Vector2(p.Position.Y, -p.Position.X); vec3.Normalize();

                    angleWithGoalVec1 = Math.Acos(vec1.X * toGoal.X + vec1.Y * toGoal.Y);
                    if(angleWithGoalVec1 > Math.PI)
                        angleWithGoalVec1 = Math.Abs(angleWithGoalVec1 - 2*Math.PI);
                    angleWithGoalVec2 = Math.Acos(vec2.X * toGoal.X + vec2.Y * toGoal.Y);
                    if(angleWithGoalVec2 > Math.PI)
                        angleWithGoalVec2 = Math.Abs(angleWithGoalVec1 - 2*Math.PI);
                    angleWithGoalVec3 = Math.Acos(vec3.X * toGoal.X + vec3.Y * toGoal.Y);
                    if(angleWithGoalVec3 > Math.PI)
                        angleWithGoalVec3 = Math.Abs(angleWithGoalVec1 - 2*Math.PI);


                    if(angleWithGoalVec1 <= angleWithGoalVec2 && angleWithGoalVec1 <= angleWithGoalVec3)
                        vectors.Add(vec1);
                    else if (angleWithGoalVec2 <= angleWithGoalVec1 && angleWithGoalVec2 <= angleWithGoalVec3)
                        vectors.Add(vec2);
                    else if (angleWithGoalVec3 <= angleWithGoalVec1 && angleWithGoalVec3 <= angleWithGoalVec2)
                    vectors.Add(vec3);
                }
                Vector2 sum = new Vector2(0,0);
                foreach (Vector2 v in vectors)
                {
                    sum += v;
                }
                sum.Normalize();
                double angle = Math.Acos(sum.X);
                double temp = 2.0f*Math.PI/8.0f;
                if (angle < temp/2 || angle >= 8 * temp - temp/2)
                {
                    Player.Movement(gameTime, new Vector2(1,0));
                    return;
                }
                if (angle >= temp/2 && angle < 2 * temp - temp/2)
                {
                    Player.Movement(gameTime, new Vector2(1, 1));
                    return;
                }
                if (angle >= 2*temp - temp/2 && angle < 3 * temp - temp/2)
                {

                    Player.Movement(gameTime, new Vector2(0, 1));
                    return;
                }
                if (angle >= 3 * temp - temp/2 && angle < 4 * temp - temp/2)
                {

                    Player.Movement(gameTime, new Vector2(-1, 1));
                    return;
                }
                if (angle >= 4 * temp - temp/2 && angle < 5 * temp - temp/2)
                {
                    Player.Movement(gameTime, new Vector2(-1, 0));
                    return;
                }
                if (angle >= 5 * temp - temp/2&& angle < 6 * temp - temp/2)
                {


                    Player.Movement(gameTime, new Vector2(-1, -1));
                    return;
                }
                if (angle >= 6 * temp - temp/2&& angle < 7 * temp - temp/2)
                {

                    Player.Movement(gameTime, new Vector2(0, -1));
                    return;
                }
                if (angle >= 7 * temp - temp/2 && angle < 8 * temp - temp/2)
                {

                    Player.Movement(gameTime, new Vector2(1, -1));
                    return;
                }  

                
                
                
            }

            private void MoveUpField(GameTime gameTime)
            {
                
                Player destination = FindFriendlyPlayerInFrontWithLoS();
                if (destination == null || destination.TacticalPosition == TacticalPosition.goalkeeper || Player.TacticalPosition == TacticalPosition.ofmidfielder)
                {

                    MoveAway(gameTime);
                    /*
                    if(Player == ((Game)Game).Match.Player1)
                        Player.Movement(gameTime, new Vector2(0,-1));
                    else
                        Player.Movement(gameTime, new Vector2(0, 1));
                     */
                    
	            
                }
                else
                {
                    //MoveAway(gameTime);
                    Player.Team.AI.IsPassing = true;
                    Player.Team.AI.PlayerPassing = this;
                    AIPlayer destinationAI = Player.Team.AI.getPlayer(destination);
                    destinationAI.IsReceivingPass = true;
                    destinationAI.PlayerPassing = this;
                    Player.Pass(gameTime, destination);
                }
                
                return;
            }
            private Player FindFriendlyPlayerWithLoS()
            {
                //TODO check distance, using fixed var

                int distance = 500;
                int tunnelDistance = 40;
                ArrayList withinRange = new ArrayList();

                //get Players within Range
                foreach (Player p in Player.Team.Players)
                {
                    if (p == Player)
                        continue;
                    if (p.CompareToExact(Player) > 100 && p.CompareToExact(Player) < distance)
                          withinRange.Add(p);
                }
                if (withinRange.Count == 0)
                    return null;
                withinRange.Sort();
                ArrayList withinRangeInFront = new ArrayList();
                ArrayList withinRangeInTheBack = new ArrayList();
                foreach (Player p in withinRange)
                {
                    if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y <= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y >= ((Game)Game).Match.Ball.Position.Y)))
                        withinRangeInFront.Add(p);
                    else
                        withinRangeInTheBack.Add(p);
                }
                foreach (Player p in withinRangeInTheBack)
                {
                    withinRangeInFront.Add(p);
                }
                withinRange = withinRangeInFront;

                //get enemy Players within Range
                ArrayList enemyPlayers = GetEnemyPlayersWithinRange(distance);


                //find enemy player in the middle
                foreach (Player p in withinRange)
                {
                    float a = (p.Position.Y - ((Game)Game).Match.Ball.Position.Y) / (p.Position.X - ((Game)Game).Match.Ball.Position.X);
                    float b = -1;
                    float c = ((Game)Game).Match.Ball.Position.Y - ((Game)Game).Match.Ball.Position.X * (p.Position.Y - ((Game)Game).Match.Ball.Position.Y) / (p.Position.X - ((Game)Game).Match.Ball.Position.X);

                    bool found = false;
                    foreach (Player e in enemyPlayers)
                    {
                        double actualTunnelDistance = (
                        Math.Abs(a * e.Position.X + b * e.Position.Y + c)
                        /
                        Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2))
                        );
                        if (
                            actualTunnelDistance
                            <
                            tunnelDistance
                           )
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        return p;
                }
                return null;
            }

            
            private Player FindFriendlyPlayerInFrontWithLoS()
            {

                //TODO check distance, using fixed var
                
                int distance = 1000;
                int tunnelDistance = 40;
                ArrayList withinRange = new ArrayList();
                
                //get Players within Range
                foreach (Player p in Player.Team.Players)
                {
                    if (p == Player)
                        continue;
                    if (p.CompareToExact(Player) > 100 && p.CompareToExact(Player) < distance)
                        if (
                        ((p.Team.IsHomeTeam) && (p.Position.Y  <= ((Game)Game).Match.Ball.Position.Y))
                        ||
                        ((!p.Team.IsHomeTeam) && (p.Position.Y >= ((Game)Game).Match.Ball.Position.Y)))
                            withinRange.Add(p);
                }
                if(withinRange.Count == 0)
                    return null;
                withinRange.Sort();

                //get enemy Players within Range
                ArrayList enemyPlayers = GetEnemyPlayersInFrontAndWithinRange(distance);

                
                //find enemy player in the middle
                foreach (Player p in withinRange)
                {
                    float a = (p.Position.Y - ((Game)Game).Match.Ball.Position.Y) / (p.Position.X - ((Game)Game).Match.Ball.Position.X);
                    float b = -1;
                    float c = ((Game)Game).Match.Ball.Position.Y - ((Game)Game).Match.Ball.Position.X * (p.Position.Y - ((Game)Game).Match.Ball.Position.Y) / (p.Position.X - ((Game)Game).Match.Ball.Position.X);
                
                    bool found = false;
                    foreach(Player e in enemyPlayers)
                    {
                        double actualTunnelDistance = (
                        Math.Abs(a * e.Position.X + b * e.Position.Y + c)
                        /
                        Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2))
                        );
                        if (
                            actualTunnelDistance
                            <
                            tunnelDistance
                           )
                        {   
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        return p;
                }
                return null;
            }
            public override void Update(GameTime gameTime)
            {
                if (Player.TacticalPosition.Equals(TacticalPosition.goalkeeper))   //controlados pelo computador
                {
                    GK(gameTime);
                }
                else
                {
                    
                    // this player is currently selected or animated, don't move him
                    if (Player.IsPressedAnimation)
                    {
                        Player.IsPressedAnimation = false;
                        return;
                    }
                    if (Player.IsAnimated
                        
                        ||
                        
                        (Player == ((Game)Game).Match.Player1) 
                        
                        ||
                        
                        ((Player == ((Game)Game).Match.Player2) && !((Game)Game).Match.Player2Cpu)
                        
                         
                         )
                        return;
                    StateChanger(gameTime);
                    StateAction(gameTime);


                }
            }

            private void GK(GameTime gameTime)
            {
                if (!Player.Team.IsHomeTeam && ((Game)Game).Match.Player2Cpu)
                {
                    if (((Game)Game).Match.Ball.FreeKick || ((Game)Game).Match.KickOff)
                    {
                        AIPlayerState = AIPlayerState.DefensiveWander;

                        if (!isPassing)
                        {
                            isPassing = true;
                            passWaitTime = 1000f;
                        }
                        else
                        {
                            passWaitTime -= gameTime.ElapsedGameTime.Milliseconds;
                            if (passWaitTime < 0)
                            {
                                isPassing = false;
                                Player.Pass(gameTime);
                            }
                        }
                        return;
                    }
                    else if (Player.HasBall)    //se tiver a bola passa-a ou chuta
                    {
                        if (!isPassing)
                        {
                            isPassing = true;
                            passWaitTime = 600f;
                        }
                        else
                        {
                            passWaitTime -= gameTime.ElapsedGameTime.Milliseconds;
                            if (passWaitTime < 0)
                            {
                                isPassing = false; 
                                if (((Game)Game).Match.Rand.Next(2) > 0)
                                    Player.Pass(gameTime);
                                else Player.Shoot(gameTime, true);
                            }
                        }
                        
                    }
                    else if (!Player.HasBall)   //se nao tiver a bola volta para a baliza
                    {
                        if (Player.Team.IsHomeTeam) Player.Team.GetGK().GoTo(new Vector2(Player.Team.GetGK().Position.X, ((Game)Game).Match.Field.Measures.Bottom - 5), false, false, false);
                        else Player.Team.GetGK().GoTo(new Vector2(Player.Team.GetGK().Position.X, ((Game)Game).Match.Field.Measures.Top + 5), false, false, false);
                    }
                }

                if (Player != ((Game)Game).Match.Player1
                     && ((Player != ((Game)Game).Match.Player2 && !((Game)Game).Match.Player2Cpu))
                     || (((Game)Game).Match.Player2Cpu && !Player.HasBall))
                {
                    //positioning on the goal line according to the opposing player position
                    if ((((Game)Game).Match.Player1.HasBall && ((Game)Game).Match.Player1.Team != Player.Team) //update se a outra equipa tem a bola
                       || (((Game)Game).Match.Player2.HasBall && ((Game)Game).Match.Player2.Team != Player.Team)
                       || (!((Game)Game).Match.Player1.HasBall && !((Game)Game).Match.Player2.HasBall)) //update se nng tem a bola
                    {
                        //there's a shot (and it was not from his own team)
                        if (((Game)Game).Match.Ball.Shot && ((Game)Game).Match.Ball.CurrentPlayer.Team != Player.Team)
                        {
                            //take the GK's reflex time before reacting
                            Player.ReflexTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                            if (Player.ReflexTimer >= Player.ReflexTime)
                            {
                                if (((Game)Game).Match.Ball.Position.X >= ((Game)Game).Match.Field.Measures.GoalStart - 10
                                        && ((Game)Game).Match.Ball.Position.X <= ((Game)Game).Match.Field.Measures.GoalEnd + 10)
                                    Player.GoTo(new Vector2(((Game)Game).Match.Ball.Position.X, Player.Position.Y), false, true, false);
                            }
                        }
                        else if (((Game)Game).Match.Ball.Position.Y <= ((Game)Game).Match.Field.Measures.Top + 60
                            || ((Game)Game).Match.Ball.Position.Y >= ((Game)Game).Match.Field.Measures.Bottom - 60) //bola proxima da baliza, cobrir 
                        {
                            if (((Game)Game).Match.Ball.Position.X >= ((Game)Game).Match.Field.Measures.GoalStart - 10
                                    && ((Game)Game).Match.Ball.Position.X <= ((Game)Game).Match.Field.Measures.GoalEnd + 10)
                                Player.GoTo(new Vector2(((Game)Game).Match.Ball.Position.X, Player.Position.Y), false, true, false);
                            else Player.GoTo(new Vector2(((Game)Game).Match.Ball.Position.X / ((Game)Game).Match.Field.Measures.GoalCoveringFactor, Player.Position.Y), false, true, false);
                        }
                        else if (Math.Abs(((Game)Game).Match.Ball.Position.X - Player.Position.X) > ((Game)Game).Match.Field.Measures.GoalCoveringFactor)
                        {
                            //check if the keeper is not actually pursuing the ball off the posts
                            if (Player.Team.IsHomeTeam && ((Game)Game).CurrentKeyboardState.IsKeyUp(((Game)Game).Cmd1.GoalerOut) ||
                                !Player.Team.IsHomeTeam && ((Game)Game).CurrentKeyboardState.IsKeyUp(((Game)Game).Cmd2.GoalerOut))
                            {
                                if (Player.Position.X < ((Game)Game).Match.Field.Measures.GoalStart //if running back focus on covering the goal
                                    || Player.Position.X > ((Game)Game).Match.Field.Measures.GoalEnd && !((Game)Game).Match.Ball.Shot)
                                    Player.GoTo(new Vector2(0, Player.Position.Y), false, true, false);
                                else Player.GoTo(new Vector2(((Game)Game).Match.Ball.Position.X / ((Game)Game).Match.Field.Measures.GoalCoveringFactor, Player.Position.Y), false, true, false);
                            }
                        }
                    }
                    else if ((((Game)Game).Match.Player1.HasBall && ((Game)Game).Match.Player1.Team == Player.Team) //update se a propria equipa tem a bola
                            || (((Game)Game).Match.Player2.HasBall && ((Game)Game).Match.Player2.Team == Player.Team))
                    {
                        if ((int)Player.Position.X != 0) //if the goalie is not in the middle of the goal head there
                        {
                            Player.GoTo(new Vector2(0, Player.Position.Y), false, true, false);
                        }
                    }

                }
            }
        }

       
    }
}