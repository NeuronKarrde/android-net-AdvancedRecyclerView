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

using Android.Runtime;
using Android.Views;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable;
using Java.Util;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Utils
{
    public abstract class AbstractExpandableItemAdapter: RecyclerView.Adapter, IExpandableItemAdapter 
    {
        public AbstractExpandableItemAdapter(){}
        protected AbstractExpandableItemAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }
        
        /// <summary>
        /// This method will not be called.
        /// Override {@link #onCreateGroupViewHolder(android.view.ViewGroup, int)} and
        /// {@link #onCreateChildViewHolder(android.view.ViewGroup, int)} instead.
        /// </summary>
        /// <param name="parent">not used</param>
        /// <param name="viewType">not used</param>
        /// <returns>null</returns>
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            throw new InvalidOperationException("This method should not be called");
        }

        /// <summary>
        /// This method will not be called.
        /// Override {@link #onCreateGroupViewHolder(android.view.ViewGroup, int)} and
        /// {@link #onCreateChildViewHolder(android.view.ViewGroup, int)} instead.
        /// </summary>
        /// <param name="parent">not used</param>
        /// <param name="viewType">not used</param>
        /// <returns>null</returns>
        /// <summary>
        /// This method will not be called.
        /// Override {@link #getGroupId(int)} and {@link #getChildId(int, int)} instead.
        /// </summary>
        /// <param name="position">not used</param>
        /// <returns>{@link RecyclerView#NO_ID}</returns>
        public override long GetItemId(int position)
        {
            return RecyclerView.NoId;
        }

        /// <summary>
        /// This method will not be called.
        /// Override {@link #onCreateGroupViewHolder(android.view.ViewGroup, int)} and
        /// {@link #onCreateChildViewHolder(android.view.ViewGroup, int)} instead.
        /// </summary>
        /// <param name="parent">not used</param>
        /// <param name="viewType">not used</param>
        /// <returns>null</returns>
        /// <summary>
        /// This method will not be called.
        /// Override {@link #getGroupId(int)} and {@link #getChildId(int, int)} instead.
        /// </summary>
        /// <param name="position">not used</param>
        /// <returns>{@link RecyclerView#NO_ID}</returns>
        /// <summary>
        /// This method will not be called.
        /// Override {@link #getGroupItemViewType(int)} and {@link #getChildItemViewType(int, int)} instead.
        /// </summary>
        /// <param name="position">not used</param>
        /// <returns>0</returns>
        public override int GetItemViewType(int position)
        {
            return 0;
        }

        public override int ItemCount => 0;

        public abstract int GroupCount { get; }

        public abstract int GetChildCount(int groupPosition);

        public abstract long GetGroupId(int groupPosition);

        public abstract long GetChildId(int groupPosition, int childPosition);

        public virtual int GetGroupItemViewType(int groupPosition)
        {
            return 0;
        }

        public virtual int GetChildItemViewType(int groupPosition, int childPosition)
        {
            return 0;
        }

        public abstract RecyclerView.ViewHolder OnCreateGroupViewHolder(ViewGroup parent, int viewType);

        public abstract RecyclerView.ViewHolder OnCreateChildViewHolder(ViewGroup parent, int viewType);

        public abstract void OnBindGroupViewHolder(RecyclerView.ViewHolder holder, int groupPosition, int viewType);

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
        }

        public void OnBindGroupViewHolder(RecyclerView.ViewHolder holder, int groupPosition, int viewType, IList<Java.Lang.Object> payloads)
        {
            OnBindGroupViewHolder(holder, groupPosition, viewType);
        }

        public void OnBindChildViewHolder(RecyclerView.ViewHolder holder, int groupPosition, int childPosition, int viewType, IList<Java.Lang.Object> payloads)
        {
            OnBindChildViewHolder(holder, groupPosition, childPosition, viewType);
        }

        public abstract void OnBindChildViewHolder(RecyclerView.ViewHolder holder, int groupPosition, int childPosition,
            int viewType);

        public abstract bool OnCheckCanExpandOrCollapseGroup(RecyclerView.ViewHolder holder, int groupPosition, int x,
            int y, bool expand);

        public virtual bool OnHookGroupExpand(int groupPosition, bool fromUser)
        {
            return true;
        }

        public bool OnHookGroupExpand(int groupPosition, bool fromUser, Java.Lang.Object payload)
        {
            return OnHookGroupExpand(groupPosition, fromUser);
        }

        public virtual bool OnHookGroupCollapse(int groupPosition, bool fromUser)
        {
            return true;
        }

        public bool OnHookGroupCollapse(int groupPosition, bool fromUser, Java.Lang.Object payload)
        {
            return OnHookGroupCollapse(groupPosition, fromUser);
        }

        public virtual bool GetInitialGroupExpandedState(int groupPosition)
        {
            return false;
        }
    }
}