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
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Composedadapter
{
    class ComposedChildAdapterDataObserver : BridgeAdapterDataObserver
    {
        public ComposedChildAdapterDataObserver(ISubscriber subscriber, RecyclerView.Adapter sourceAdapter) 
            : base(subscriber, sourceAdapter, new List<ComposedChildAdapterTag>())
        {
        }

        private IList<ComposedChildAdapterTag> GetChildAdapterTags()
        {
            return (IList<ComposedChildAdapterTag>)GetTag();
        }

        public virtual void RegisterChildAdapterTag(ComposedChildAdapterTag tag)
        {
            GetChildAdapterTags().Add(tag);
        }

        public virtual void UnregisterChildAdapterTag(ComposedChildAdapterTag tag)
        {
            GetChildAdapterTags().Remove(tag);
        }

        public virtual bool HasChildAdapters()
        {
            return GetChildAdapterTags().Any();
        }

        public virtual void Release()
        {
            GetChildAdapterTags().Clear();
        }
    }
}