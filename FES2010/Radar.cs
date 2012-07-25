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
    public class Radar : Microsoft.Xna.Framework.GameComponent
    {
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Texture2D texture, ball, player;

        int PosX { set; get; }
        int PosY { set; get; }
        int Width { set; get; }
        int Height { set; get; }


        public Radar(Game game, SpriteBatch spriteBatch, SpriteFont spriteFont, Texture2D texture)
            : base(game)
        {
            this.spriteBatch = spriteBatch;
            this.spriteFont = spriteFont;
            this.texture = texture;

            Width = game.screenWidth / 7;
            Height = game.screenHeight / 4;
            PosX = game.screenWidth / 2  - Width / 2;
            PosY = game.screenHeight - Height - Height / 20;
        }
       
        public override void Initialize()
        {
            base.Initialize();
        }

        public void LoadContent()
        {
            ball = ((Game)Game).Content.Load<Texture2D>("ball");
            player = ((Game)Game).Content.Load<Texture2D>("player");
        }

        public override void Update(GameTime gameTime)
        {       
            base.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            if (((Game)Game).Match.DisplayRadar)
            {
                spriteBatch.Draw(texture, new Rectangle(PosX, PosY, Width, Height), new Color(Color.Gray, 120));

                spriteBatch.Draw(ball, ConvertCoordinates(((Game)Game).Match.Ball.Position), new Color(Color.White, 200));

                foreach (Player p in ((Game)Game).Match.HomeTeam.Players)
                    spriteBatch.Draw(player, ConvertCoordinates(p.Position), new Color(p.Team.Color, 130));

                foreach (Player p in ((Game)Game).Match.AwayTeam.Players)
                    spriteBatch.Draw(player, ConvertCoordinates(p.Position), new Color(p.Team.Color, 130));
            }
        }

        Rectangle ConvertCoordinates(Vector2 position)
        {
            float xPos = (position.X - ((Game)Game).Match.Field.Measures.Left) / ((Game)Game).Match.Field.Measures.FieldWidth;
            float yPos = (position.Y - ((Game)Game).Match.Field.Measures.Top) / ((Game)Game).Match.Field.Measures.FieldHeight;
            
            return new Rectangle((int)(PosX + xPos * Width), (int)(PosY + yPos * Height), 5, 5);
        }
    }
}