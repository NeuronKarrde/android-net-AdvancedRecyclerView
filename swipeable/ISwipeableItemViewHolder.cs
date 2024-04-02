/*
 *    Copyright (C) 2015 Haruki Hasegawa
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http:
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */
using Android.Views;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Swipeable
{
    public interface ISwipeableItemViewHolder
    {
        int SwipeStateFlags { get; set; }
        SwipeableItemState SwipeState { get; }
        int SwipeResult { get; set; }
        int AfterSwipeReaction { get; set; }
        bool IsProportionalSwipeAmountModeEnabled { get; set; }
        float SwipeItemHorizontalSlideAmount { get; set; }
        float SwipeItemVerticalSlideAmount { get; set; }
        float MaxLeftSwipeAmount { get; set; }
        float MaxUpSwipeAmount { get; set; }
        float MaxRightSwipeAmount { get; set; }
        float MaxDownSwipeAmount { get; set; }
        View SwipeableContainerView { get; }
        void OnSlideAmountUpdated(float horizontalAmount, float verticalAmount, bool isSwiping);
    }
}