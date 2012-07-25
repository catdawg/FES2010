using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace GameMenu
{
    public class MenuChoice
    {
        #region private properties

        MenuChoiceCollection m_nodes;

        // no parent stuff for now
        MenuChoice m_parent;

        /// <summary>
        /// mode of this menu choice (Normal by default)
        /// when this is set to LeftRight, m_nodes becomes displayed along with
        /// this menu choice (rather than as a sub-menu).
        /// </summary>
        ChoiceType m_choiceType;
        int m_selectedChoice;

        #endregion

        #region public properties
        /// <summary>
        /// color of the text for this menu choice.
        /// defaults to the parent value (if one exists).
        /// </summary>
        public Color textColor;

        /// <summary>
        /// background color for this menu choice.
        /// defaults to the parent value (if one exists).
        /// </summary>
        public Color selectColor;

        /// <summary>
        /// rectangle of this text
        /// </summary>
        public Rectangle rect;

        private int m_index;
        /// <summary>
        /// this menu choices index within the current list
        /// </summary>
        public int index
        {
            get
            {
                return m_index;
            }

        }

        /// <summary>
        /// this menu choices index within the current list
        /// </summary>
        public int count
        {
            get
            {
                return m_nodes.count;
            }

        }
                       
        private Boolean m_isSelected;
        /// <summary>
        /// gets whether this choice is selected or not;
        /// </summary>
        public Boolean isSelected
        {
            get
            {
                return m_isSelected;
            }
            set
            {
                m_isSelected = value;
                m_selectedChoice = Menu.GetDefaultLRChoice(this);
            }
        }

        /// <summary>
        /// Determines when the left/right choices are shown, default is true.
        /// </summary>
        public Boolean visibleOnSelectionOnly;

        /// <summary>
        /// gets or set the text displayed by this choice
        /// </summary>
        public string text;

        private object m_value;
        /// <summary>
        /// gets the value of this choice
        /// </summary>
        public object value
        {
            get
            {
                return m_value;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// contructor
        /// </summary>
        /// <param name="text">the text to be displayed by this choice</param>
        public MenuChoice(string text)
        {
            this.text = text;
            m_nodes = new MenuChoiceCollection();

            m_choiceType = ChoiceType.Normal;
            visibleOnSelectionOnly = true;
            textColor = Color.White;
            selectColor = Color.Green;
        }

        /// <summary>
        /// Adds an entry to the end of the list of menu choices.
        /// </summary>
        /// <param name="choice">string value of the menu choice</param>
        public MenuChoice AddChoice(string text)
        {
            MenuChoice choice = m_nodes.AddChoice(text);
            choice.textColor = textColor;
            choice.selectColor = selectColor;

            return choice;
        }

        /// <summary>
        /// sets the left right choices for this menu choice
        /// </summary>
        /// <param name="choice">string value of the choice</param>
        public void AddLeftRightChoices(Array choices)
        {
            m_choiceType = ChoiceType.LeftRight;
            m_selectedChoice = 0;

            foreach (string str in choices)
            {
                MenuChoice c = m_nodes.AddChoice(str);
                c.selectColor = selectColor;
                c.textColor = textColor;
            }
        }

        /// <summary>
        /// gets the choices for this menu choice (for all choiceTypes)
        /// </summary>
        public MenuChoice GetChoice(int index)
        {
            if (index < m_nodes.count)
                return m_nodes[index];
            else return null;
        }
        
        /// <summary>
        /// returns the children below this menu choice
        /// </summary>
        /// <returns></returns>
        public MenuChoiceCollection GetChoices()
        {
            return m_nodes;
        }

        /// <summary>
        /// gets the Selected choice for this menu item
        /// </summary>
        public MenuChoice GetSelectedChoice()
        {
            if (m_choiceType == ChoiceType.LeftRight)
                return m_nodes[m_selectedChoice];
            else return this;
        }

        public void MoveSelectionRight()
        {
            if (m_choiceType == ChoiceType.LeftRight)
            {
                if (m_selectedChoice + 1 >= m_nodes.count)
                    m_selectedChoice = 0;
                else
                    m_selectedChoice += 1;
                m_nodes.SetSelectedIndex(m_selectedChoice);
            }
        }

        public void MoveSelectionLeft()
        {
            if (m_choiceType == ChoiceType.LeftRight)
            {
                if (m_selectedChoice - 1 < 0)
                    m_selectedChoice = m_nodes.count - 1;
                else
                    m_selectedChoice -= 1;
                m_nodes.SetSelectedIndex(m_selectedChoice);
            }
        }

        /// <summary>
        /// returns the choice type
        /// </summary>
        public ChoiceType GetChoiceType()
        {
            return m_choiceType;
        }

        /// <summary>
        /// returns the current color of this choice (selected or unselected).
        /// </summary>
        public Color GetTextColor()
        {
            if (isSelected)
                return selectColor;
            else
                return textColor;
        }

        /// <summary>
        /// call this to set the parent to some value (for when the user presses escape
        /// </summary>
        /// <param name="p">the parent of this menu choice</param>
        public void SetParent(MenuChoice p)
        {
            m_parent = p;
        }

        /// <summary>
        /// returns our parent node
        /// </summary>
        /// <returns></returns>
        public MenuChoice GetParent()
        {
            return m_parent;
        }

        #endregion
    }
}
