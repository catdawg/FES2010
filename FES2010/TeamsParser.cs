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
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace FES2010
{
    class TeamsParser
    {
        String filePath;

        public TeamsParser(String filePath) 
        { 
            this.filePath = filePath;
        }
        public void Parse(Game game, ArrayList teams /*, Scenario2DPainter scenario2DPainter */)
        {
            String line;
            Team team = null;   //current team

            if (File.Exists(filePath))
            {
                StreamReader file = null;
                try
                {
                    file = new StreamReader(filePath);
                    while ((line = file.ReadLine()) != null)
                    {
                        line = line.TrimStart(' '); //remove whitespaces from the start of the line
                        if (line.Equals("") || line.StartsWith("//"))    //ignore empty and comment lines
                            continue;
                        else if (line.StartsWith("T:")) //new team
                        {
                            String teamName = line.Substring(2).TrimStart(' ');
                            team = new Team(game, teamName, Color.White);

                            //add team to teams
                            teams.Add(team);
                        }
                        else if (line.StartsWith("C:"))
                        {
                            String color = line.Substring(2).TrimStart(' ');

                            byte r = byte.Parse(color.Substring(0, 3));
                            byte g = byte.Parse(color.Substring(3, 3));
                            byte b = byte.Parse(color.Substring(6, 3));

                            team.Color = new Color(r, g, b);
                        }
                        else if (line.StartsWith("P:")) //positioning
                        {
                            String teamStrategy = line.Substring(2).TrimStart(' ');
                            team.SetStrategy(teamStrategy);
                        }
                        else if (team != null)   //read players
                        {
                            String playerName = "", position = "", side = "";
                            int number, defense, goalkeeping, offense, shot, speed, stamina;
                            int extraWhiteSpaces = 0;

                            Player newPlayer = null;
                            String[] tokens = line.Split(' ');

                            int i = 0;  //in the end of the cycle it will be the index of the player number
                            double num;
                            //get player name
                            while (!double.TryParse(tokens[i], out num))    //while not a number
                            {
                                if (tokens[i].Equals(""))  //extra white space between player's names
                                {
                                    extraWhiteSpaces++;
                                    i++;
                                    continue;
                                }
                                if (i == 0) playerName += tokens[i];
                                else playerName += " " + tokens[i];
                                i++;
                            }
                            int j = i;
                            i -= extraWhiteSpaces;
                            while (j < tokens.Length)   //extra white space between the rest of the info
                            {
                                if (tokens[j].Equals(""))
                                {
                                    extraWhiteSpaces++;
                                }
                                j++;
                            }
                            //get player properties
                            if (tokens.Length - i == 9 + extraWhiteSpaces) //correct amount of info
                            {
                                //limit the skill level allowed
                                int maxSkill = 100;
                                try
                                {
                                    i += extraWhiteSpaces;
                                    number = int.Parse(tokens[i++]);
                                    position = tokens[i++];
                                    side = tokens[i++];
                                    defense = (int.Parse(tokens[i++]) > maxSkill) ? maxSkill : int.Parse(tokens[i - 1]);
                                    goalkeeping = (int.Parse(tokens[i++]) > maxSkill) ? maxSkill : int.Parse(tokens[i - 1]);
                                    offense = (int.Parse(tokens[i++]) > maxSkill) ? maxSkill : int.Parse(tokens[i - 1]);
                                    shot = (int.Parse(tokens[i++]) > maxSkill) ? maxSkill : int.Parse(tokens[i - 1]);
                                    speed = (int.Parse(tokens[i++]) > maxSkill) ? maxSkill : int.Parse(tokens[i - 1]);
                                    stamina = (int.Parse(tokens[i++]) > maxSkill) ? maxSkill : int.Parse(tokens[i - 1]);

                                    TacticalPosition position2 = TacticalPosition.goalkeeper;
                                    Side side2 = Side.center;

                                    if (position.Equals("defender")) position2 = TacticalPosition.defender;
                                    else if (position.Equals("dfmidfielder")) position2 = TacticalPosition.dfmidfielder;
                                    else if (position.Equals("midfielder")) position2 = TacticalPosition.midfielder;
                                    else if (position.Equals("ofmidfielder")) position2 = TacticalPosition.ofmidfielder;
                                    else if (position.Equals("striker")) position2 = TacticalPosition.striker;

                                    if (side.Equals("left")) side2 = Side.left;
                                    else if (side.Equals("right")) side2 = Side.right;

                                    //create new player
                                    newPlayer = new Player(game, team, playerName, number, position2, side2, defense, goalkeeping, offense, shot, speed, stamina /*scenario2DPainter*/, 0f, 0f);

                                    //insert player in team
                                    team.Players.Add(newPlayer);
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine("Wrong line format for player " + playerName + "!");
                                }
                            }
                            else Console.WriteLine("Incorrect amount of info in line: " + line);

                        }
                        Console.WriteLine(line);
                    }
                }
                finally
                {
                    if (file != null)
                        file.Close();
                }
            }
            else Console.WriteLine("Teams file not found!");
        }
    }
}
