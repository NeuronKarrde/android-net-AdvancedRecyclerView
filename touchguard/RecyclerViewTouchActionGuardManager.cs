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
using Android.Util;
using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Touchguard
{
    /// <summary>
    /// Hooks touch events to avoid unexpected scrolling.
    /// </summary>
    public class RecyclerViewTouchActionGuardManager
    {
        private static readonly string TAG = "ARVTouchActionGuardMgr";
        private static readonly bool LOCAL_LOGV = false;
        private static readonly bool LOCAL_LOGD = false;
        private RecyclerView.IOnItemTouchListener mInternalUseOnItemTouchListener;
        private RecyclerView mRecyclerView;
        private bool mGuarding;
        private int mInitialTouchY;
        private int mLastTouchY;
        private int mTouchSlop;
        private bool mInterceptScrollingWhileAnimationRunning;
        private bool _enabled;
        /// <summary>
        /// Constructor.
        /// </summary>
        public RecyclerViewTouchActionGuardManager()
        {
            mInternalUseOnItemTouchListener = new AnonymousOnItemTouchListener(this);
        }

        private sealed class AnonymousOnItemTouchListener : Java.Lang.Object, RecyclerView.IOnItemTouchListener
        {
            public AnonymousOnItemTouchListener(RecyclerViewTouchActionGuardManager parent)
            {
                this.parent = parent;
            }

            private readonly RecyclerViewTouchActionGuardManager parent;
            public bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
            {
                return this.OnInterceptTouchEvent(rv, e);
            }

            public void OnTouchEvent(RecyclerView rv, MotionEvent e)
            {
                this.OnTouchEvent(rv, e);
            }

            public void OnRequestDisallowInterceptTouchEvent(bool disallowIntercept)
            {
            }
        }

        /// <summary>
        /// Indicates this manager instance has released or not.
        /// </summary>
        /// <returns>True if this manager instance has released</returns>
        public virtual bool IsReleased()
        {
            return (mInternalUseOnItemTouchListener == null);
        }

        /// <summary>
        /// Attaches {@link AndroidX.RecyclerView.widget.RecyclerView} instance.
        /// </summary>
        /// <param name="rv">The {@link AndroidX.RecyclerView.widget.RecyclerView} instance</param>
        public virtual void AttachRecyclerView(RecyclerView rv)
        {
            if (IsReleased())
            {
                throw new InvalidOperationException("Accessing released object");
            }

            if (mRecyclerView != null)
            {
                throw new InvalidOperationException("RecyclerView instance has already been set");
            }

            mRecyclerView = rv;
            mRecyclerView.AddOnItemTouchListener(mInternalUseOnItemTouchListener);
            mTouchSlop = ViewConfiguration.Get(rv.Context).ScaledTouchSlop;
        }

        /// <summary>
        /// Detach the {@link AndroidX.RecyclerView.widget.RecyclerView} instance and release internal field references.
        /// 
        /// This method should be called in order to avoid memory leaks.
        /// </summary>
        public virtual void Release()
        {
            if (mRecyclerView != null && mInternalUseOnItemTouchListener != null)
            {
                mRecyclerView.RemoveOnItemTouchListener(mInternalUseOnItemTouchListener);
            }

            mInternalUseOnItemTouchListener = null;
            mRecyclerView = null;
        }

        /*package*/
        protected virtual bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
        {
            if (!_enabled)
            {
                return false;
            }

            var action = e.ActionMasked;
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onInterceptTouchEvent() action = " + action);
            }

            switch (action)
            {
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    HandleActionUpOrCancel();
                    break;
                case MotionEventActions.Down:
                    HandleActionDown(e);
                    break;
                case MotionEventActions.Move:
                    if (HandleActionMove(rv, e))
                    {
                        return true;
                    }

                    break;
            }

            return false;
        }

        /*package*/
        protected virtual void OnTouchEvent(RecyclerView rv, MotionEvent e)
        {
            if (!_enabled)
            {
                return;
            }

            var action = e.ActionMasked;
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onTouchEvent() action = " + action);
            }

            switch (action)
            {
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    HandleActionUpOrCancel();
                    break;
            }
        }

        private bool HandleActionMove(RecyclerView rv, MotionEvent e)
        {
            if (!mGuarding)
            {
                mLastTouchY = (int)(e.GetY() + 0.5F);
                int distance = mLastTouchY - mInitialTouchY;
                if (mInterceptScrollingWhileAnimationRunning && (Math.Abs(distance) > mTouchSlop) && rv.IsAnimating)
                {

                    // intercept vertical move touch events while animation is running
                    mGuarding = true;
                }
            }

            return mGuarding;
        }

        private void HandleActionUpOrCancel()
        {
            mGuarding = false;
            mInitialTouchY = 0;
            mLastTouchY = 0;
        }

        private void HandleActionDown(MotionEvent e)
        {
            mInitialTouchY = mLastTouchY = (int)(e.GetY() + 0.5F);
            mGuarding = false;
        }

        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                {
                    return;
                }

                _enabled = value;
                if (!_enabled)
                {
                    HandleActionUpOrCancel();
                }
            }
        }

        /// <summary>
        /// Sets whether to use interception of "vertical scroll while animation running".
        /// </summary>
        /// <param name="enabled">enabled / disabled</param>
        public virtual void SetInterceptVerticalScrollingWhileAnimationRunning(bool enabled)
        {
            mInterceptScrollingWhileAnimationRunning = enabled;
        }

        /// <summary>
        /// Checks whether the interception of "vertical scroll while animation running" is enabled.
        /// </summary>
        /// <returns>enabled / disabled</returns>
        public virtual bool IsInterceptScrollingWhileAnimationRunning()
        {
            return mInterceptScrollingWhileAnimationRunning;
        }
    }
}