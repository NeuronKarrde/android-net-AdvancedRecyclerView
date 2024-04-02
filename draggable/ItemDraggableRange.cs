/*
 *    Copyright (C) 2015 Haruki Hasegawa
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    public class ItemDraggableRange
    {
        private readonly int mStart;
        private readonly int mEnd;
        public ItemDraggableRange(int start, int end)
        {
            if (!(start <= end))
            {
                throw new ArgumentException("end position (= " + end + ") is smaller than start position (=" + start + ")");
            }

            mStart = start;
            mEnd = end;
        }

        public virtual int GetStart()
        {
            return mStart;
        }

        public virtual int GetEnd()
        {
            return mEnd;
        }

        public virtual bool CheckInRange(int position)
        {
            return ((position >= mStart) && (position <= mEnd));
        }

        protected virtual string GetClassName()
        {
            return "ItemDraggableRange";
        }

        public virtual string ToString()
        {
            return GetClassName() + "{" + "mStart=" + mStart + ", mEnd=" + mEnd + '}';
        }
    }
}