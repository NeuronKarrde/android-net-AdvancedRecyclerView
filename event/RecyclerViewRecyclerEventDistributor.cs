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
using Java.Lang.Ref;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Event
{
    public class RecyclerViewRecyclerEventDistributor : BaseRecyclerViewEventDistributor<RecyclerView.IRecyclerListener>
    {
        private InternalRecyclerListener mInternalRecyclerListener;
        public RecyclerViewRecyclerEventDistributor() : base()
        {
            mInternalRecyclerListener = new InternalRecyclerListener(this);
        }

        protected override void OnRecyclerViewAttached(RecyclerView rv)
        {
            base.OnRecyclerViewAttached(rv);
            rv.SetRecyclerListener(mInternalRecyclerListener);
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            if (mInternalRecyclerListener != null)
            {
                mInternalRecyclerListener.Release();
                mInternalRecyclerListener = null;
            }
        }

        /*package*/
        protected virtual void HandleOnViewRecycled(RecyclerView.ViewHolder holder)
        {
            if (mListeners == null)
            {
                return;
            }

            foreach (RecyclerView.IRecyclerListener listener in mListeners)
            {
                listener.OnViewRecycled(holder);
            }
        }

        /*package*/
        private class InternalRecyclerListener : Java.Lang.Object, RecyclerView.IRecyclerListener
        {
            private readonly WeakReference<RecyclerViewRecyclerEventDistributor> mRefDistributor;
            public InternalRecyclerListener(RecyclerViewRecyclerEventDistributor distributor) : base()
            {
                mRefDistributor = new WeakReference<RecyclerViewRecyclerEventDistributor>(distributor);
            }

            public virtual void OnViewRecycled(RecyclerView.ViewHolder holder)
            {
                if (mRefDistributor.TryGetTarget(out RecyclerViewRecyclerEventDistributor distributor))
                {
                    distributor.HandleOnViewRecycled(holder);
                }
            }

            public virtual void Release()
            {
                mRefDistributor.SetTarget(null);
            }
        }
    }
}