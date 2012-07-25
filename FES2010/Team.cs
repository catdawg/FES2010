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


namespace FES2010
{
    public class TacticalDisposition
    {
        public int left, right, center;
        public float centerFactor;

        public TacticalDisposition(int left, int right, int center)
        {
            this.left = left;
            this.right = right;
            this.center = center;

            centerFactor = (left + right == 0 ? 1.0f : 0.0f);
        }
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Team : Microsoft.Xna.Framework.GameComponent
    {
        public String Name { get; set; }
        public Color Color { get; set; }
        int[] positions;
        int[] available;
        public ArrayList Players { get; set; }
        public ArrayList PlayersBench { get; set; }
        public bool IsHomeTeam { get; set; }

        public TeamAI AI { get; set; }

        public Team(Game game, String name, Color color)
            : base(game)
        {
            this.Name = name;
            this.Color = color;
            this.positions = new int[6];
            this.available = new int[6];
            Players = new ArrayList();
            PlayersBench = new ArrayList();
            IsHomeTeam = true;
            AI = new TeamAI(game, this);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void LoadContent()
        {
            foreach (Player p in Players)
            {
                p.LoadContent();
            }

            OrganizePlayers(true);
            AI.LoadContent();
        }

        public void UnloadContent()
        {
            for (int i = 0; i != Players.Count; i++)
                Players[i] = null;

            Players.Clear();

            for (int i = 0; i != PlayersBench.Count; i++)
                PlayersBench[i] = null;

            PlayersBench.Clear();

            AI.UnloadContent();
        }

        public void Draw(GameTime gameTime)
        {
            foreach(Player p in Players)
            {
                p.Draw(gameTime);
            }
        }

        public override void Update(GameTime gameTime)
        {

            foreach (Player p in Players)
            {
                p.Update(gameTime);
            }

            AI.Update(gameTime);
            base.Update(gameTime);
        }

        public void ChangeSide()
        {
            foreach (Player p in Players)
            {
                Vector2 temp = p.Position;
                temp.X *= -1.0f; temp.Y *= -1.0f;
                p.Position = new Vector2(temp.X, temp.Y);

                p.InitPositionX *= -1.0f;
            }
        }

        public void SetStrategy(string teamStrategy)
        {
            positions[0] = 1;
            positions[1] = teamStrategy[0] - 48;
            positions[2] = teamStrategy[1] - 48;
            positions[3] = teamStrategy[2] - 48;
            positions[4] = teamStrategy[3] - 48;
            positions[5] = teamStrategy[4] - 48;

            if (positions[1] + positions[2] + positions[3] + positions[4] + positions[5] != 10)
                throw new TypeLoadException("incorrect number of players");
        }

        public void ResetFreeKick()
        {
            foreach (Player p in Players)
            {
                if (p.FreeKick && p.TacticalPosition != TacticalPosition.goalkeeper) p.FreeKick = false;
            }
        }

        //starting the 2nd half the stamina will be 3/4 of the player's max stamina
        public void RefillStamina()
        {
            foreach (Player p in Players)
                p.TopSpeedFactor = 1.6f;
        }

        public void OrganizePlayers(bool start)
        {
            float factor = 1.0f;
            if (start) factor = 2.0f;

            float ofmFactor;
            TacticalDisposition[] slots = new TacticalDisposition[6];

            Buffer.BlockCopy(positions, 0, available, 0, Buffer.ByteLength(positions));

            for (int i = 1; i != 6; i++)
            {
                if (positions[i] != 0)
                {
                    slots[i] = new TacticalDisposition(0, 0, positions[i]);

                    foreach (Player p in Players)
                        if ((int)p.TacticalPosition == i)
                        {
                            if (p.Side != Side.center)
                                slots[i] = new TacticalDisposition(1, 1, positions[i] - 2);
                            break;
                        }
                }
            }

              
            foreach (Player p in Players)
            {
                if (p.FreeKick && p.TacticalPosition != TacticalPosition.goalkeeper) p.FreeKick = false;
                if (p.IsAnimated && p.TacticalPosition != TacticalPosition.goalkeeper) p.IsAnimated = false;
                int position = (int)p.TacticalPosition;
                ofmFactor = (p.TacticalPosition == TacticalPosition.ofmidfielder ? 5.0f : 0.0f);

                if (available[position] > 0)
                {
                    if (p.TacticalPosition != TacticalPosition.goalkeeper)
                    {
                        switch (p.Side)
                        {
                            case Side.left:
                                if (slots[position].left > 0)
                                {
                                    p.Position = new Vector2(
                                        ((Game)Game).Match.Field.Measures.Left + (0.6f / (1.0f + (float)positions[position])) * ((Game)Game).Match.Field.Measures.FieldWidth,
                                        ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.Positions[position] / factor + ((Game)Game).Match.Field.Measures.FieldHeight / (40.0f + ofmFactor));

                                    slots[position].left--; available[position]--;
                                }
                                break;

                            case Side.right:
                                if (slots[position].right > 0)
                                {
                                    p.Position = new Vector2(
                                        ((Game)Game).Match.Field.Measures.Left + ((0.4f + (float)positions[position]) / (1.0f + (float)positions[position])) * ((Game)Game).Match.Field.Measures.FieldWidth,
                                        ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.Positions[position] / factor + ((Game)Game).Match.Field.Measures.FieldHeight / (40.0f + ofmFactor));

                                    slots[position].right--; available[position]--;
                                }
                                break;

                            case Side.center:
                                if (slots[position].center > 0)
                                {
                                    p.Position = new Vector2(
                                        ((Game)Game).Match.Field.Measures.Left + ((1.0f - slots[position].centerFactor + (float)slots[position].center) / (1.0f + (float)positions[position])) * ((Game)Game).Match.Field.Measures.FieldWidth,
                                        ((Game)Game).Match.Field.Measures.Bottom - ((Game)Game).Match.Field.Measures.Positions[position] / factor + ((Game)Game).Match.Field.Measures.FieldHeight / (40.0f + ofmFactor));

                                    slots[position].center--; available[position]--;
                                }
                                break;
                        }
                    }
                    else
                    {
                        p.Position = new Vector2(
                            ((Game)Game).Match.Field.Measures.Left + ((Game)Game).Match.Field.Measures.FieldWidth / 2.0f,
                            ((Game)Game).Match.Field.Measures.Bottom);

                        available[position]--;
                    }

                    p.InitPositionX = p.Position.X;
                }

            }
            if (!IsHomeTeam)
                ChangeSide();
        }

        public Player GetGK()
        {
            foreach (Player p in Players)
            {
                if (p.TacticalPosition.Equals(TacticalPosition.goalkeeper))
                {
                    return p;
                }
            }
            return null;
        }

        public void SelectGK()
        {
            foreach (Player p in Players)
            {
                if (p.TacticalPosition.Equals(TacticalPosition.goalkeeper))
                {
                    if (IsHomeTeam)
                        ((Game)Game).Match.Player1 = p;
                    else ((Game)Game).Match.Player2 = p;
                }
            }
        }

        //returns an array list with this team's currently on screen players
        public ArrayList GetOnScreenPlayers(int max)
        {
            ArrayList onScreen = new ArrayList();

            //if on screen
            Vector2 screenViewX = new Vector2(((Game)Game).Match.Scenario.CameraPosition.X - ((Game)Game).Match.Scenario.ViewWidth / 2f,
                                                ((Game)Game).Match.Scenario.CameraPosition.X + ((Game)Game).Match.Scenario.ViewWidth / 2f);

            Vector2 screenViewY = new Vector2(((Game)Game).Match.Scenario.CameraPosition.Y - ((Game)Game).Match.Scenario.ViewHeight / 2f,
                                                ((Game)Game).Match.Scenario.CameraPosition.Y + ((Game)Game).Match.Scenario.ViewHeight / 2f);

            foreach (Player p in Players)
            {
                if (p.Position.X >= screenViewX.X && p.Position.X <= screenViewX.Y
                    && p.Position.Y >= screenViewY.X && p.Position.Y <= screenViewY.Y)
                {
                    onScreen.Add(p);
                }
            }
            onScreen.Sort();
            return onScreen.Count >= max ? onScreen.GetRange(0, max) : onScreen;
        }

        //returns this team's closest player to the given position
        public Player GetClosestPlayer(Vector2 position, Player ignore)
        {
            Player tmp = null;

            float tempDist = float.MaxValue;
            float newDist = 0f;

            foreach (Player p in Players)
            {
                if (p == ignore)
                    continue;
                newDist = (float)Math.Sqrt(Math.Pow((p.Position.X - position.X), 2) + Math.Pow((p.Position.Y - position.Y), 2));
                if (newDist <= tempDist)
                {
                    tmp = p;
                    tempDist = newDist;
                }
            }
            return tmp;
        }

        //escolhe o jogador para que o jogador tenta passar através de um tunel de cerca de 30 graus na direccao do passe e o tempo que a tecla foi premida
        public Player ChoosePassPlayer(Player origin, float PassForce, Vector2 aim)
        {
            ArrayList eligiblePlayers = new ArrayList();    //players who can receive the pass
            Vector2 areaX;
            Vector2 areaY;

            if(aim.X > 0) areaX = new Vector2(origin.Position.X, -((Game)Game).Match.Field.Measures.Left);
            else if (aim.X < 0) areaX = new Vector2(((Game)Game).Match.Field.Measures.Left, origin.Position.X);
            else areaX = new Vector2(0);

            if(aim.Y > 0) areaY = new Vector2(origin.Position.Y, ((Game)Game).Match.Field.Measures.Bottom);
            else if (aim.Y < 0) areaY = new Vector2(((Game)Game).Match.Field.Measures.Top,origin.Position.Y);
            else areaY = new Vector2(0);

            //ArrayList players = GetOnScreenPlayers(11);

            foreach (Player p in Players)
            {
                if (p == origin) continue;
                if(aim.X != 0)  //para os lados ou diagonais
                {
                    if (aim.Y == 0)  //para um dos lados apenas
                    {
                        if (p.Position.X > areaX.X && p.Position.X <= areaX.Y
                            && p.Position.Y <= origin.Position.Y + (Math.Abs(p.Position.X - origin.Position.X)) * Math.Sin(25f * Math.PI / 180f)
                            && p.Position.Y >= origin.Position.Y - (Math.Abs(p.Position.X - origin.Position.X)) * Math.Sin(25f * Math.PI / 180f))
                            eligiblePlayers.Add(p);
                    }
                    else    //diagonais
                    {
                        if (aim.Y < 0)  //diagonal para cima
                        {
                            if (p.Position.X > areaX.X && p.Position.X <= areaX.Y
                                && p.Position.Y >= (origin.Position.Y - (Math.Abs(p.Position.X - origin.Position.X)) * Math.Sin(80f * Math.PI / 180f))    //abaixo de
                                && p.Position.Y <= (origin.Position.Y - (Math.Abs(p.Position.X - origin.Position.X)) * Math.Sin(10f * Math.PI / 180f)))   //acima de
                            {
                                /*Console.WriteLine("passer: " + origin.Position.X + ", " + origin.Position.Y);
                                Console.WriteLine("abaixo de: " + (origin.Position.Y - (Math.Abs(p.Position.X - origin.Position.X)) * Math.Sin(90f * Math.PI / 180f)));
                                Console.WriteLine("acima de: " + (origin.Position.Y - (Math.Abs(p.Position.X - origin.Position.X)) * Math.Sin(10f * Math.PI / 180f)));
                                */
                                eligiblePlayers.Add(p);
                            }
                        }
                        else    //diagonal para baixo
                        {
                            if (p.Position.X > areaX.X && p.Position.X <= areaX.Y
                                && p.Position.Y >= origin.Position.Y + (Math.Abs(p.Position.X + origin.Position.X)) * Math.Sin(10f * Math.PI / 180f)
                                && p.Position.Y <= origin.Position.Y + (Math.Abs(p.Position.X + origin.Position.X)) * Math.Sin(80f * Math.PI / 180f))
                            {
                                /*Console.WriteLine("passer: " + origin.Position.X + ", " + origin.Position.Y);
                                Console.WriteLine("abaixo de: " + (origin.Position.Y - (Math.Abs(p.Position.X - origin.Position.X)) * Math.Sin(90f * Math.PI / 180f)));
                                Console.WriteLine("acima de: " + (origin.Position.Y - (Math.Abs(p.Position.X - origin.Position.X)) * Math.Sin(10f * Math.PI / 180f)));
                                */
                                eligiblePlayers.Add(p);
                            }
                        }
                    }
                }
                else if (aim.Y != 0)    //para cima ou baixo
                {
                    if (aim.X == 0)     //cima ou baixo
                    {
                        if (p.Position.Y > areaY.X && p.Position.Y <= areaY.Y
                            && p.Position.X <= origin.Position.X + (Math.Abs(p.Position.Y - origin.Position.Y)) * Math.Cos(25f * Math.PI / 180f)
                            && p.Position.X >= origin.Position.X - (Math.Abs(p.Position.Y - origin.Position.Y)) * Math.Cos(25f * Math.PI / 180f))
                            eligiblePlayers.Add(p);
                    }
                }
            }
            eligiblePlayers.Sort();

            if (eligiblePlayers.Count == 0)
                return ((Game)Game).Match.Ball.GetClosestPlayer(this, origin);
            else
            {
                int maxForce = 50;
                float div = maxForce / eligiblePlayers.Count;
                for (int i = 1; i <= eligiblePlayers.Count; i++)
                {
                    if (PassForce <= div * i) return (Player)eligiblePlayers[i - 1];
                }
            }
            return (Player)eligiblePlayers[0];
        }
    }
} 