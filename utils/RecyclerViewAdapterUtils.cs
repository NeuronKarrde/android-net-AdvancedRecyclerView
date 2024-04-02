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
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Utils
{
    public class RecyclerViewAdapterUtils
    {
        private RecyclerViewAdapterUtils()
        {
        }

        /// <summary>
        /// Gets parent RecyclerView instance.
        /// </summary>
        /// <param name="view">Child view of the RecyclerView's item</param>
        /// <returns>Parent RecyclerView instance</returns>
        public static RecyclerView GetParentRecyclerView(View view)
        {
            if (view == null)
            {
                return null;
            }

            var parent = view.Parent;
            if (parent is RecyclerView)
            {
                return (RecyclerView)parent;
            }
            else if (parent is View)
            {
                return GetParentRecyclerView((View)parent);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets directly child of RecyclerView (== {@link AndroidX.RecyclerView.widget.RecyclerView.ViewHolder#itemView}})
        /// </summary>
        /// <param name="view">Child view of the RecyclerView's item</param>
        /// <returns>Item view</returns>
        public static View GetParentViewHolderItemView(View view)
        {
            RecyclerView rv = GetParentRecyclerView(view);
            if (rv == null)
            {
                return null;
            }

            return rv.FindContainingItemView(view);
        }

        /// <summary>
        /// Gets {@link AndroidX.RecyclerView.widget.RecyclerView.ViewHolder}.
        /// </summary>
        /// <param name="view">Child view of the RecyclerView's item</param>
        /// <returns>ViewHolder</returns>
        public static RecyclerView.ViewHolder GetViewHolder(View view)
        {
            RecyclerView rv = GetParentRecyclerView(view);
            if (rv == null)
            {
                return null;
            }

            return rv.FindContainingViewHolder(view);
        }
    }
}