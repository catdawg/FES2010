using System;
using System.Collections;
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
    

    public class MenuChoiceCollection : IEnumerable
    {
        #region private stuff
        List<MenuChoice> m_choices;
        int m_selectedIndex;

        // this last choice makes indexing a little tricky, so watch out for that.
        // we have count and [] operators, so for loops don't need to be careful, though
        MenuChoice m_lastChoice;

        // no parent stuff for now
        //MenuChoice m_parent;
        #endregion

        #region public properties

        public int count
        {
            get
            {
                // careful about last choice!
                if (m_lastChoice != null)
                    return m_choices.Count + 1;
                else
                    return m_choices.Count;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)new MenuChoiceCollectionEnumerator(this);
        }

        #endregion

        #region operators
        public MenuChoice this[int index]
        {
            get
            {
                // carefule (again) about last choice.
                if (m_lastChoice != null)
                {
                    if (index == m_choices.Count)
                        return m_lastChoice;
                    else if (index > m_choices.Count)
                        throw new IndexOutOfRangeException();

                    return m_choices[index];
                }
                else
                {
                    if (index > m_choices.Count)
                        throw new IndexOutOfRangeException();

                    return m_choices[index];
                }
            }
        }
        #endregion

        #region public methods
        public MenuChoiceCollection()
        {
            m_choices = new List<MenuChoice>();
            m_selectedIndex = 0;
        }

        public MenuChoice AddChoice(string text)
        {
            MenuChoice choice = new MenuChoice(text);
            m_choices.Add(choice);
            if (m_choices.Count == 1)
                SetSelectedIndex(0);

            return choice;
        }

        public MenuChoice Insert(int index, string text)
        {
            MenuChoice choice = new MenuChoice(text);
            m_choices.Insert(index, choice);

            return choice;
        }

        public void Remove(string text)
        {
            int index = IndexOf(text);
            if (index == -1)
                return;
            else if (index == this.count - 1 && m_lastChoice != null)
                m_lastChoice = null;
            else
                m_choices.Remove(m_choices[index]);
        }

        public void RemoveAt(int index)
        {
            if (index < this.count)
            {
                if (index == this.count - 1 && m_lastChoice != null)
                    m_lastChoice = null; // effectively remove the last choices
                else
                    m_choices.RemoveAt(index);
            }
        }

        public int IndexOf(string text)
        {
            for (int i = 0; i < this.count; ++i)
            {
                if (this[i].text == text)
                    return i;
            }

            return -1;
        }

        public void Clear()
        {
            m_choices.Clear();
        }

        public void SetSelectedIndex(int index)
        {
            if (index < this.count)
            {
                for (int i = 0; i < this.count; ++i)
                {
                    this[i].isSelected = false;
                }
                this[index].isSelected = true;
                m_selectedIndex = index;
            }
        }

        public int GetSelectedIndex()
        {
            return m_selectedIndex;
        }

        public MenuChoice GetSelectedChoice()
        {
            return this[m_selectedIndex];
        }

        /// <summary>
        /// sets the last choice returned in this collection.  Useful for a final "back" choice
        /// </summary>
        /// <param name="choice">the last choice</param>
        public void SetLastChoice(MenuChoice choice)
        {
            m_lastChoice = choice;
        }

        #endregion

        #region enumerator
        public class MenuChoiceCollectionEnumerator : IEnumerator
        {
            MenuChoiceCollection m_menu;
            int m_enumeratorIndex;
            public MenuChoiceCollectionEnumerator(MenuChoiceCollection menu)
            {
                m_enumeratorIndex = -1;
                m_menu = menu;
            }

            #region Enumerator Interface
            public object Current
            {
                get
                {
                    if (m_enumeratorIndex >= m_menu.count || m_enumeratorIndex == -1)
                        throw new InvalidOperationException();
                    return m_menu[m_enumeratorIndex];
                }
            }

            // Summary:
            //     Advances the enumerator to the next element of the collection.
            //
            // Returns:
            //     true if the enumerator was successfully advanced to the next element; false
            //     if the enumerator has passed the end of the collection.
            //
            // Exceptions:
            //   System.InvalidOperationException:
            //     The collection was modified after the enumerator was created.
            public bool MoveNext()
            {
                if (m_enumeratorIndex == m_menu.count - 1)
                {
                    m_enumeratorIndex = m_menu.count;
                    return false;
                }

                m_enumeratorIndex += 1;
                return true;
            }
            //
            // Summary:
            //     Sets the enumerator to its initial position, which is before the first element
            //     in the collection.
            //
            // Exceptions:
            //   System.InvalidOperationException:
            //     The collection was modified after the enumerator was created.
            public void Reset()
            {
                m_enumeratorIndex = 0;
            }

            #endregion
        }
        #endregion
    }
}
