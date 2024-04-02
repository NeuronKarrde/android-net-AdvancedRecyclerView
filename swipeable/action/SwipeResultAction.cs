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

using Android.Runtime;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable.Action
{
    public abstract class SwipeResultAction
    {
        private readonly int mResultAction;
        
        protected SwipeResultAction(int resultAction)
        {
            mResultAction = resultAction;
        }
        public SwipeResultAction(IntPtr javaReference, JniHandleOwnership transfer) 
        {
        }

        public virtual int GetResultActionType()
        {
            return mResultAction;
        }

        public void PerformAction()
        {
            OnPerformAction();
        }

        public void SlideAnimationEnd()
        {
            OnSlideAnimationEnd();
            OnCleanUp();
        }

        /// <summary>
        /// This method is called immediately after returning from the {@link com.h6ah4i.android.widget.advrecyclerview.swipeable.SwipeableItemAdapter#onSwipeItem(RecyclerView.ViewHolder, int, int)} method.
        /// You can modify data set and call notifyXXX() methods of adapter in this method.
        /// </summary>
        protected virtual void OnPerformAction()
        {
        }

        /// <summary>
        /// This method is called when item slide animation has completed.
        /// </summary>
        protected virtual void OnSlideAnimationEnd()
        {
        }

        /// <summary>
        /// This method is called after the {@link #onSlideAnimationEnd()} method. Clear fields to avoid memory leaks.
        /// </summary>
        protected virtual void OnCleanUp()
        {
        }
    }
}