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
using Android.Animation;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Java.Lang;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Animator.Impl
{
    public abstract class BaseItemAnimationManager<T> where T : ItemAnimationInfo
    {
        private static ITimeInterpolator sDefaultInterpolator;
        protected readonly BaseItemAnimator mItemAnimator;
        protected readonly IList<T> mPending;
        protected readonly IList<IList<T>> mDeferredReadySets;
        protected readonly List<RecyclerView.ViewHolder> mActive;
        public BaseItemAnimationManager(BaseItemAnimator itemAnimator)
        {
            mItemAnimator = itemAnimator;
            mPending = new List<T>();
            mActive = new List<RecyclerView.ViewHolder>();
            mDeferredReadySets = new List<IList<T>>();
        }

        protected bool DebugLogEnabled()
        {
            return mItemAnimator.DebugLogEnabled();
        }

        public virtual bool HasPending()
        {
            return mPending.Any();
        }

        public virtual bool IsRunning()
        {
            return mPending.Any() || mActive.Any() || mDeferredReadySets.Any();
        }

        public virtual bool RemoveFromActive(RecyclerView.ViewHolder item)
        {
            return mActive.Remove(item);
        }

        public virtual void CancelAllStartedAnimations()
        {
            List<RecyclerView.ViewHolder> active = mActive;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                View view = active[i].ItemView;
                ViewCompat.Animate(view).Cancel();
            }
        }
        
        public void RunPendingAnimations(bool deferred, long deferredDelay)
        {
            List<T> ready = new List<T>(mPending);
            mPending.Clear();

            if (deferred)
            {
                mDeferredReadySets.Add(ready);

                var process = new AnonimousPendingAnimationRunnable(this, ready);
                View view = ready[0].GetAvailableViewHolder().ItemView;
                ViewCompat.PostOnAnimationDelayed(view, process, deferredDelay);
            }
            else
            {
                foreach (T info in ready)
                {
                    CreateAnimation(info);
                }
                ready.Clear();
            }
        }

        class AnonimousPendingAnimationRunnable : Java.Lang.Object, IRunnable
        {
            private readonly BaseItemAnimationManager<T> _parent;
            private readonly List<T> _ready;

            public AnonimousPendingAnimationRunnable(BaseItemAnimationManager<T> parent, List<T> ready)
            {
                _parent = parent;
                _ready = ready;
            }
            public void Run()
            {
                foreach (T info in _ready)
                {
                    _parent.CreateAnimation(info);
                }
                _ready.Clear();
                _parent.mDeferredReadySets.Remove(_ready);
            }
        }

        public abstract void DispatchStarting(T info, RecyclerView.ViewHolder item);
        public abstract void DispatchFinished(T info, RecyclerView.ViewHolder item);
        public abstract long GetDuration();
        public abstract void SetDuration(long duration);
        public virtual void EndPendingAnimations(RecyclerView.ViewHolder item)
        {
            IList<T> pending = mPending;
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                T info = pending[i];
                if (EndNotStartedAnimation(info, item) && (item != null))
                {
                    pending.RemoveAt(i);
                }
            }

            if (item == null)
            {
                pending.Clear();
            }
        }

        public virtual void EndAllPendingAnimations()
        {
            EndPendingAnimations(null);
        }

        public virtual void EndDeferredReadyAnimations(RecyclerView.ViewHolder item)
        {
            for (int i = mDeferredReadySets.Count - 1; i >= 0; i--)
            {
                IList<T> ready = mDeferredReadySets[i];
                for (int j = ready.Count - 1; j >= 0; j--)
                {
                    T info = ready[j];
                    if (EndNotStartedAnimation(info, item) && (item != null))
                    {
                        ready.RemoveAt(j);
                    }
                }

                if (item == null)
                {
                    ready.Clear();
                }

                if (!ready.Any())
                {
                    mDeferredReadySets.Remove(ready);
                }
            }
        }

        public virtual void EndAllDeferredReadyAnimations()
        {
            EndDeferredReadyAnimations(null);
        }

        /*package*/
        protected virtual void CreateAnimation(T info)
        {
            OnCreateAnimation(info);
        }

        /*package*/
        protected virtual void EndAnimation(RecyclerView.ViewHolder holder)
        {
            mItemAnimator.EndAnimation(holder);
        }

        /*package*/
        protected virtual void ResetAnimation(RecyclerView.ViewHolder holder)
        {
            if (sDefaultInterpolator == null)
            {
                sDefaultInterpolator = new ValueAnimator().Interpolator;
            }

            holder.ItemView.Animate().SetInterpolator(sDefaultInterpolator);
            EndAnimation(holder);
        }

        /*package*/
        protected virtual void DispatchFinishedWhenDone()
        {
            mItemAnimator.DispatchFinishedWhenDone();
        }

        /*package*/
        protected virtual void EnqueuePendingAnimationInfo(T info)
        {
            mPending.Add(info);
        }

        /*package*/
        protected virtual void StartActiveItemAnimation(T info, RecyclerView.ViewHolder holder, ViewPropertyAnimatorCompat animator)
        {
            animator.SetListener(new BaseAnimatorListener(this, info, holder, animator));
            AddActiveAnimationTarget(holder);
            animator.Start();
        }

        /*package*/
        private void AddActiveAnimationTarget(RecyclerView.ViewHolder item)
        {
            if (item == null)
            {
                throw new InvalidOperationException("item is null");
            }

            mActive.Add(item);
        }

        /*package*/
        protected abstract void OnCreateAnimation(T info);
        /*package*/
        protected abstract void OnAnimationEndedSuccessfully(T info, RecyclerView.ViewHolder item);
        /*package*/
        protected abstract void OnAnimationEndedBeforeStarted(T info, RecyclerView.ViewHolder item);
        /*package*/
        protected abstract void OnAnimationCancel(T info, RecyclerView.ViewHolder item);
        /*package*/
        protected abstract bool EndNotStartedAnimation(T info, RecyclerView.ViewHolder item);
        /*package*/
        protected class BaseAnimatorListener : Java.Lang.Object, IViewPropertyAnimatorListener
        {
            private BaseItemAnimationManager<T> mManager;
            private T mAnimationInfo;
            private RecyclerView.ViewHolder mHolder;
            private ViewPropertyAnimatorCompat mAnimator;
            public BaseAnimatorListener(BaseItemAnimationManager<T> manager, T info, RecyclerView.ViewHolder holder, ViewPropertyAnimatorCompat animator)
            {
                mManager = manager;
                mAnimationInfo = info;
                mHolder = holder;
                mAnimator = animator;
            }

            public virtual void OnAnimationStart(View view)
            {
                mManager.DispatchStarting(mAnimationInfo, mHolder);
            }

            public virtual void OnAnimationEnd(View view)
            {
                BaseItemAnimationManager<T> manager = mManager;
                T info = mAnimationInfo;
                RecyclerView.ViewHolder holder = mHolder;
                mAnimator.SetListener(null);
                mManager = null;
                mAnimationInfo = null;
                mHolder = null;
                mAnimator = null;
                manager.OnAnimationEndedSuccessfully(info, holder);
                manager.DispatchFinished(info, holder);
                info.Clear(holder);
                manager.mActive.Remove(holder);
                manager.DispatchFinishedWhenDone();
            }

            public virtual void OnAnimationCancel(View view)
            {
                mManager.OnAnimationCancel(mAnimationInfo, mHolder);
            }
        }
    }
}