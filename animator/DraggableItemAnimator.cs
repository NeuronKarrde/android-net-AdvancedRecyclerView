/*
 *    Copyright (C) 2016 Haruki Hasegawa
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

using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Animator
{
    /// <summary>
    /// ItemAnimator for Draggable item. This animator is required to work animations properly on drop an item.
    /// </summary>
    public class DraggableItemAnimator : RefactoredDefaultItemAnimator
    {
        protected override void OnSetup()
        {
            base.OnSetup();
            base.SupportsChangeAnimations = false;
        }

        public override bool AnimateChange(RecyclerView.ViewHolder oldHolder, RecyclerView.ViewHolder newHolder, int fromX, int fromY, int toX, int toY)
        {
            if (oldHolder == newHolder && fromX == toX && fromY == toY)
            {

                // WORKAROUND: Skip animateChange() for the dropped item. Should be implemented better approach.
                DispatchChangeFinished(oldHolder, true);
                return false;
            }

            return base.AnimateChange(oldHolder, newHolder, fromX, fromY, toX, toY);
        }
    }
}