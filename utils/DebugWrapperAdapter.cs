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
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using AndroidX.RecyclerView.Widget;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Utils
{
    /// <summary>
    /// A wrapper adapter for debugging purpose.
    /// </summary>
    public class DebugWrapperAdapter : SimpleWrapperAdapter
    {
        public static readonly int FLAG_VERIFY_WRAP_POSITION = 1;
        public static readonly int FLAG_VERIFY_UNWRAP_POSITION = 1 << 1;
        public static readonly int FLAGS_ALL_DEBUG_FEATURES = FLAG_VERIFY_WRAP_POSITION | FLAG_VERIFY_UNWRAP_POSITION;
        private int mFlags = FLAGS_ALL_DEBUG_FEATURES;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapter">The debug target adapter</param>
        public DebugWrapperAdapter(RecyclerView.Adapter adapter) : base(adapter)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapter">The debug target adapter</param>
        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// This class also invokes {@link WrapperAdapter#unwrapPosition(UnwrapPositionResult, int)} of the child adapter when
        /// verify option is enabled.
        /// If inconsistency has been detected, <code>IllegalStateException</code> will be thrown.
        /// </summary>
        public int WrapPosition(AdapterPathSegment pathSegment, int position)
        {
            if (((mFlags & FLAG_VERIFY_WRAP_POSITION) != 0) && (GetWrappedAdapter() is IWrapperAdapter))
            {
                IWrapperAdapter wrapperAdapter = (IWrapperAdapter)GetWrappedAdapter();
                int wrappedPosition = wrapperAdapter.WrapPosition(pathSegment, position);
                if (wrappedPosition != RecyclerView.NoPosition)
                {
                    UnwrapPositionResult tmpResult = new UnwrapPositionResult();
                    wrapperAdapter.UnwrapPosition(tmpResult, wrappedPosition);
                    if (tmpResult.position != position)
                    {
                        string wrappedClassName = GetWrappedAdapter().GetType().Name;
                        throw new InvalidOperationException("Found a WrapperAdapter implementation issue while executing wrapPosition(): " + wrappedClassName + "\n" + "wrapPosition(" + position + ") returns " + wrappedPosition + ", but " + "unwrapPosition(" + wrappedPosition + ") returns " + tmpResult.position);
                    }
                }
            }

            return base.WrapPosition(pathSegment, position);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapter">The debug target adapter</param>
        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// This class also invokes {@link WrapperAdapter#unwrapPosition(UnwrapPositionResult, int)} of the child adapter when
        /// verify option is enabled.
        /// If inconsistency has been detected, <code>IllegalStateException</code> will be thrown.
        /// </summary>
        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// This class also invokes {@link WrapperAdapter#wrapPosition(AdapterPathSegment, int)} of the child adapter when
        /// verify option is enabled.
        /// If inconsistency has been detected, <code>IllegalStateException</code> will be thrown.
        /// </summary>
        public override void UnwrapPosition(UnwrapPositionResult dest, int position)
        {
            if (((mFlags & FLAG_VERIFY_UNWRAP_POSITION) != 0) && (GetWrappedAdapter() is IWrapperAdapter))
            {
                IWrapperAdapter wrapperAdapter = (IWrapperAdapter)GetWrappedAdapter();
                UnwrapPositionResult tmpResult = new UnwrapPositionResult();
                wrapperAdapter.UnwrapPosition(tmpResult, position);
                if (tmpResult.IsValid())
                {
                    AdapterPathSegment segment = new AdapterPathSegment(tmpResult.adapter, tmpResult.tag);
                    int reWrappedPosition = wrapperAdapter.WrapPosition(segment, tmpResult.position);
                    if (position != reWrappedPosition)
                    {
                        string wrappedClassName = GetWrappedAdapter().GetType().Name;
                        throw new InvalidOperationException("Found a WrapperAdapter implementation issue while executing unwrapPosition(): " + wrappedClassName + "\n" + "unwrapPosition(" + position + ") returns " + tmpResult.position + ", but " + "wrapPosition(" + tmpResult.position + ") returns " + reWrappedPosition);
                    }
                }
            }

            base.UnwrapPosition(dest, position);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapter">The debug target adapter</param>
        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// This class also invokes {@link WrapperAdapter#unwrapPosition(UnwrapPositionResult, int)} of the child adapter when
        /// verify option is enabled.
        /// If inconsistency has been detected, <code>IllegalStateException</code> will be thrown.
        /// </summary>
        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// This class also invokes {@link WrapperAdapter#wrapPosition(AdapterPathSegment, int)} of the child adapter when
        /// verify option is enabled.
        /// If inconsistency has been detected, <code>IllegalStateException</code> will be thrown.
        /// </summary>
        /// <summary>
        /// Sets setting flags.
        /// </summary>
        /// <param name="flags">Bit-ORof debug feature flags.</param>
        public virtual void SetSettingFlags(int flags)
        {
            mFlags = flags;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapter">The debug target adapter</param>
        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// This class also invokes {@link WrapperAdapter#unwrapPosition(UnwrapPositionResult, int)} of the child adapter when
        /// verify option is enabled.
        /// If inconsistency has been detected, <code>IllegalStateException</code> will be thrown.
        /// </summary>
        /// <summary>
        /// {@inheritDoc}
        /// <p>
        /// This class also invokes {@link WrapperAdapter#wrapPosition(AdapterPathSegment, int)} of the child adapter when
        /// verify option is enabled.
        /// If inconsistency has been detected, <code>IllegalStateException</code> will be thrown.
        /// </summary>
        /// <summary>
        /// Sets setting flags.
        /// </summary>
        /// <param name="flags">Bit-ORof debug feature flags.</param>
        /// <summary>
        /// Returns current setting flags.
        /// </summary>
        /// <returns>Bit-OR of debug feature flags.</returns>
        public virtual int GetSettingFlags()
        {
            return mFlags;
        }
    }
}