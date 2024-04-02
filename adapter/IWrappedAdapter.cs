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

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter
{
    /// <summary>
    /// An interface provides better methods for wrapped adapters.
    /// </summary>
    public interface IWrappedAdapter
    {
        /// <summary>
        /// onViewAttachedToWindow() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">Holder of the view being attached</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onViewAttachedToWindow(RecyclerView.ViewHolder)</remarks>
        void OnViewAttachedToWindow(RecyclerView.ViewHolder holder, int viewType);
        /// <summary>
        /// onViewAttachedToWindow() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">Holder of the view being attached</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onViewAttachedToWindow(RecyclerView.ViewHolder)</remarks>
        /// <summary>
        /// onViewDetachedFromWindow() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">Holder of the view being detached</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onViewDetachedFromWindow(RecyclerView.ViewHolder)</remarks>
        void OnViewDetachedFromWindow(RecyclerView.ViewHolder holder, int viewType);
        /// <summary>
        /// onViewAttachedToWindow() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">Holder of the view being attached</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onViewAttachedToWindow(RecyclerView.ViewHolder)</remarks>
        /// <summary>
        /// onViewDetachedFromWindow() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">Holder of the view being detached</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onViewDetachedFromWindow(RecyclerView.ViewHolder)</remarks>
        /// <summary>
        /// onViewRecycled() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">The ViewHolder for the view being recycled</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onViewRecycled(RecyclerView.ViewHolder)</remarks>
        void OnViewRecycled(RecyclerView.ViewHolder holder, int viewType);
        /// <summary>
        /// onViewAttachedToWindow() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">Holder of the view being attached</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onViewAttachedToWindow(RecyclerView.ViewHolder)</remarks>
        /// <summary>
        /// onViewDetachedFromWindow() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">Holder of the view being detached</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onViewDetachedFromWindow(RecyclerView.ViewHolder)</remarks>
        /// <summary>
        /// onViewRecycled() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">The ViewHolder for the view being recycled</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onViewRecycled(RecyclerView.ViewHolder)</remarks>
        /// <summary>
        /// onFailedToRecycleView() with unwrapped item view type parameter.
        /// </summary>
        /// <param name="holder">The ViewHolder containing the View that could not be recycled due to its
        ///                 transient state.</param>
        /// <param name="viewType">Unwrapped view type. Use this instead of #{{@link RecyclerView.ViewHolder#getItemViewType()}}.</param>
        /// <returns>True if the View should be recycled, false otherwise. Note that if this method
        /// returns <code>true</code>, RecyclerView <em>will ignore</em> the transient state of
        /// the View and recycle it regardless. If this method returns <code>false</code>,
        /// RecyclerView will check the View's transient state again before giving a final decision.
        /// Default implementation returns false.</returns>
        /// <remarks>@seeAndroidX.RecyclerView.widget.RecyclerView.Adapter#onFailedToRecycleView(RecyclerView.ViewHolder)</remarks>
        bool OnFailedToRecycleView(RecyclerView.ViewHolder holder, int viewType);
    }
}