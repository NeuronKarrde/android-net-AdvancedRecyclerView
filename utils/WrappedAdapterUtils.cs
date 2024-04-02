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
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Utils
{
    public class WrappedAdapterUtils
    {
        private WrappedAdapterUtils()
        {
        }

        public static void InvokeOnViewRecycled(RecyclerView.Adapter adapter, RecyclerView.ViewHolder holder, int viewType)
        {
            if (adapter is IWrapperAdapter)
            {
                ((IWrapperAdapter)adapter).OnViewRecycled(holder, viewType);
            }
            else
            {
                adapter.OnViewRecycled(holder);
            }
        }

        public static bool InvokeOnFailedToRecycleView(RecyclerView.Adapter adapter, RecyclerView.ViewHolder holder, int viewType)
        {
            if (adapter is IWrappedAdapter)
            {
                return ((IWrappedAdapter)adapter).OnFailedToRecycleView(holder, viewType);
            }
            else
            {
                return adapter.OnFailedToRecycleView(holder);
            }
        }

        public static void InvokeOnViewAttachedToWindow(RecyclerView.Adapter adapter, RecyclerView.ViewHolder holder, int viewType)
        {
            if (adapter is IWrappedAdapter)
            {
                ((IWrappedAdapter)adapter).OnViewAttachedToWindow(holder, viewType);
            }
            else
            {
                adapter.OnViewAttachedToWindow(holder);
            }
        }

        public static void InvokeOnViewDetachedFromWindow(RecyclerView.Adapter adapter, RecyclerView.ViewHolder holder, int viewType)
        {
            if (adapter is IWrappedAdapter)
            {
                ((IWrappedAdapter)adapter).OnViewDetachedFromWindow(holder, viewType);
            }
            else
            {
                adapter.OnViewDetachedFromWindow(holder);
            }
        }
    }
}