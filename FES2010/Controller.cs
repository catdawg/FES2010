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
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace FES2010
{
    public class Controller
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

        public Controller()
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
            this.Pass = Keys.C;
            this.FreePass = Keys.E;
            this.Shoot = Keys.X;
        }

        public Controller(int player2)
        {
            this.Up = Keys.I;
            this.Down = Keys.K;
            this.Left = Keys.J;
            this.Right = Keys.L;
            this.Sprint = Keys.N;
            this.ChangePlayer = Keys.U;
            this.Pause = Keys.Enter;
            this.Pressure = Keys.P;
            this.GoalerOut = Keys.E;
            this.Pass = Keys.OemPeriod;
            this.FreePass = Keys.O;
            this.Shoot = Keys.OemComma;
        }


        public Controller(Keys up, Keys down, Keys left, Keys right, Keys sprint, Keys changePlayer, 
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
