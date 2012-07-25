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
    public class Field : Microsoft.Xna.Framework.GameComponent
    {
        Texture2D texture;
        public Measures Measures { get; set; }

        public Vector2 HalfSize { get; set; } //real size of the field (with scaling)

        public int Width { get; set; }
        public int Height { get; set; }
        public float Scale { get; set; }   //visualization scale

        public Field(Game game)
            : base(game)
        {
            

            Scale = 2.0f;

            
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

            texture = Game.Content.Load<Texture2D>("soccer_field");
            Width = texture.Height;
            Height = texture.Width;

            HalfSize = new Vector2(Width / 2 * Scale, Height / 2 * Scale);

            // must be initialized after all vars for the field have been set
            this.Measures = new Measures(this);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
           ((Game)Game).Match.Scenario2DPainter.DrawInScenario(texture, new Vector2(0, 0), null, Color.White, (float)((Math.PI)/2.0f), new Vector2(texture.Width/2f, texture.Height/2f), Scale, SpriteEffects.None, 1);  //

        }

        
    }
    public class Measures
    {
        public float BoxHeight { private set; get; }
        public float HalfBoxWidth { private set; get; }
        public float Bottom { private set; get; }
        public float Top { private set; get; }
        public float Left { private set; get; }
        public float GoalKickX1 { private set; get; }
        public float GoalKickX2 { private set; get; }
        public float LeftCornerX { private set; get; }
        public float RightCornerX { private set; get; }
        public float TopCornerY { private set; get; }
        public float BottomCornerY { private set; get; }
        public float GoalStart { private set; get; }
        public float GoalEnd { private set; get; }
        public float GoalCoveringFactor { private set; get; }
        public float FieldHeight { private set; get; }
        public float FieldWidth { private set; get; }
        public float[] Positions {private set; get; }

        public Measures(Field field)
        {
            BoxHeight = 240;
            HalfBoxWidth = 285;
            Bottom = field.HalfSize.Y - 40;
            Top = -field.HalfSize.Y + 40;
            Left = -field.HalfSize.X;
            GoalKickX1 = field.HalfSize.X / 5;
            GoalKickX2 = - field.HalfSize.X / 5;
            LeftCornerX = Left + 10;
            TopCornerY = Top + 15;
            RightCornerX = field.HalfSize.X - 10;
            BottomCornerY =  Bottom - 15;
            GoalStart = -55;    //change to goal.size/2 when there's one xD
            GoalEnd = 56;
            GoalCoveringFactor = field.HalfSize.X / GoalEnd;
            FieldHeight = Math.Abs(Bottom - Top);
            FieldWidth = Math.Abs((field.HalfSize.X) - (-field.HalfSize.X));
            Positions = new float[6];
            Positions[0] = 10;
            Positions[1] = (1 / 6.0f) * FieldHeight;
            Positions[2] = (1 / 3.0f) * FieldHeight;
            Positions[3] = (1 / 2.0f) * FieldHeight;
            Positions[4] = (2 / 3.0f) * FieldHeight;
            Positions[5] = (5 / 6.0f) * FieldHeight;
        }
    }
}