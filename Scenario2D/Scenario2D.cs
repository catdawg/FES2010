using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;


    public class Scenario2D 
    {

        public Vector2 CameraPosition;// { get; set; }
        public Vector2 CameraFacing { get; set; }
        public Game Game { get; set; }
        public float ViewHeight { get; set; }
        public float ViewWidth { get; set; }
        public float CameraFactor { get; set; }

        public Scenario2D(Game game)
        {
            Game = game;
            CameraFactor = 0.1f;
        }

        public Vector2 GetScreenPosition(Vector2 position)
        {
            return new Vector2(
            ((position.X - (CameraPosition.X - ViewWidth/2f)) * ((Game.Window.ClientBounds.Width) / ViewWidth)),
            ((position.Y - (CameraPosition.Y - ViewHeight / 2f)) * (Game.Window.ClientBounds.Height / ViewHeight)));
        }

        public float GetScaleX()
        {
            return ((Game.Window.ClientBounds.Width)) / (ViewWidth);
        }

        public float GetScaleY()
        {
            return ((Game.Window.ClientBounds.Height)) / (ViewHeight);
        }

        public Rectangle GetScreenRectangle(Rectangle rectangle)
        {
            Vector2 vec = GetScreenPosition(new Vector2(rectangle.X, rectangle.Y));
            return new Rectangle((int)vec.X, (int)vec.Y, (int)(rectangle.Width*GetScaleX()), (int)(rectangle.Height*GetScaleY()));
        }
    }