using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using XNAExtras;

namespace GameMenu
{
    public enum ChoiceType
    {
        Normal,
        LeftRight
    };

    public class Menu : DrawableGameComponent
    {
        #region static methods

        public static int GetDefaultLRChoice(MenuChoice choice)
        {
            // in the future, get config data from some config file
            // for now, default to 0
            return 0;
        }

        #endregion

        #region private members

        // our menu tree
        MenuChoice m_root;
        // essentially, the current menu page 
        // (if they were to choose "options", for example, this would change)
        MenuChoice m_currentNodeValue;
        MenuChoice m_currentNode
        {
            set
            {
                m_currentNodeValue = value;
            }
            get
            {
                return m_currentNodeValue;
            }
        }

        GraphicsDevice m_device;

        // state checking variables for the Update() method
        KeyboardState m_prevKbState;
        MouseState m_prevMouseState;
        int m_currentMouseIntersection = -1;

        #endregion

        #region private methods

        // assumes that the current selection is the item of the event.
        void OnChoiceSelected()
        {
            if (ChoiceSelected != null)
            {
                MenuEvent e = new MenuEvent();
                MenuChoice choice = m_currentNode.GetChoices().GetSelectedChoice();
                if (choice.GetChoiceType() == ChoiceType.Normal)
                {
                    e.choiceString = choice.text;
                    e.choice = choice;
                }
                else if (choice.GetChoiceType() == ChoiceType.LeftRight)
                {
                    choice = choice.GetSelectedChoice();
                    e.choiceString = choice.text;
                    e.choice = choice;
                }
                ChoiceSelected(this, e);
            }
        }

        void OnChoiceDeselected()
        {
            if (ChoiceDeselected != null)
            {
                MenuEvent e = new MenuEvent();
                MenuChoice choice = m_currentNode.GetChoices().GetSelectedChoice();
                if (choice.GetChoiceType() == ChoiceType.Normal)
                {
                    e.choiceString = choice.text;
                    e.choice = choice;
                }
                else if (choice.GetChoiceType() == ChoiceType.LeftRight)
                {
                    choice = choice.GetSelectedChoice();
                    e.choiceString = choice.text;
                    e.choice = choice;
                }
                ChoiceDeselected(this, e);
            }
        }

        void OnChoiceExecuted()
        {
            if (ChoiceExecuted != null)
            {
                MenuEvent e = new MenuEvent();
                MenuChoice choice = m_currentNode.GetChoices().GetSelectedChoice();
                if (choice.GetChoiceType() == ChoiceType.Normal)
                {
                    e.choiceString = choice.text;
                    e.choice = choice;
                }
                else if (choice.GetChoiceType() == ChoiceType.LeftRight)
                {
                    choice = choice.GetSelectedChoice();
                    e.choiceString = choice.text;
                    e.choice = choice;
                }
                ChoiceExecuted(this, e);
            }
        }

        void OnChoiceValueChanged()
        {
            if (ChoiceValueChanged != null)
            {
                MenuEvent e = new MenuEvent();
                MenuChoice choice = m_currentNode.GetChoices().GetSelectedChoice();
                if (choice.GetChoiceType() == ChoiceType.Normal)
                {
                    e.choiceString = choice.text;
                    e.choice = choice;
                }
                else if (choice.GetChoiceType() == ChoiceType.LeftRight)
                {
                    choice = choice.GetSelectedChoice();
                    e.choiceString = choice.text;
                    e.choice = choice;
                }
                ChoiceValueChanged(this, e);
            }
        }

        MenuChoice CreateBackNode()
        {
            MenuChoice ret = new MenuChoice(backChoiceString);
            ret.textColor = textColor;
            ret.selectColor = selectColor;
            return ret;
        }

        void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            LoadResources();
        }

        /// <summary>
        /// Call this function to make sure the menu updates itself.  It will
        /// do nothing if the menu is not visible
        /// </summary>
        void LoadResources()
        {
            m_fontTimes.Reset(m_device);
        }

