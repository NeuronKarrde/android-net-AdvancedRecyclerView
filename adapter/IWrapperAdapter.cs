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
    /// An RecyclerView adapter which wraps another adapter(s).
    /// </summary>
    public interface IWrapperAdapter : IWrappedAdapter
    {
        /// <summary>
        /// Unwraps position. This method converts the passed wrapped position to child adapter's position.
        /// </summary>
        /// <param name="dest">The destination</param>
        /// <param name="position">The wrapped position to be unwrapped</param>
        void UnwrapPosition(UnwrapPositionResult dest, int position);
        /// <summary>
        /// Unwraps position. This method converts the passed wrapped position to child adapter's position.
        /// </summary>
        /// <param name="dest">The destination</param>
        /// <param name="position">The wrapped position to be unwrapped</param>
        /// <summary>
        /// Wraps position. This method converts the passed child adapter's position to wrapped position.
        /// </summary>
        /// <param name="pathSegment">The path segment of the child adapter</param>
        /// <param name="position">The child adapter's position to be wrapped</param>
        /// <returns>Wrapped position</returns>
        int WrapPosition(AdapterPathSegment pathSegment, int position);
        /// <summary>
        /// Unwraps position. This method converts the passed wrapped position to child adapter's position.
        /// </summary>
        /// <param name="dest">The destination</param>
        /// <param name="position">The wrapped position to be unwrapped</param>
        /// <summary>
        /// Wraps position. This method converts the passed child adapter's position to wrapped position.
        /// </summary>
        /// <param name="pathSegment">The path segment of the child adapter</param>
        /// <param name="position">The child adapter's position to be wrapped</param>
        /// <returns>Wrapped position</returns>
        /// <summary>
        /// Gets wrapped children adapters.
        /// </summary>
        /// <param name="adapters">The destination</param>
        void GetWrappedAdapters(List<RecyclerView.Adapter> adapters);
        /// <summary>
        /// Unwraps position. This method converts the passed wrapped position to child adapter's position.
        /// </summary>
        /// <param name="dest">The destination</param>
        /// <param name="position">The wrapped position to be unwrapped</param>
        /// <summary>
        /// Wraps position. This method converts the passed child adapter's position to wrapped position.
        /// </summary>
        /// <param name="pathSegment">The path segment of the child adapter</param>
        /// <param name="position">The child adapter's position to be wrapped</param>
        /// <returns>Wrapped position</returns>
        /// <summary>
        /// Gets wrapped children adapters.
        /// </summary>
        /// <param name="adapters">The destination</param>
        /// <summary>
        /// Releases bounded resources.
        /// </summary>
        void Release();
    }
}