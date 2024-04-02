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
using Java.Util;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Event
{
    public abstract class BaseRecyclerViewEventDistributor<T>
    {
        protected bool mReleased;
        protected RecyclerView mRecyclerView;
        protected IList<T> mListeners;
        protected bool mPerformingClearMethod;
        public BaseRecyclerViewEventDistributor()
        {
        }

        public virtual RecyclerView GetRecyclerView()
        {
            return mRecyclerView;
        }

        public virtual void Release()
        {
            if (mReleased)
            {
                return;
            }

            mReleased = true;
            Clear(true);
            OnRelease();
        }

        public virtual bool IsReleased()
        {
            return mReleased;
        }

        public virtual void AttachRecyclerView(RecyclerView rv)
        {
            string METHOD_NAME = "attachRecyclerView()";
            VerifyIsNotReleased(METHOD_NAME);
            VerifyIsNotPerformingClearMethod(METHOD_NAME);
            OnRecyclerViewAttached(rv);
        }

        public virtual bool Add(T listener)
        {
            return Add(listener, -1);
        }

        public virtual bool Add(T listener, int index)
        {
            string METHOD_NAME = "add()";
            VerifyIsNotReleased(METHOD_NAME);
            VerifyIsNotPerformingClearMethod(METHOD_NAME);
            if (mListeners == null)
            {
                mListeners = new List<T>();
            }

            if (!mListeners.Contains(listener))
            {
                if (index < 0)
                {

                    // append to the tail of the list
                    mListeners.Add(listener);
                }
                else
                {

                    // insert to the specified position
                    mListeners.Insert(index, listener);
                }


                // raise onAddedToEventDistributor() event
                if (listener is RecyclerViewEventDistributorListener<T> distributorListener)
                {
                    distributorListener.OnAddedToEventDistributor(this);
                }
            }

            return true;
        }

        public virtual bool Remove(T listener)
        {
            string METHOD_NAME = "remove()";
            VerifyIsNotPerformingClearMethod(METHOD_NAME);
            VerifyIsNotReleased(METHOD_NAME);
            if (mListeners == null)
            {
                return false;
            }

            bool removed = mListeners.Remove(listener);
            if (removed)
            {

                // raise onRemovedFromEventDistributor() event
                if (listener is RecyclerViewEventDistributorListener<T> distributorListener)
                {
                    distributorListener.OnRemovedFromEventDistributor(this);
                }
            }

            return removed;
        }

        public virtual void Clear()
        {
            Clear(false);
        }

        protected virtual void Clear(bool calledFromRelease)
        {
            string METHOD_NAME = "clear()";
            if (!calledFromRelease)
            {
                VerifyIsNotReleased(METHOD_NAME);
            }

            VerifyIsNotPerformingClearMethod(METHOD_NAME);
            if (mListeners == null)
            {
                return;
            }

            try
            {
                mPerformingClearMethod = true;
                int n = mListeners.Count;
                for (int i = n - 1; i >= 0; i--)
                {
                    T listener = mListeners[i];
                    mListeners.RemoveAt(i);

                    // raise onRemovedFromEventDistributor() event
                    if (listener is RecyclerViewEventDistributorListener<T> distributorListener)
                    {
                        distributorListener.OnRemovedFromEventDistributor(this);
                    }
                }
            }
            finally
            {
                mPerformingClearMethod = false;
            }
        }

        public virtual int Size()
        {
            if (mListeners != null)
            {
                return mListeners.Count;
            }
            else
            {
                return 0;
            }
        }

        public virtual bool Contains(T listener)
        {
            if (mListeners != null)
            {
                return mListeners.Contains(listener);
            }
            else
            {
                return false;
            }
        }

        protected virtual void OnRelease()
        {
            mRecyclerView = null;
            mListeners = null;
            mPerformingClearMethod = false;
        }

        protected virtual void OnRecyclerViewAttached(RecyclerView rv)
        {
            mRecyclerView = rv;
        }

        protected virtual void VerifyIsNotPerformingClearMethod(string methodName)
        {
            if (mPerformingClearMethod)
            {
                throw new InvalidOperationException(methodName + " can not be called while performing the clear() method");
            }
        }

        protected virtual void VerifyIsNotReleased(string methodName)
        {
            if (mReleased)
            {
                throw new InvalidOperationException(methodName + " can not be called after release() method called");
            }
        }
    }
}