        void CalculatePositioning()
        {
            // the center of our menu
            Vector2 menuCenter;
            // the x value of the top of the first menu choice
            int menuTopX;

            if (Math.Abs(position.X + 86.0) < 0.01 && Math.Abs(position.Y + 86.0) < 0.01)
            {
                menuCenter.X = m_device.Viewport.Width / 2;
                menuCenter.Y = m_device.Viewport.Height / 2;
                menuTopX = (int)(menuCenter.Y - (m_currentNode.count * m_fontTimes.LineHeight +
                                (m_currentNode.count - 1) * verticalSpacing) / 2);
            }
            else
            {
                menuTopX = (int) position.X;

                int maxTextWidth = 0;
                foreach (MenuChoice choice in m_currentNode.GetChoices())
                {
                    maxTextWidth = Math.Max(m_fontTimes.MeasureString(choice.text), maxTextWidth);
                }
                int menuHeight = m_currentNode.count * m_fontTimes.LineHeight +
                                    (m_currentNode.count - 1) * verticalSpacing;

                menuCenter.X = maxTextWidth / 2;
                menuCenter.Y = position.Y + menuHeight / 2;
            }

            int left, top = menuTopX;
            // draw our text on the window
            foreach (MenuChoice choice in m_currentNode.GetChoices())
            {
                int width = m_fontTimes.MeasureString(choice.text);
                left = (int)(menuCenter.X - (width / 2));
                // if so, grab new bitmaps of the current text
                choice.rect = new Rectangle(left, top, width, m_fontTimes.LineHeight);
                top += m_fontTimes.LineHeight + verticalSpacing;

                if (choice.GetChoiceType() == ChoiceType.LeftRight)
                {
                    MenuChoice c;
                    int currentLeft = left + width + horizontalSpacing;
                    // calculate the position of the choice's choices
                    for (int i = 0; i < choice.count; ++i)
                    {
                        c = choice.GetChoice(i);
                        c.rect = new Rectangle(currentLeft, choice.rect.Top, choice.rect.Width, m_fontTimes.LineHeight);

                        currentLeft += m_fontTimes.MeasureString(c.text) + horizontalSpacing;
                    }
                }
            }
        }

        #endregion

        #region public properties
        public static BitmapFont m_fontTimes;

        Color m_textColor;
        /// <summary>
        ///  color of the text for each menu item
        /// </summary>
        public Color textColor
        {
            get
            {
                return m_textColor;
            }
            set
            {
                m_textColor = value;
                if (m_root != null)
                {
                    MenuChoice back = CreateBackNode();
                    back.SetParent(m_root);
                    m_root.GetChoices().SetLastChoice(back);
                }
            }
        }

        Color m_selectColor;
        /// <summary>
        /// color for menu choice that are selected
        /// </summary>
        public Color selectColor
        {
            get
            {
                return m_selectColor;
            }
            set
            {
                m_selectColor = value;
                if (m_root != null)
                {
                    MenuChoice back = CreateBackNode();
                    back.SetParent(m_root);
                    m_root.GetChoices().SetLastChoice(back);
                }
            }
        }

        /// <summary>
        ///   allow the user to set a background picture
        /// </summary>
        public Texture2D background;

        Boolean m_showBackChoice;
        /// <summary>
        /// Shows the "Back" menu choice, default is true.  Set the string in backChoiceString;
        /// </summary>
        public Boolean showBackChoice
        {
            get 
            {
                return m_showBackChoice;
            }
            set
            {
                m_showBackChoice = value;
                if (m_showBackChoice == false)
                {
                    if (m_currentNode != null)
                    {
                        m_currentNode.GetChoices().SetLastChoice(null);
                    }
                    if (m_root != null)
                    {
                        m_root.GetChoices().SetLastChoice(null);
                    }
                }
                else //true
                {
                    if (m_currentNode != null)
                    {
                        MenuChoice back = CreateBackNode();
                        back.SetParent(m_currentNode);
                        m_currentNode.GetChoices().SetLastChoice(back);
                    }
                    if (m_root != null)
                    {
                        MenuChoice back = CreateBackNode();
                        back.SetParent(m_root);
                        m_root.GetChoices().SetLastChoice(back);
                    }
                }
            }
        }

