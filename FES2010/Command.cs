using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace FES2010
{
    public class Command
    {
        public Keys Up { set; get; }
        public Keys Down { set; get; }
        public Keys Left { set; get; }
        public Keys Right { set; get; }
        public Keys Sprint { set; get; }
        public Keys ChangePlayer { set; get; }
        public Keys Pause { set; get; }
        public Keys Pressure { set; get; }
        public Keys GoalerOut { set; get; }
        public Keys Pass { set; get; }
        public Keys FreePass { set; get; }
        public Keys Shoot { set; get; }

        public Command()
        {
            this.Up = Keys.W;
            this.Down = Keys.S;
            this.Left = Keys.A;
            this.Right = Keys.D;
            this.Sprint = Keys.LeftShift;
            this.ChangePlayer = Keys.Q;
            this.Pause = Keys.D1;
            this.Pressure = Keys.Space;
            this.GoalerOut = Keys.E;
            this.Pass = Keys.LeftAlt;
            this.FreePass = Keys.C;
            this.Shoot = Keys.LeftControl;
        }

        public Command(int player2)
        {
            this.Up = Keys.Up;
            this.Down = Keys.Down;
            this.Left = Keys.Left;
            this.Right = Keys.Right;
            this.Sprint = Keys.RightShift;
            this.ChangePlayer = Keys.M;
            this.Pause = Keys.Enter;
            this.Pressure = Keys.OemPeriod;
            this.GoalerOut = Keys.OemMinus;
            this.Pass = Keys.End;
            this.FreePass = Keys.OemComma;
            this.Shoot = Keys.RightControl;
        }


        public Command(Keys up, Keys down, Keys left, Keys right, Keys sprint, Keys changePlayer, 
                       Keys pause, Keys pressure, Keys goalerOut, Keys pass, Keys freePass, Keys shoot)
        {
             this.Up = up;
             this.Down = down;
             this.Left = left;
             this.Right = right;
             this.Sprint = sprint;
             this.ChangePlayer = changePlayer;
             this.Pause = pause;
             this.Pressure = pressure;
             this.GoalerOut = goalerOut;
             this.Pass = pass;
             this.FreePass = freePass;
             this.Shoot = shoot;
        }
    }
}
