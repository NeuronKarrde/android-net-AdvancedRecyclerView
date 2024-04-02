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
using Android.Views;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Utils
{
    public abstract class AbstractSwipeableItemViewHolder : RecyclerView.ViewHolder, ISwipeableItemViewHolder
    {
        public AbstractSwipeableItemViewHolder(View itemView) : base(itemView)
        {
        }
        
        public AbstractSwipeableItemViewHolder(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
        {
        }

        public int SwipeStateFlags 
        {
            get => SwipeState.GetFlags();
            set => SwipeState.SetFlags(value);
        }

        public SwipeableItemState SwipeState { get; } = new SwipeableItemState();

        public int SwipeResult { get; set; } = RecyclerViewSwipeManager.RESULT_NONE;
        
        public int AfterSwipeReaction { get; set; } = RecyclerViewSwipeManager.AFTER_SWIPE_REACTION_DEFAULT;
        
        public bool IsProportionalSwipeAmountModeEnabled { get; set; } = true;

        public float SwipeItemVerticalSlideAmount { get; set; }
        public float SwipeItemHorizontalSlideAmount { get; set; }

        public abstract View SwipeableContainerView { get; }
        public float MaxLeftSwipeAmount { get; set; } = RecyclerViewSwipeManager.OUTSIDE_OF_THE_WINDOW_LEFT;
        public float MaxUpSwipeAmount { get; set; } = RecyclerViewSwipeManager.OUTSIDE_OF_THE_WINDOW_TOP;
        public float MaxRightSwipeAmount { get; set; } = RecyclerViewSwipeManager.OUTSIDE_OF_THE_WINDOW_RIGHT;
        public float MaxDownSwipeAmount { get; set; } = RecyclerViewSwipeManager.OUTSIDE_OF_THE_WINDOW_BOTTOM;

        public void OnSlideAmountUpdated(float horizontalAmount, float verticalAmount, bool isSwiping)
        {
        }
    }
}