        /// <summary>
        /// the string shown for the "Back" choice
        /// </summary>
        public string backChoiceString;

        private Boolean m_visible;
        /// <summary>
        /// Determines whether the menu is visible or not.  setting this to
        /// false will disable most of the menu functionality.
        /// </summary>
        public Boolean visible
        {
            get
            {
                return m_visible;
            }
            set
            {
                if (visible == true || m_currentNode == null) // showing the menu?
                {
                    // start at the top
                    m_currentNode = m_root;

                    // and make sure we're in the correct spot
                    CalculatePositioning();
                }
                // oh, and select the first menu choice as well.
                m_currentNode.GetChoices().SetSelectedIndex(0);

                // now we're decent enough to be seen
                m_visible = value;
            }
        }

        private Vector2 m_position;
        /// <summary>
        /// Position of the menu in the window
        /// </summary>        
        public Vector2 position
        {
            get
            {
                return m_position;
            }
            set
            {
                m_position = value;
                //new position, new menu postitions!
                if(visible)
                    CalculatePositioning();
            }
        }
        

        /// <summary>
        /// vertical space between each menu item.  default is 10 pixels.
        /// </summary>
        public int verticalSpacing;
        /// <summary>
        /// horizontal space between each menu item.  default is 10 pixels.
        /// </summary>
        public int horizontalSpacing;

        #endregion

        #region public events

        public struct MenuEvent
        {
            public string choiceString;
            public MenuChoice choice;
        }

        public delegate void ChoiceSelectedHandler(object source, MenuEvent e);
        public delegate void ChoiceDeselectedHandler(object source, MenuEvent e);
        public delegate void ChoiceExecutedHandler(object source, MenuEvent e);
        public delegate void ChoiceValueChangedHandler(object source, MenuEvent e);

        /// <summary>
        /// Triggered when the selection moves to a new choice.
        /// When applicable, ChoiceSelected occurs after ChoiceDeselected.
        /// </summary>
        public event ChoiceSelectedHandler ChoiceSelected;

        /// <summary>
        /// Triggered when the selection moves away from the current choice.
        /// When applicable, ChoiceDeselected occurs before ChoiceSelected.
        /// </summary>
        public event ChoiceDeselectedHandler ChoiceDeselected;

        /// <summary>
        /// Triggered when the user executes a choice using the enter key
        /// or a mouse click.
        /// </summary>
        public event ChoiceExecutedHandler ChoiceExecuted;

        /// <summary>
        /// Triggered when the value of a choice changes (for example, in a resolution picker)
        /// </summary>
        public event ChoiceValueChangedHandler ChoiceValueChanged;
             
        #endregion

