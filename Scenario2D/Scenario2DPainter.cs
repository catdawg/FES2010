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


    public class Scenario2DPainter : SpriteBatch
    {

        public Scenario2D Scenario2D { get; set; }

        public Scenario2DPainter(GraphicsDevice graphicsDevice, Scenario2D scenario) : base(graphicsDevice)
        {
            Scenario2D = scenario;
        }

        public void DrawInScenario(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            base.Draw(texture,
                Scenario2D.GetScreenRectangle(destinationRectangle),
                color);
        }
        public void DrawInScenario(Texture2D texture, Vector2 position, Color color)
        {
            base.Draw(texture,
                Scenario2D.GetScreenPosition(position),
                null,
                color,
                0,
                new Vector2(0, 0),
                new Vector2(Scenario2D.GetScaleX(), Scenario2D.GetScaleY()), SpriteEffects.None, 1);
        }
        public void DrawInScenario(Texture2D texture, 
            Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        {

            Rectangle? sourceRectangle2;
            if (sourceRectangle == null)
                sourceRectangle2 = null;
            else sourceRectangle2 = Scenario2D.GetScreenRectangle((Rectangle)sourceRectangle);
            base.Draw( texture, 
                Scenario2D.GetScreenRectangle(destinationRectangle), 
                sourceRectangle2, color);     
        }

        public void DrawInScenario(Texture2D texture, 
            Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            Rectangle? sourceRectangle2;
            if (sourceRectangle == null)
                sourceRectangle2 = null;
            else sourceRectangle2 = Scenario2D.GetScreenRectangle((Rectangle)sourceRectangle);
            base.Draw(texture,
                Scenario2D.GetScreenPosition(position),
                sourceRectangle2,
                color,
                0,
                new Vector2(0, 0),
                new Vector2(Scenario2D.GetScaleX(), Scenario2D.GetScaleY()), SpriteEffects.None, 1);
        }

        public void DrawInScenario(Texture2D texture, 
            Rectangle destinationRectangle, 
            Rectangle? sourceRectangle, 
            Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            base.Draw(texture, Scenario2D.GetScreenRectangle(destinationRectangle), 
                sourceRectangle, color, rotation, origin, effects, layerDepth);
        }

        public void DrawInScenario(Texture2D texture,
            Vector2 position,
            Rectangle? sourceRectangle,
            Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            Rectangle? sourceRectangle2;
            if (sourceRectangle == null)
                sourceRectangle2 = null;
            else sourceRectangle2 = Scenario2D.GetScreenRectangle((Rectangle)sourceRectangle);
            base.Draw(texture, Scenario2D.GetScreenPosition(position),
                sourceRectangle2, color, rotation, origin, (new Vector2(Scenario2D.GetScaleX()*scale, Scenario2D.GetScaleY()*scale)), effects, layerDepth);
        }

        public void DrawInScenario(Texture2D texture,
            Vector2 position,
            Rectangle? sourceRectangle,
            Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            Rectangle? sourceRectangle2;
            if (sourceRectangle == null)
                sourceRectangle2 = null;
            else sourceRectangle2 = Scenario2D.GetScreenRectangle((Rectangle)sourceRectangle);
            base.Draw(texture, Scenario2D.GetScreenPosition(position),
                sourceRectangle2, color, rotation, origin, (new Vector2(Scenario2D.GetScaleX()*scale.X, Scenario2D.GetScaleY()*scale.Y)), effects, layerDepth);
        }
    
    }
