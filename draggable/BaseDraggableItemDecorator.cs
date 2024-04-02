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

using Android.Graphics;
using Android.Views;
using Android.Views.Animations;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    abstract class BaseDraggableItemDecorator : RecyclerView.ItemDecoration
    {
        private static readonly int RETURN_TO_DEFAULT_POS_ANIMATE_THRESHOLD_DP = 2;
        private static readonly int RETURN_TO_DEFAULT_POS_ANIMATE_THRESHOLD_MSEC = 20;
        private int mReturnToDefaultPositionDuration = 200;
        private readonly int mReturnToDefaultPositionAnimateThreshold;
        private IInterpolator mReturnToDefaultPositionInterpolator;
        protected readonly RecyclerView mRecyclerView;
        protected RecyclerView.ViewHolder mDraggingItemViewHolder;
        public BaseDraggableItemDecorator(RecyclerView recyclerView, RecyclerView.ViewHolder draggingItemViewHolder)
        {
            mRecyclerView = recyclerView;
            mDraggingItemViewHolder = draggingItemViewHolder;
            float displayDensity = recyclerView.Resources.DisplayMetrics.Density;
            mReturnToDefaultPositionAnimateThreshold = (int)(RETURN_TO_DEFAULT_POS_ANIMATE_THRESHOLD_DP * displayDensity + 0.5F);
        }

        public virtual void SetReturnToDefaultPositionAnimationDuration(int duration)
        {
            mReturnToDefaultPositionDuration = duration;
        }

        public virtual void SetReturnToDefaultPositionAnimationInterpolator(IInterpolator interpolator)
        {
            mReturnToDefaultPositionInterpolator = interpolator;
        }

        protected virtual void MoveToDefaultPosition(View targetView, float initialScaleX, float initialScaleY, float initialRotation, float initialAlpha, bool animate)
        {
            float initialTranslationZ = ViewCompat.GetTranslationZ(targetView);
            float durationFactor = DetermineMoveToDefaultPositionAnimationDurationFactor(targetView, initialScaleX, initialScaleY, initialRotation, initialAlpha);
            int animDuration = (int)(mReturnToDefaultPositionDuration * durationFactor);
            if (animate && (animDuration > RETURN_TO_DEFAULT_POS_ANIMATE_THRESHOLD_MSEC))
            {
                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(targetView);
                targetView.ScaleX = initialScaleX;
                targetView.ScaleY = initialScaleY;
                targetView.Rotation = initialRotation;
                targetView.Alpha = initialAlpha;
                ViewCompat.SetTranslationZ(targetView, initialTranslationZ + 1); // to render on top of other items
                animator.Cancel();
                animator.SetDuration(animDuration);
                animator.SetInterpolator(mReturnToDefaultPositionInterpolator);
                animator.TranslationX(0F);
                animator.TranslationY(0F);
                animator.TranslationZ(initialTranslationZ);
                animator.Alpha(1F);
                animator.Rotation(0);
                animator.ScaleX(1F);
                animator.ScaleY(1F);
                animator.SetListener(new AnonymousViewPropertyAnimatorListener(this, initialTranslationZ));
                animator.Start();
            }
            else
            {
                ResetDraggingItemViewEffects(targetView, initialTranslationZ);
            }
        }

        private sealed class AnonymousViewPropertyAnimatorListener : Java.Lang.Object, IViewPropertyAnimatorListener
        {
            public AnonymousViewPropertyAnimatorListener(BaseDraggableItemDecorator parent, float initialTranslationZ)
            {
                this.parent = parent;
                _initialTranslationZ = initialTranslationZ;
            }

            private readonly BaseDraggableItemDecorator parent;
            private readonly float _initialTranslationZ;

            public void OnAnimationStart(View view)
            {
            }

            public void OnAnimationEnd(View view)
            {
                ViewPropertyAnimatorCompat animator = ViewCompat.Animate(view);
                animator.SetListener(null);
                ResetDraggingItemViewEffects(view, _initialTranslationZ);

                // invalidate explicitly to refresh other decorations
                if (view.Parent is RecyclerView)
                {
                    ViewCompat.PostInvalidateOnAnimation((RecyclerView)view.Parent);
                }
            }

            public void OnAnimationCancel(View view)
            {
            }
        }

        // to render on top of other items
        // invalidate explicitly to refresh other decorations
        protected virtual float DetermineMoveToDefaultPositionAnimationDurationFactor(View targetView, float initialScaleX, float initialScaleY, float initialRotation, float initialAlpha)
        {
            float curTranslationX = targetView.TranslationX;
            float curTranslationY = targetView.TranslationY;
            int halfItemWidth = targetView.Width / 2;
            int halfItemHeight = targetView.Height / 2;
            float translationXProportion = (halfItemWidth > 0) ? Math.Abs(curTranslationX / halfItemWidth) : 0;
            float translationYProportion = (halfItemHeight > 0) ? Math.Abs(curTranslationY / halfItemHeight) : 0;
            float scaleProportion = Math.Abs(Math.Max(initialScaleX, initialScaleY) - 1F);
            float rotationProportion = Math.Abs(initialRotation * (1F / 30));
            float alphaProportion = Math.Abs(initialAlpha - 1F);
            float factor = 0;
            factor = Math.Max(factor, translationXProportion);
            factor = Math.Max(factor, translationYProportion);
            factor = Math.Max(factor, scaleProportion);
            factor = Math.Max(factor, rotationProportion);
            factor = Math.Max(factor, alphaProportion);
            factor = Math.Min(factor, 1F);
            return factor;
        }

        // to render on top of other items
        // invalidate explicitly to refresh other decorations
        protected static void ResetDraggingItemViewEffects(View view, float initialTranslationZ)
        {
            view.TranslationX=(0);
            view.TranslationY=(0);
            ViewCompat.SetTranslationZ(view, initialTranslationZ);
            view.Alpha=(1F);
            view.Rotation=(0);
            view.ScaleX=(1F);
            view.ScaleY=(1F);
        }

        // to render on top of other items
        // invalidate explicitly to refresh other decorations
        protected static void SetItemTranslation(RecyclerView rv, RecyclerView.ViewHolder holder, float x, float y)
        {
            RecyclerView.ItemAnimator itemAnimator = rv.GetItemAnimator();
            if (itemAnimator != null)
            {
                itemAnimator.EndAnimation(holder);
            }

            holder.ItemView.TranslationX=(x);
            holder.ItemView.TranslationY=(y);
        }
    }
}