        /// <summary>
        /// Constructor. Your graphics device, please.
        /// </summary>
        public Menu(Game game) : base(game)
        {
            if (game.GraphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            // SET THE DEVICE FIRST
            m_device = game.GraphicsDevice;
            m_device.DeviceReset += new EventHandler(GraphicsDevice_DeviceReset);

            showBackChoice = true;
            backChoiceString = "Return";

            m_fontTimes = new BitmapFont("times.xml");
            textColor = Color.White;
            selectColor = Color.Green;
            verticalSpacing = 10;
            horizontalSpacing = 10;

            m_root = new MenuChoice("Menu");
            // create a default back node.
            MenuChoice back = CreateBackNode();
            back.SetParent(m_root);
            m_root.GetChoices().SetLastChoice(back);

            // set the position last, it relies on some of the above being initialized
            position = new Vector2(-86, -86);

            LoadResources();
        }

        /// <summary>
        /// Adds an entry to the end of the list of menu choices.
        /// </summary>
        /// <param name="choice">string value of the menu choice</param>
        public MenuChoice AddChoice(string choice)
        {
            MenuChoiceCollection choices = m_root.GetChoices();

            MenuChoice ret = choices.AddChoice(choice);
            ret.SetParent(m_root);
            ret.textColor = textColor;
            ret.selectColor = selectColor;
            choices.SetSelectedIndex(0);
            
            // got a new string, recalculate widths, centers, text positioning
            if(visible)
                CalculatePositioning();

            return ret;
        }

        #region XNA functions
        /// <summary>
        /// Call this function to make sure the menu updates itself.  It will
        /// do nothing if the menu is not visible
        /// </summary>
        /// <param name="gameTime">the current game time</param>
        public override void Update(GameTime gameTime)
        {
            if(!visible)
                return;
            
            // check mouse & keyboard
            KeyboardState kbState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            // Priorities:
            //
            // For starters, mouse comes before keyboard.  And if a selection event occurs, then
            //  stop processing selection input.  This will make the most recent interaction
            //  take priority over others (for example, a mouse sitting on top of a menu choice
            //  when you're trying to use the keyboard)

            int currentSelection = m_currentNode.GetChoices().GetSelectedIndex();
            bool selectionHandled = false;
            // SELECTION CODE
            // mouse selection
            if (mouseState.X != m_prevMouseState.X || mouseState.Y != m_prevMouseState.Y)
            {
                // we're in here because the mouse has moved
                // check for a collision
                Rectangle mouseRect = new Rectangle(mouseState.X, mouseState.Y, 1, 1);
                for (int i = 0; i < m_currentNode.count; ++i)
                {
                    if (mouseRect.Intersects(m_currentNode.GetChoice(i).rect))
                    {
                        m_currentMouseIntersection = i;
                        // deselection event
                        OnChoiceDeselected();

                        m_currentNode.GetChoices().SetSelectedIndex(i);
                        selectionHandled = true; // don't let the keyboard do anything for this Update call
                        
                        // kick off selection changed event!
                        OnChoiceSelected();
                    }

                    if (!selectionHandled)
                    {
                        m_currentMouseIntersection = -1;
                    }
                }
            }
            // keyboard up button
            if (kbState.IsKeyDown(Keys.Up) && !m_prevKbState.IsKeyDown(Keys.Up) && !selectionHandled)
            {
                // deselection event
                OnChoiceDeselected();

                // at the top of the list
                if (currentSelection - 1 < 0)
                    m_currentNode.GetChoices().SetSelectedIndex(m_currentNode.count - 1); // loop around
                else
                    m_currentNode.GetChoices().SetSelectedIndex(currentSelection - 1);
                selectionHandled = true; // set this to true for sanity.
                
                // kick off selection changed event!
                OnChoiceSelected();

            }
            // keyboard down button
            else if (kbState.IsKeyDown(Keys.Down) && !m_prevKbState.IsKeyDown(Keys.Down))
            {
                // deselection event
                OnChoiceDeselected();

                // at the bottom of the list
                if (currentSelection == m_currentNode.count - 1)
                    m_currentNode.GetChoices().SetSelectedIndex(0); // loop around
                else
                    m_currentNode.GetChoices().SetSelectedIndex(currentSelection + 1);
                selectionHandled = true; // set this to true for sanity.

                // kick off selection changed event!
                OnChoiceSelected();
            }
            // keyboard left button
            if (kbState.IsKeyDown(Keys.Left) && !m_prevKbState.IsKeyDown(Keys.Left) && !selectionHandled)
            {
                OnChoiceDeselected();
                m_currentNode.GetChoice(currentSelection).MoveSelectionLeft();
                OnChoiceSelected();
            }
            // keyboard right button
            if (kbState.IsKeyDown(Keys.Right) && !m_prevKbState.IsKeyDown(Keys.Right) && !selectionHandled)
            {
                OnChoiceDeselected();
                m_currentNode.GetChoice(currentSelection).MoveSelectionRight();
                OnChoiceSelected();
            }
            // end SELECTION CODE

            // handle execution
            if (kbState.IsKeyDown(Keys.Enter) && !m_prevKbState.IsKeyDown(Keys.Enter))
            {
                if (m_currentNode.GetChoice(currentSelection).GetChoiceType() == ChoiceType.Normal)
                {
                    // handle "back" case
                    if (m_currentNode.GetChoice(currentSelection).text == backChoiceString && m_currentNode != m_root)
                    {
                        m_currentNode = m_currentNode.GetChoice(currentSelection).GetParent();
                    }
                    else //else, we're at the root, so act normally
                    {
                        // execution event
                        OnChoiceExecuted();

                        if (m_currentNode.GetChoice(currentSelection).count > 0)
                        {
                            m_currentNode.GetChoice(currentSelection).SetParent(m_currentNode);

                            MenuChoice back = CreateBackNode();
                            if (showBackChoice)
                            {
                                // give it information to go backwards
                                back.SetParent(m_currentNode);
                            }

                            m_currentNode = m_currentNode.GetChoice(currentSelection);

                            if(showBackChoice)
                                m_currentNode.GetChoices().SetLastChoice(back);
                            m_currentNode.GetChoices().SetSelectedIndex(0);

                            CalculatePositioning();
                        }
                    }
                }
            }

            // mouse button and we are hovering above something
            if (mouseState.LeftButton == ButtonState.Pressed && m_currentMouseIntersection != -1)
            {
                if (m_currentNode.GetChoice(currentSelection).GetChoiceType() == ChoiceType.Normal)
                {
                    // handle "back" case
                    if (m_currentNode.GetChoice(currentSelection).text == backChoiceString && m_currentNode != m_root)
                    {
                        m_currentNode = m_currentNode.GetChoice(currentSelection).GetParent();
                    }
                    else //else, we're at the root, so act normally
                    {
                        // execution event
                        OnChoiceExecuted();

                        if (m_currentNode.GetChoice(currentSelection).count > 0)
                        {
                            m_currentNode.GetChoice(currentSelection).SetParent(m_currentNode);

                            MenuChoice back = CreateBackNode();
                            if (showBackChoice)
                            {
                                // give it information to go backwards
                                back.SetParent(m_currentNode);
                            }

                            m_currentNode = m_currentNode.GetChoice(currentSelection);

                            if (showBackChoice)
                                m_currentNode.GetChoices().SetLastChoice(back);
                            m_currentNode.GetChoices().SetSelectedIndex(0);

                            CalculatePositioning();
                        }
                    }
                    m_currentMouseIntersection = -1;
                }
            }

            m_prevKbState = kbState;
            m_prevMouseState = mouseState;
        }

        /// <summary>
        /// Call this function to have the menu draw itself.  It will
        /// do nothing if the menu is not visible.  Must be called outside
        /// of a spritebatch.begin() call (this has it's own spritebatch).
        /// </summary>
        /// <param name="gameTime">the current game time</param>
        public override void Draw(GameTime gameTime)
        {
            if(!visible || m_currentNode.count == 0)
                return;

            // draw our text on the window
            foreach (MenuChoice choice in m_currentNode.GetChoices())
            {
                // grab new bitmaps of the current text
                if (choice.GetChoiceType() == ChoiceType.Normal)
                    m_fontTimes.DrawString((int)choice.rect.X, (int)choice.rect.Y, choice.GetTextColor(), choice.text, "");
                else
                {
                    // draw our main text
                    m_fontTimes.DrawString((int)choice.rect.X, (int)choice.rect.Y, choice.GetTextColor(), choice.text, "");
                    // draw our choices
                    if (choice.visibleOnSelectionOnly && choice.isSelected || !choice.visibleOnSelectionOnly)
                    {
                        MenuChoice c;
                        for (int i = 0; i < choice.count; ++i)
                        {
                            c = choice.GetChoice(i);
                            m_fontTimes.DrawString((int)c.rect.X, (int)c.rect.Y, c.GetTextColor(), c.text, "");
                        }
                    }
                }
            }


        }

        

        #endregion
    }
}
