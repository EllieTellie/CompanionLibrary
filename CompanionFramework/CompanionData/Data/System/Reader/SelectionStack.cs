using System.Collections.Generic;

namespace Companion.Data
{
    /// <summary>
    /// Selection stack for roster detection. Backed by a list.
    /// </summary>
    public class SelectionStack
    {
        List<Selection> selections = new List<Selection>();

        public void Push(Selection selection)
        {
            selections.Add(selection);
        }

        public Selection Peek()
        {
            if (selections.Count == 0)
                return null;

            return selections[selections.Count-1];
        }

        public Selection Pop()
        {
            if (selections.Count == 0)
                return null;

            int lastIndex = selections.Count - 1;
            Selection selection = selections[lastIndex];
            selections.RemoveAt(lastIndex);
            return selection;
        }

        /// <summary>
        /// Find the parent index in the previous selections. It removes any selections on the stack up to that parent.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Parent</returns>
        public Selection PopToParent(int index)
        {
            Selection parent = null;
            for (int i = selections.Count - 1; i >= 0; i--)
            {
                Selection previousSelection = selections[i];
                if (previousSelection.GetIndex() >= index)
                {
                    selections.RemoveAt(i); // pop it off
                }
                else
                {
                    parent = previousSelection;
                    break;
                }
            }

            return parent;
        }

        /// <summary>
        /// Find the parent index in the previous selections.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Parent</returns>
        public Selection GetParent(int index)
        {
            for (int i=selections.Count - 1; i>=0; i--)
            {
                Selection previousSelection = selections[i];

                if (previousSelection.GetIndex() == index - 1) // if it's our parent
                    return previousSelection;
            }

            return null;
        }

        public int Count
        {
            get
            {
                return selections.Count;
            }
        }

        public void Clear()
        {
            selections.Clear();
        }
    }
}
