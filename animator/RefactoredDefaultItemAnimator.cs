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

using Android.Views;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Animator.Impl;
using Java.Util;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Animator
{
    public class RefactoredDefaultItemAnimator : GeneralItemAnimator
    {
        protected override void OnSetup()
        {
            SetItemAddAnimationsManager(new DefaultItemAddAnimationManager(this));
            SetItemRemoveAnimationManager(new DefaultItemRemoveAnimationManager(this));
            SetItemChangeAnimationsManager(new DefaultItemChangeAnimationManager(this));
            SetItemMoveAnimationsManager(new DefaultItemMoveAnimationManager(this));
        }

        protected override void OnSchedulePendingAnimations()
        {
            SchedulePendingAnimationsByDefaultRule();
        }

        public override bool CanReuseUpdatedViewHolder(RecyclerView.ViewHolder viewHolder, IList<Java.Lang.Object> payloads)
        {
            return payloads.Any() || base.CanReuseUpdatedViewHolder(viewHolder, payloads);
        }

        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// If the payload list is not empty, RefactoredDefaultItemAnimator returns <code>true</code>.
        /// When this is the case:
        /// <ul>
        /// <li>If you override {@link #animateChange(RecyclerView.ViewHolder, RecyclerView.ViewHolder, int, int, int, int)}, both
        /// ViewHolder arguments will be the same instance.
        /// </li>
        /// <li>
        /// If you are not overriding {@link #animateChange(RecyclerView.ViewHolder, RecyclerView.ViewHolder, int, int, int, int)},
        /// then RefactoredDefaultItemAnimator will call {@link #animateMove(RecyclerView.ViewHolder, int, int, int, int)} and
        /// run a move animation instead.
        /// </li>
        /// </ul>
        /// </summary>
        /// <summary>
        /// Item Animation manager for ADD operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        protected class DefaultItemAddAnimationManager : ItemAddAnimationManager
        {
            public DefaultItemAddAnimationManager(BaseItemAnimator itemAnimator) : base(itemAnimator)
            {
            }

            protected override void OnCreateAnimation(AddAnimationInfo info)
            {
                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(info.holder.ItemView);
                animator.Alpha(1);
                animator.SetDuration(GetDuration());
                StartActiveItemAnimation(info, info.holder, animator);
            }

            protected override void OnAnimationEndedSuccessfully(AddAnimationInfo info, RecyclerView.ViewHolder item)
            {
            }

            protected override void OnAnimationEndedBeforeStarted(AddAnimationInfo info, RecyclerView.ViewHolder item)
            {
                item.ItemView.Alpha = 1;
            }

            protected override void OnAnimationCancel(AddAnimationInfo info, RecyclerView.ViewHolder item)
            {
                item.ItemView.Alpha = 1;
            }

            public override bool AddPendingAnimation(RecyclerView.ViewHolder item)
            {
                ResetAnimation(item);
                item.ItemView.Alpha = 0;
                EnqueuePendingAnimationInfo(new AddAnimationInfo(item));
                return true;
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// If the payload list is not empty, RefactoredDefaultItemAnimator returns <code>true</code>.
        /// When this is the case:
        /// <ul>
        /// <li>If you override {@link #animateChange(RecyclerView.ViewHolder, RecyclerView.ViewHolder, int, int, int, int)}, both
        /// ViewHolder arguments will be the same instance.
        /// </li>
        /// <li>
        /// If you are not overriding {@link #animateChange(RecyclerView.ViewHolder, RecyclerView.ViewHolder, int, int, int, int)},
        /// then RefactoredDefaultItemAnimator will call {@link #animateMove(RecyclerView.ViewHolder, int, int, int, int)} and
        /// run a move animation instead.
        /// </li>
        /// </ul>
        /// </summary>
        /// <summary>
        /// Item Animation manager for ADD operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        /// <summary>
        /// Item Animation manager for REMOVE operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        protected class DefaultItemRemoveAnimationManager : ItemRemoveAnimationManager
        {
            public DefaultItemRemoveAnimationManager(BaseItemAnimator itemAnimator) : base(itemAnimator)
            {
            }

            protected override void OnCreateAnimation(RemoveAnimationInfo info)
            {
                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(info.holder.ItemView);
                animator.SetDuration(GetDuration());
                animator.Alpha(0);
                StartActiveItemAnimation(info, info.holder, animator);
            }

            protected override void OnAnimationEndedSuccessfully(RemoveAnimationInfo info, RecyclerView.ViewHolder item)
            {
                View view = item.ItemView;
                view.Alpha = 1;
            }

            protected override void OnAnimationEndedBeforeStarted(RemoveAnimationInfo info, RecyclerView.ViewHolder item)
            {
                View view = item.ItemView;
                view.Alpha = 1;
            }

            protected override void OnAnimationCancel(RemoveAnimationInfo info, RecyclerView.ViewHolder item)
            {
            }

            public override bool AddPendingAnimation(RecyclerView.ViewHolder holder)
            {
                ResetAnimation(holder);
                EnqueuePendingAnimationInfo(new RemoveAnimationInfo(holder));
                return true;
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// If the payload list is not empty, RefactoredDefaultItemAnimator returns <code>true</code>.
        /// When this is the case:
        /// <ul>
        /// <li>If you override {@link #animateChange(RecyclerView.ViewHolder, RecyclerView.ViewHolder, int, int, int, int)}, both
        /// ViewHolder arguments will be the same instance.
        /// </li>
        /// <li>
        /// If you are not overriding {@link #animateChange(RecyclerView.ViewHolder, RecyclerView.ViewHolder, int, int, int, int)},
        /// then RefactoredDefaultItemAnimator will call {@link #animateMove(RecyclerView.ViewHolder, int, int, int, int)} and
        /// run a move animation instead.
        /// </li>
        /// </ul>
        /// </summary>
        /// <summary>
        /// Item Animation manager for ADD operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        /// <summary>
        /// Item Animation manager for REMOVE operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        /// <summary>
        /// Item Animation manager for CHANGE operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        protected class DefaultItemChangeAnimationManager : ItemChangeAnimationManager
        {
            public DefaultItemChangeAnimationManager(BaseItemAnimator itemAnimator) : base(itemAnimator)
            {
            }

            protected override void OnCreateChangeAnimationForOldItem(ChangeAnimationInfo info)
            {
                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(info.oldHolder.ItemView);
                animator.SetDuration(GetDuration());
                animator.TranslationX(info.toX - info.fromX);
                animator.TranslationY(info.toY - info.fromY);
                animator.Alpha(0);
                StartActiveItemAnimation(info, info.oldHolder, animator);
            }

            protected override void OnCreateChangeAnimationForNewItem(ChangeAnimationInfo info)
            {
                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(info.newHolder.ItemView);
                animator.TranslationX(0);
                animator.TranslationY(0);
                animator.SetDuration(GetDuration());
                animator.Alpha(1);
                StartActiveItemAnimation(info, info.newHolder, animator);
            }

            protected override void OnAnimationEndedSuccessfully(ChangeAnimationInfo info, RecyclerView.ViewHolder item)
            {
                View view = item.ItemView;
                view.Alpha =1;
                view.TranslationX =0;
                view.TranslationY =0;
            }

            protected override void OnAnimationEndedBeforeStarted(ChangeAnimationInfo info, RecyclerView.ViewHolder item)
            {
                View view = item.ItemView;
                view.Alpha =1;
                view.TranslationX =0;
                view.TranslationY =0;
            }

            protected override void OnAnimationCancel(ChangeAnimationInfo info, RecyclerView.ViewHolder item)
            {
            }

            public override bool AddPendingAnimation(RecyclerView.ViewHolder oldHolder, RecyclerView.ViewHolder newHolder, int fromX, int fromY, int toX, int toY)
            {
                float prevTranslationX = oldHolder.ItemView.TranslationX;
                float prevTranslationY = oldHolder.ItemView.TranslationY;
                float prevAlpha = oldHolder.ItemView.Alpha;
                ResetAnimation(oldHolder);
                int deltaX = (int)(toX - fromX - prevTranslationX);
                int deltaY = (int)(toY - fromY - prevTranslationY);

                // recover prev translation state after ending animation
                oldHolder.ItemView.TranslationX = (prevTranslationX);
                oldHolder.ItemView.TranslationY = (prevTranslationY);
                oldHolder.ItemView.Alpha = (prevAlpha);
                if (newHolder != null)
                {

                    // carry over translation values
                    ResetAnimation(newHolder);
                    newHolder.ItemView.TranslationX = (-deltaX);
                    newHolder.ItemView.TranslationY = (-deltaY);
                    newHolder.ItemView.Alpha = (0);
                }

                EnqueuePendingAnimationInfo(new ChangeAnimationInfo(oldHolder, newHolder, fromX, fromY, toX, toY));
                return true;
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// If the payload list is not empty, RefactoredDefaultItemAnimator returns <code>true</code>.
        /// When this is the case:
        /// <ul>
        /// <li>If you override {@link #animateChange(RecyclerView.ViewHolder, RecyclerView.ViewHolder, int, int, int, int)}, both
        /// ViewHolder arguments will be the same instance.
        /// </li>
        /// <li>
        /// If you are not overriding {@link #animateChange(RecyclerView.ViewHolder, RecyclerView.ViewHolder, int, int, int, int)},
        /// then RefactoredDefaultItemAnimator will call {@link #animateMove(RecyclerView.ViewHolder, int, int, int, int)} and
        /// run a move animation instead.
        /// </li>
        /// </ul>
        /// </summary>
        /// <summary>
        /// Item Animation manager for ADD operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        /// <summary>
        /// Item Animation manager for REMOVE operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        /// <summary>
        /// Item Animation manager for CHANGE operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        // recover prev translation state after ending animation
        // carry over translation values
        /// <summary>
        /// Item Animation manager for MOVE operation  (Same behavior as DefaultItemAnimator class)
        /// </summary>
        protected class DefaultItemMoveAnimationManager : ItemMoveAnimationManager
        {
            public DefaultItemMoveAnimationManager(BaseItemAnimator itemAnimator) : base(itemAnimator)
            {
            }

            protected override void OnCreateAnimation(MoveAnimationInfo info)
            {
                View view = info.holder.ItemView;
                int deltaX = info.toX - info.fromX;
                int deltaY = info.toY - info.fromY;
                if (deltaX != 0)
                {
                    ViewCompat.Animate(view).TranslationX(0);
                }

                if (deltaY != 0)
                {
                    ViewCompat.Animate(view).TranslationY(0);
                }

                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(view);
                animator.SetDuration(GetDuration());
                StartActiveItemAnimation(info, info.holder, animator);
            }

            protected override void OnAnimationEndedSuccessfully(MoveAnimationInfo info, RecyclerView.ViewHolder item)
            {
            }

            protected override void OnAnimationEndedBeforeStarted(MoveAnimationInfo info, RecyclerView.ViewHolder item)
            {
                View view = item.ItemView;
                view.TranslationY = (0);
                view.TranslationX = (0);
            }

            protected override void OnAnimationCancel(MoveAnimationInfo info, RecyclerView.ViewHolder item)
            {
                View view = item.ItemView;
                int deltaX = info.toX - info.fromX;
                int deltaY = info.toY - info.fromY;
                if (deltaX != 0)
                {
                    ViewCompat.Animate(view).TranslationX(0);
                }

                if (deltaY != 0)
                {
                    ViewCompat.Animate(view).TranslationY(0);
                }

                if (deltaX != 0)
                {
                    view.TranslationX = (0);
                }

                if (deltaY != 0)
                {
                    view.TranslationY = (0);
                }
            }

            public override bool AddPendingAnimation(RecyclerView.ViewHolder item, int fromX, int fromY, int toX, int toY)
            {
                View view = item.ItemView;
                fromX += (int)item.ItemView.TranslationX;
                fromY += (int)item.ItemView.TranslationY;
                ResetAnimation(item);
                int deltaX = toX - fromX;
                int deltaY = toY - fromY;
                MoveAnimationInfo info = new MoveAnimationInfo(item, fromX, fromY, toX, toY);
                if (deltaX == 0 && deltaY == 0)
                {
                    DispatchFinished(info, info.holder);
                    info.Clear(info.holder);
                    return false;
                }

                if (deltaX != 0)
                {
                    view.TranslationX = (-deltaX);
                }

                if (deltaY != 0)
                {
                    view.TranslationY = (-deltaY);
                }

                EnqueuePendingAnimationInfo(info);
                return true;
            }
        }
    }
}