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

using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Animator
{
    public abstract class BaseItemAnimator : SimpleItemAnimator
    {
        private IItemAnimatorListener mListener;
        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        public interface IItemAnimatorListener
        {
            void OnRemoveFinished(RecyclerView.ViewHolder item);
            void OnAddFinished(RecyclerView.ViewHolder item);
            void OnMoveFinished(RecyclerView.ViewHolder item);
            void OnChangeFinished(RecyclerView.ViewHolder item);
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public virtual void SetListener(IItemAnimatorListener listener)
        {
            mListener = listener;
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public override void OnAddStarting(RecyclerView.ViewHolder item)
        {
            OnAddStartingImpl(item);
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public override void OnAddFinished(RecyclerView.ViewHolder item)
        {
            OnAddFinishedImpl(item);
            if (mListener != null)
            {
                mListener.OnAddFinished(item);
            }
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public override void OnChangeStarting(RecyclerView.ViewHolder item, bool oldItem)
        {
            OnChangeStartingItem(item, oldItem);
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public override void OnChangeFinished(RecyclerView.ViewHolder item, bool oldItem)
        {
            OnChangeFinishedImpl(item, oldItem);
            if (mListener != null)
            {
                mListener.OnChangeFinished(item);
            }
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public override void OnMoveStarting(RecyclerView.ViewHolder item)
        {
            OnMoveStartingImpl(item);
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public override void OnMoveFinished(RecyclerView.ViewHolder item)
        {
            OnMoveFinishedImpl(item);
            if (mListener != null)
            {
                mListener.OnMoveFinished(item);
            }
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public override void OnRemoveStarting(RecyclerView.ViewHolder item)
        {
            OnRemoveStartingImpl(item);
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public override void OnRemoveFinished(RecyclerView.ViewHolder item)
        {
            OnRemoveFinishedImpl(item);
            if (mListener != null)
            {
                mListener.OnRemoveFinished(item);
            }
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        protected virtual void OnAddStartingImpl(RecyclerView.ViewHolder item)
        {
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        protected virtual void OnAddFinishedImpl(RecyclerView.ViewHolder item)
        {
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        protected virtual void OnChangeStartingItem(RecyclerView.ViewHolder item, bool oldItem)
        {
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        protected virtual void OnChangeFinishedImpl(RecyclerView.ViewHolder item, bool oldItem)
        {
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        protected virtual void OnMoveStartingImpl(RecyclerView.ViewHolder item)
        {
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        protected virtual void OnMoveFinishedImpl(RecyclerView.ViewHolder item)
        {
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        protected virtual void OnRemoveStartingImpl(RecyclerView.ViewHolder item)
        {
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        protected virtual void OnRemoveFinishedImpl(RecyclerView.ViewHolder item)
        {
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public virtual bool DispatchFinishedWhenDone()
        {
            if (!IsRunning)
            {
                DispatchAnimationsFinished();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The interface to be implemented by listeners to animation events from this
        /// ItemAnimator. This is used internally and is not intended for developers to
        /// create directly.
        /// </summary>
        /// <summary>
        /// Internal only:
        /// Sets the listener that must be called when the animator is finished
        /// animating the item (or immediately if no animation happens). This is set
        /// internally and is not intended to be set by external code.
        /// </summary>
        /// <param name="listener">The listener that must be called.</param>
        public virtual bool DebugLogEnabled()
        {
            return false;
        }
    }
}