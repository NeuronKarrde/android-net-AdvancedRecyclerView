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
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Java.Util;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable
{
    public interface IExpandableItemAdapter
    {
        int GroupCount { get; }
        int GetChildCount(int groupPosition);
        long GetGroupId(int groupPosition);
        long GetChildId(int groupPosition, int childPosition);
        int GetGroupItemViewType(int groupPosition);
        int GetChildItemViewType(int groupPosition, int childPosition);
        RecyclerView.ViewHolder OnCreateGroupViewHolder(ViewGroup parent, int viewType);
        RecyclerView.ViewHolder OnCreateChildViewHolder(ViewGroup parent, int viewType);
        void OnBindGroupViewHolder(RecyclerView.ViewHolder holder, int groupPosition, int viewType);
        void OnBindGroupViewHolder(RecyclerView.ViewHolder holder, int groupPosition, int viewType, IList<Java.Lang.Object> payloads);
        void OnBindChildViewHolder(RecyclerView.ViewHolder holder, int groupPosition, int childPosition, int viewType, IList<Java.Lang.Object> payloads);
        void OnBindChildViewHolder(RecyclerView.ViewHolder holder, int groupPosition, int childPosition, int viewType);
        bool OnCheckCanExpandOrCollapseGroup(RecyclerView.ViewHolder holder, int groupPosition, int x, int y, bool expand);
        bool OnHookGroupExpand(int groupPosition, bool fromUser);
        bool OnHookGroupExpand(int groupPosition, bool fromUser, Java.Lang.Object payload);
        bool OnHookGroupCollapse(int groupPosition, bool fromUser);
        bool OnHookGroupCollapse(int groupPosition, bool fromUser, Java.Lang.Object payload);
        bool GetInitialGroupExpandedState(int groupPosition);
    }
}