/*
 *    Copyright (C) 2015 Haruki Hasegawa
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http:
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Adapter;
using Com.H6ah4i.Android.Widget.Advrecyclerview.Utils;
using Java.Lang.Ref;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using Math = System.Math;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Draggable
{
    public class RecyclerViewDragDropManager
    {
        private static readonly string TAG = "ARVDragDropManager";

        public static readonly IInterpolator DEFAULT_SWAP_TARGET_TRANSITION_INTERPOLATOR = new BasicSwapTargetTranslationInterpolator();

        public static readonly IInterpolator DEFAULT_ITEM_SETTLE_BACK_INTO_PLACE_ANIMATION_INTERPOLATOR = new DecelerateInterpolator();
        public static readonly int ITEM_MOVE_MODE_DEFAULT = 0;
        public static readonly int ITEM_MOVE_MODE_SWAP = 1;
        public interface OnItemDragEventListener
        {
            void OnItemDragStarted(int position);
            void OnItemDragPositionChanged(int fromPosition, int toPosition);
            void OnItemDragFinished(int fromPosition, int toPosition, bool result);
            void OnItemDragMoveDistanceUpdated(int offsetX, int offsetY);
        }
        private static readonly int SCROLL_DIR_NONE = 0;
        private static readonly int SCROLL_DIR_UP = (1 << 0);
        private static readonly int SCROLL_DIR_DOWN = (1 << 1);
        private static readonly int SCROLL_DIR_LEFT = (1 << 2);
        private static readonly int SCROLL_DIR_RIGHT = (1 << 3);
        private static readonly bool LOCAL_LOGV = false;
        private static readonly bool LOCAL_LOGD = false;
        private static readonly bool LOCAL_LOGI = true;
        private static readonly float SCROLL_THRESHOLD = 0.3F;
        private static readonly float SCROLL_AMOUNT_COEFF = 25;
        private static readonly float SCROLL_TOUCH_SLOP_MULTIPLY = 1.5F;
        class SwapTarget
        {
            public RecyclerView.ViewHolder holder;
            public int position;
            public bool self;
            public virtual void Clear()
            {
                holder = null;
                position = RecyclerView.NoPosition;
                self = false;
            }
        }
        class FindSwapTargetContext
        {
            public RecyclerView rv;
            public DraggingItemInfo draggingItemInfo;
            public RecyclerView.ViewHolder draggingItem;
            public int lastTouchX;
            public int lastTouchY;
            public int overlayItemLeft;
            public int overlayItemTop;
            public int overlayItemLeftNotClipped;
            public int overlayItemTopNotClipped;
            public int layoutType;
            public bool vertical;
            public ItemDraggableRange wrappedAdapterRange;
            public ItemDraggableRange rootAdapterRange;
            public bool checkCanSwap;
            public virtual void Setup(RecyclerView rv, RecyclerView.ViewHolder vh, DraggingItemInfo info, int lastTouchX, int lastTouchY, ItemDraggableRange wrappedAdapterPange, ItemDraggableRange rootAdapterRange, bool checkCanSwap)
            {
                this.rv = rv;
                this.draggingItemInfo = info;
                this.draggingItem = vh;
                this.lastTouchX = lastTouchX;
                this.lastTouchY = lastTouchY;
                this.wrappedAdapterRange = wrappedAdapterPange;
                this.rootAdapterRange = rootAdapterRange;
                this.checkCanSwap = checkCanSwap;
                this.layoutType = CustomRecyclerViewUtils.GetLayoutType(rv);
                this.vertical = CustomRecyclerViewUtils.ExtractOrientation(this.layoutType) == CustomRecyclerViewUtils.ORIENTATION_VERTICAL;
                this.overlayItemLeft = this.overlayItemLeftNotClipped = lastTouchX - info.grabbedPositionX;
                this.overlayItemTop = this.overlayItemTopNotClipped = lastTouchY - info.grabbedPositionY;
                if (this.vertical)
                {
                    this.overlayItemLeft = Math.Max(this.overlayItemLeft, rv.PaddingLeft);
                    this.overlayItemLeft = Math.Min(this.overlayItemLeft, Math.Max(0, rv.Width - rv.PaddingRight - draggingItemInfo.width));
                }
                else
                {
                    this.overlayItemTop = Math.Max(this.overlayItemTop, rv.PaddingTop);
                    this.overlayItemTop = Math.Min(this.overlayItemTop, Math.Max(0, rv.Height - rv.PaddingBottom - draggingItemInfo.height));
                }
            }

            public virtual void Clear()
            {
                this.rv = null;
                this.draggingItemInfo = null;
                this.draggingItem = null;
            }
        }
        private RecyclerView mRecyclerView;
        private IInterpolator mSwapTargetTranslationInterpolator = DEFAULT_SWAP_TARGET_TRANSITION_INTERPOLATOR;
        private ScrollOnDraggingProcessRunnable mScrollOnDraggingProcess;
        private RecyclerView.IOnItemTouchListener mInternalUseOnItemTouchListener;
        private RecyclerView.OnScrollListener mInternalUseOnScrollListener;
        private BaseEdgeEffectDecorator mEdgeEffectDecorator;
        private NinePatchDrawable mShadowDrawable;
        private float mDisplayDensity;
        private int mTouchSlop;
        private int mScrollTouchSlop;
        private int mInitialTouchX;
        private int mInitialTouchY;
        private long mInitialTouchItemId = RecyclerView.NoId;
        private bool mInitiateOnLongPress;
        private bool mInitiateOnTouch;
        private bool mInitiateOnMove = true;
        private int mLongPressTimeout;
        private bool mCheckCanDrop;
        private bool mInScrollByMethod;
        private int mActualScrollByXAmount;
        private int mActualScrollByYAmount;
        private readonly Rect mTmpRect1 = new Rect();
        private int mItemSettleBackIntoPlaceAnimationDuration = 200;
        private IInterpolator mItemSettleBackIntoPlaceAnimationInterpolator = DEFAULT_ITEM_SETTLE_BACK_INTO_PLACE_ANIMATION_INTERPOLATOR;
        private int mItemMoveMode = ITEM_MOVE_MODE_DEFAULT;
        private DraggingItemEffectsInfo mDraggingItemEffectsInfo = new DraggingItemEffectsInfo();
        private DraggableItemWrapperAdapter mWrapperAdapter;
        RecyclerView.ViewHolder mDraggingItemViewHolder;
        private DraggingItemInfo mDraggingItemInfo;
        private DraggingItemDecorator mDraggingItemDecorator;
        private SwapTargetItemOperator mSwapTargetItemOperator;
        private NestedScrollView mNestedScrollView;
        private int mNestedScrollViewScrollX;
        private int mNestedScrollViewScrollY;
        /*package*/
        private int mLastTouchX;
        /*package*/
        private int mLastTouchY;
        /*package*/
        private int mDragStartTouchX;
        /*package*/
        private int mDragStartTouchY;
        /*package*/
        private int mDragMinTouchX;
        /*package*/
        private int mDragMinTouchY;
        /*package*/
        private int mDragMaxTouchX;
        /*package*/
        private int mDragMaxTouchY;
        /*package*/
        private int mDragScrollDistanceX;
        /*package*/
        private int mDragScrollDistanceY;
        /*package*/
        private int mScrollDirMask = SCROLL_DIR_NONE;
        /*package*/
        private OverScrollMode mOrigOverScrollMode;
        /*package*/
        private ItemDraggableRange mDraggableRange;
        /*package*/
        private ItemDraggableRange mRootDraggableRange;
        /*package*/
        private InternalHandler mHandler;
        /*package*/
        private OnItemDragEventListener mItemDragEventListener;
        /*package*/
        private bool mCanDragH;
        /*package*/
        private bool mCanDragV;
        /*package*/
        private float mDragEdgeScrollSpeed = 1F;
        /*package*/
        private int mCurrentItemMoveMode = ITEM_MOVE_MODE_DEFAULT;
        /*package*/
        private object mComposedAdapterTag;
        /*package*/
        private SwapTarget mTempSwapTarget = new SwapTarget();
        /*package*/
        private FindSwapTargetContext mFindSwapTargetContext = new FindSwapTargetContext();
        /*package*/
        public RecyclerViewDragDropManager()
        {
            mInternalUseOnItemTouchListener = new AnonymousOnItemTouchListener(this);
            mInternalUseOnScrollListener = new AnonymousOnScrollListener(this);
            mScrollOnDraggingProcess = new ScrollOnDraggingProcessRunnable(this);
            mLongPressTimeout = ViewConfiguration.LongPressTimeout;
            mCheckItemSwappingRunnable = new AnonymousCheckItemSwappingRunnable(this);
        }

        private sealed class AnonymousOnItemTouchListener : Java.Lang.Object, RecyclerView.IOnItemTouchListener
        {
            public AnonymousOnItemTouchListener(RecyclerViewDragDropManager parent)
            {
                _parent = parent;
            }

            private readonly RecyclerViewDragDropManager _parent;
            public bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
            {
                return _parent.OnInterceptTouchEvent(rv, e);
            }

            public void OnTouchEvent(RecyclerView rv, MotionEvent e)
            {
                _parent.OnTouchEvent(rv, e);
            }

            public void OnRequestDisallowInterceptTouchEvent(bool disallowIntercept)
            {
                _parent.OnRequestDisallowInterceptTouchEvent(disallowIntercept);
            }
        }

        private sealed class AnonymousOnScrollListener : RecyclerView.OnScrollListener
        {
            public AnonymousOnScrollListener(RecyclerViewDragDropManager parent)
            {
                this._parent = parent;
            }

            private readonly RecyclerViewDragDropManager _parent;
            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {
                _parent.OnScrollStateChanged(recyclerView, newState);
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                _parent.OnScrolled(recyclerView, dx, dy);
            }
        }
        /*package*/
        public virtual RecyclerView.Adapter CreateWrappedAdapter(RecyclerView.Adapter adapter)
        {
            if (!adapter.HasStableIds)
            {
                throw new ArgumentException("The passed adapter does not support stable IDs");
            }

            if (mWrapperAdapter != null)
            {
                throw new InvalidOperationException("already have a wrapped adapter");
            }

            mWrapperAdapter = new DraggableItemWrapperAdapter(this, adapter);
            return mWrapperAdapter;
        }
        /*package*/
        public virtual bool IsReleased()
        {
            return (mInternalUseOnItemTouchListener == null);
        }
        /*package*/
        public virtual void AttachRecyclerView(RecyclerView rv)
        {
            if (IsReleased())
            {
                throw new InvalidOperationException("Accessing released object");
            }

            if (mRecyclerView != null)
            {
                throw new InvalidOperationException("RecyclerView instance has already been set");
            }

            mRecyclerView = rv;
            mRecyclerView.AddOnScrollListener(mInternalUseOnScrollListener);
            mRecyclerView.AddOnItemTouchListener(mInternalUseOnItemTouchListener);
            mDisplayDensity = mRecyclerView.Resources.DisplayMetrics.Density;
            mTouchSlop = ViewConfiguration.Get(mRecyclerView.Context).ScaledTouchSlop;
            mScrollTouchSlop = (int)(mTouchSlop * SCROLL_TOUCH_SLOP_MULTIPLY + 0.5F);
            mHandler = new InternalHandler(this);
            if (SupportsEdgeEffect())
            {
                switch (CustomRecyclerViewUtils.GetOrientation(mRecyclerView))
                {
                    case CustomRecyclerViewUtils.ORIENTATION_HORIZONTAL:
                        mEdgeEffectDecorator = new LeftRightEdgeEffectDecorator(mRecyclerView);
                        break;
                    case CustomRecyclerViewUtils.ORIENTATION_VERTICAL:
                        mEdgeEffectDecorator = new TopBottomEdgeEffectDecorator(mRecyclerView);
                        break;
                }

                if (mEdgeEffectDecorator != null)
                {
                    mEdgeEffectDecorator.Start();
                }
            }
        }
        /*package*/
        public virtual void Release()
        {
            CancelDrag(true);
            if (mHandler != null)
            {
                mHandler.Release();
                mHandler = null;
            }

            if (mEdgeEffectDecorator != null)
            {
                mEdgeEffectDecorator.Finish();
                mEdgeEffectDecorator = null;
            }

            if (mRecyclerView != null && mInternalUseOnItemTouchListener != null)
            {
                mRecyclerView.RemoveOnItemTouchListener(mInternalUseOnItemTouchListener);
            }

            mInternalUseOnItemTouchListener = null;
            if (mRecyclerView != null && mInternalUseOnScrollListener != null)
            {
                mRecyclerView.RemoveOnScrollListener(mInternalUseOnScrollListener);
            }

            mInternalUseOnScrollListener = null;
            if (mScrollOnDraggingProcess != null)
            {
                mScrollOnDraggingProcess.Release();
                mScrollOnDraggingProcess = null;
            }

            mWrapperAdapter = null;
            mRecyclerView = null;
            mSwapTargetTranslationInterpolator = null;
        }
        /*package*/
        public virtual bool IsDragging()
        {
            return (mDraggingItemInfo != null) && (!mHandler.IsCancelDragRequested());
        }
        /*package*/
        public virtual void SetDraggingItemShadowDrawable(NinePatchDrawable drawable)
        {
            mShadowDrawable = drawable;
        }
        /*package*/
        public virtual void SetSwapTargetTranslationInterpolator(IInterpolator interpolator)
        {
            mSwapTargetTranslationInterpolator = interpolator;
        }
        /*package*/
        public virtual bool IsInitiateOnLongPressEnabled()
        {
            return mInitiateOnLongPress;
        }
        /*package*/
        public virtual void SetInitiateOnLongPress(bool initiateOnLongPress)
        {
            mInitiateOnLongPress = initiateOnLongPress;
        }
        /*package*/
        public virtual bool IsInitiateOnMoveEnabled()
        {
            return mInitiateOnMove;
        }
        /*package*/
        public virtual void SetInitiateOnMove(bool initiateOnMove)
        {
            mInitiateOnMove = initiateOnMove;
        }
        /*package*/
        public virtual bool IsInitiateOnTouchEnabled()
        {
            return mInitiateOnTouch;
        }
        /*package*/
        public virtual void SetInitiateOnTouch(bool initiateOnTouch)
        {
            mInitiateOnTouch = initiateOnTouch;
        }
        /*package*/
        public virtual void SetLongPressTimeout(int longPressTimeout)
        {
            mLongPressTimeout = longPressTimeout;
        }
        /*package*/
        public virtual IInterpolator SetSwapTargetTranslationInterpolator()
        {
            return mSwapTargetTranslationInterpolator;
        }
        /*package*/
        public virtual OnItemDragEventListener GetOnItemDragEventListener()
        {
            return mItemDragEventListener;
        }
        /*package*/
        public virtual void SetOnItemDragEventListener(OnItemDragEventListener listener)
        {
            mItemDragEventListener = listener;
        }
        /*package*/
        public virtual void SetDragEdgeScrollSpeed(float speed)
        {
            mDragEdgeScrollSpeed = Math.Min(Math.Max(speed, 0F), 2F);
        }
        /*package*/
        public virtual float GetDragEdgeScrollSpeed()
        {
            return mDragEdgeScrollSpeed;
        }
        /*package*/
        public virtual void SetCheckCanDropEnabled(bool enabled)
        {
            mCheckCanDrop = enabled;
        }
        /*package*/
        public virtual bool IsCheckCanDropEnabled()
        {
            return mCheckCanDrop;
        }
        /*package*/
        /*package*/
        public virtual bool OnInterceptTouchEvent(RecyclerView rv, MotionEvent e)
        {
            var action = e.ActionMasked;
            bool handled = false;
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onInterceptTouchEvent() action = " + action);
            }

            switch (action)
            {
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    handled = HandleActionUpOrCancel(action, true);
                    break;
                case MotionEventActions.Down:
                    if (!IsDragging())
                    {
                        handled = HandleActionDown(rv, e);
                    }

                    break;
                case MotionEventActions.Move:
                    if (IsDragging())
                    {
                        HandleActionMoveWhileDragging(rv, e);
                        handled = true;
                    }
                    else
                    {
                        if (HandleActionMoveWhileNotDragging(rv, e))
                        {
                            handled = true;
                        }
                    }
                    break;
            }

            return handled;
        }
        /*package*/
        /*package*/
        /*package*/
        public virtual void OnTouchEvent(RecyclerView rv, MotionEvent e)
        {
            var action = e.ActionMasked;
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onTouchEvent() action = " + action);
            }

            if (!IsDragging())
            {
                return;
            }

            switch (action)
            {
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    HandleActionUpOrCancel(action, true);
                    break;
                case MotionEventActions.Move:
                    HandleActionMoveWhileDragging(rv, e);
                    break;
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        public virtual void OnRequestDisallowInterceptTouchEvent(bool disallowIntercept)
        {
            if (disallowIntercept)
            {
                CancelDrag(true);
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        public virtual void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onScrolled(dx = " + dx + ", dy = " + dy + ")");
            }

            if (mInScrollByMethod)
            {
                mActualScrollByXAmount = dx;
                mActualScrollByYAmount = dy;
            }
            else if (IsDragging())
            {
                ViewCompat.PostOnAnimationDelayed(mRecyclerView, mCheckItemSwappingRunnable, 500);
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        public virtual void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            if (LOCAL_LOGV)
            {
                Log.Verbose(TAG, "onScrollStateChanged(newState = " + newState + ")");
            }

            if (newState == RecyclerView.ScrollStateDragging)
            {
                CancelDrag(true);
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        private bool HandleActionDown(RecyclerView rv, MotionEvent e)
        {
            RecyclerView.ViewHolder holder = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(rv, e.GetX(), e.GetY());
            if (!CheckTouchedItemState(rv, holder))
            {
                return false;
            }

            int touchX = (int)(e.GetX() + 0.5F);
            int touchY = (int)(e.GetY() + 0.5F);
            if (!CanStartDrag(holder, touchX, touchY))
            {
                return false;
            }

            int orientation = CustomRecyclerViewUtils.GetOrientation(mRecyclerView);
            int spanCount = CustomRecyclerViewUtils.GetSpanCount(mRecyclerView);
            mInitialTouchX = mLastTouchX = touchX;
            mInitialTouchY = mLastTouchY = touchY;
            mInitialTouchItemId = holder.ItemId;
            mCanDragH = (orientation == CustomRecyclerViewUtils.ORIENTATION_HORIZONTAL) || ((orientation == CustomRecyclerViewUtils.ORIENTATION_VERTICAL) && (spanCount > 1));
            mCanDragV = (orientation == CustomRecyclerViewUtils.ORIENTATION_VERTICAL) || ((orientation == CustomRecyclerViewUtils.ORIENTATION_HORIZONTAL) && (spanCount > 1));
            bool handled;
            if (mInitiateOnTouch)
            {
                handled = CheckConditionAndStartDragging(rv, e, false);
            }
            else if (mInitiateOnLongPress)
            {
                mHandler.StartLongPressDetection(e, mLongPressTimeout);
                handled = false;
            }
            else
            {
                handled = false;
            }

            return handled;
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        public virtual void HandleOnLongPress(MotionEvent e)
        {
            if (mInitiateOnLongPress)
            {
                CheckConditionAndStartDragging(mRecyclerView, e, false);
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        public virtual void HandleOnCheckItemViewSizeUpdate()
        {
            RecyclerView.ViewHolder vh = mRecyclerView.FindViewHolderForItemId(mDraggingItemInfo.id);
            if (vh == null)
            {
                return;
            }

            int w = vh.ItemView.Width;
            int h = vh.ItemView.Height;
            if (!(w == mDraggingItemInfo.width && h == mDraggingItemInfo.height))
            {
                mDraggingItemInfo = DraggingItemInfo.CreateWithNewView(mDraggingItemInfo, vh);
                mDraggingItemDecorator.UpdateDraggingItemView(mDraggingItemInfo, vh);
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private void StartDragging(RecyclerView rv, MotionEvent e, RecyclerView.ViewHolder holder, ItemDraggableRange range, AdapterPath path, int wrappedItemPosition, object composedAdapterTag)
        {
            SafeEndAnimation(rv, holder);
            mHandler.CancelLongPressDetection();
            mDraggingItemInfo = new DraggingItemInfo(rv, holder, mLastTouchX, mLastTouchY);
            mDraggingItemViewHolder = holder;
            mDraggableRange = range;
            mRootDraggableRange = ConvertToRootAdapterRange(path, mDraggableRange);
            NestedScrollView nestedScrollView = FindAncestorNestedScrollView(mRecyclerView);
            if (nestedScrollView != null && !mRecyclerView.NestedScrollingEnabled)
            {
                mNestedScrollView = nestedScrollView;
            }
            else
            {
                mNestedScrollView = null;
            }

            mOrigOverScrollMode = rv.OverScrollMode;
            rv.OverScrollMode = OverScrollMode.Never;
            mLastTouchX = (int)(e.GetX() + 0.5F);
            mLastTouchY = (int)(e.GetY() + 0.5F);
            mNestedScrollViewScrollX = (mNestedScrollView != null) ? mNestedScrollView.ScrollX : 0;
            mNestedScrollViewScrollY = (mNestedScrollView != null) ? mNestedScrollView.ScrollY : 0;
            mDragStartTouchY = mDragMinTouchY = mDragMaxTouchY = mLastTouchY;
            mDragStartTouchX = mDragMinTouchX = mDragMaxTouchX = mLastTouchX;
            mScrollDirMask = SCROLL_DIR_NONE;
            mCurrentItemMoveMode = mItemMoveMode;
            mComposedAdapterTag = composedAdapterTag;
            mRecyclerView.Parent.RequestDisallowInterceptTouchEvent(true);
            StartScrollOnDraggingProcess();
            mWrapperAdapter.StartDraggingItem(mDraggingItemInfo, holder, mDraggableRange, wrappedItemPosition, mCurrentItemMoveMode);
            mWrapperAdapter.OnBindViewHolder(holder, wrappedItemPosition);
            mDraggingItemDecorator = new DraggingItemDecorator(mRecyclerView, holder, mRootDraggableRange);
            mDraggingItemDecorator.SetShadowDrawable(mShadowDrawable);
            mDraggingItemDecorator.SetupDraggingItemEffects(mDraggingItemEffectsInfo);
            mDraggingItemDecorator.Start(mDraggingItemInfo, mLastTouchX, mLastTouchY);
            int layoutType = CustomRecyclerViewUtils.GetLayoutType(mRecyclerView);
            if (!mCheckCanDrop && CustomRecyclerViewUtils.IsLinearLayout(layoutType))
            {
                mSwapTargetItemOperator = new SwapTargetItemOperator(mRecyclerView, holder, mDraggingItemInfo);
                mSwapTargetItemOperator.SetSwapTargetTranslationInterpolator(mSwapTargetTranslationInterpolator);
                mSwapTargetItemOperator.Start();
                mSwapTargetItemOperator.Update(mDraggingItemDecorator.GetDraggingItemTranslationX(), mDraggingItemDecorator.GetDraggingItemTranslationY());
            }

            if (mEdgeEffectDecorator != null)
            {
                mEdgeEffectDecorator.ReorderToTop();
            }
            mWrapperAdapter.OnDragItemStarted();
            if (mItemDragEventListener != null)
            {
                mItemDragEventListener.OnItemDragStarted(mWrapperAdapter.GetDraggingItemInitialPosition());
                mItemDragEventListener.OnItemDragMoveDistanceUpdated(0, 0);
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        public virtual int GetItemMoveMode()
        {
            return mItemMoveMode;
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        public virtual void SetItemMoveMode(int mode)
        {
            mItemMoveMode = mode;
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        public virtual void CancelDrag()
        {
            CancelDrag(false);
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        public virtual void CancelDrag(bool immediately)
        {
            HandleActionUpOrCancel(MotionEventActions.Cancel, false);
            if (immediately)
            {
                FinishDragging(false);
            }
            else
            {
                if (IsDragging())
                {
                    mHandler.RequestDeferredCancelDrag();
                }
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private void FinishDragging(bool result)
        {
            if (!IsDragging())
            {
                return;
            }
            if (mHandler != null)
            {
                mHandler.RemoveDeferredCancelDragRequest();
                mHandler.RemoveDraggingItemViewSizeUpdateCheckRequest();
            }
            if (mRecyclerView != null && mDraggingItemViewHolder != null)
            {
                mRecyclerView.OverScrollMode = (mOrigOverScrollMode);
            }

            if (mDraggingItemDecorator != null)
            {
                mDraggingItemDecorator.SetReturnToDefaultPositionAnimationDuration(mItemSettleBackIntoPlaceAnimationDuration);
                mDraggingItemDecorator.SetReturnToDefaultPositionAnimationInterpolator(mItemSettleBackIntoPlaceAnimationInterpolator);
                mDraggingItemDecorator.Finish(true);
            }

            if (mSwapTargetItemOperator != null)
            {
                mSwapTargetItemOperator.SetReturnToDefaultPositionAnimationDuration(mItemSettleBackIntoPlaceAnimationDuration);
                mDraggingItemDecorator.SetReturnToDefaultPositionAnimationInterpolator(mItemSettleBackIntoPlaceAnimationInterpolator);
                mSwapTargetItemOperator.Finish(true);
            }

            if (mEdgeEffectDecorator != null)
            {
                mEdgeEffectDecorator.ReleaseBothGlows();
            }

            StopScrollOnDraggingProcess();
            if (mRecyclerView != null && mRecyclerView.Parent != null)
            {
                mRecyclerView.Parent.RequestDisallowInterceptTouchEvent(false);
            }

            if (mRecyclerView != null)
            {
                mRecyclerView.Invalidate();
            }

            mDraggableRange = null;
            mRootDraggableRange = null;
            mDraggingItemDecorator = null;
            mSwapTargetItemOperator = null;
            mDraggingItemViewHolder = null;
            mDraggingItemInfo = null;
            mComposedAdapterTag = null;
            mNestedScrollView = null;
            mLastTouchX = 0;
            mLastTouchY = 0;
            mNestedScrollViewScrollX = 0;
            mNestedScrollViewScrollY = 0;
            mDragStartTouchX = 0;
            mDragStartTouchY = 0;
            mDragMinTouchX = 0;
            mDragMinTouchY = 0;
            mDragMaxTouchX = 0;
            mDragMaxTouchY = 0;
            mDragScrollDistanceX = 0;
            mDragScrollDistanceY = 0;
            mCanDragH = false;
            mCanDragV = false;
            int draggingItemInitialPosition = RecyclerView.NoPosition;
            int draggingItemCurrentPosition = RecyclerView.NoPosition;
            if (mWrapperAdapter != null)
            {
                draggingItemInitialPosition = mWrapperAdapter.GetDraggingItemInitialPosition();
                draggingItemCurrentPosition = mWrapperAdapter.GetDraggingItemCurrentPosition();
                mWrapperAdapter.OnDragItemFinished(draggingItemInitialPosition, draggingItemCurrentPosition, result);
            }
            if (mItemDragEventListener != null)
            {
                mItemDragEventListener.OnItemDragFinished(draggingItemInitialPosition, draggingItemCurrentPosition, result);
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private bool HandleActionUpOrCancel(MotionEventActions action, bool invokeFinish)
        {
            bool result = (action == MotionEventActions.Up);
            bool handled = IsDragging();
            if (mHandler != null)
            {
                mHandler.CancelLongPressDetection();
            }

            mInitialTouchX = 0;
            mInitialTouchY = 0;
            mLastTouchX = 0;
            mLastTouchY = 0;
            mDragStartTouchX = 0;
            mDragStartTouchY = 0;
            mDragMinTouchX = 0;
            mDragMinTouchY = 0;
            mDragMaxTouchX = 0;
            mDragMaxTouchY = 0;
            mDragScrollDistanceX = 0;
            mDragScrollDistanceY = 0;
            mInitialTouchItemId = RecyclerView.NoId;
            mCanDragH = false;
            mCanDragV = false;
            if (invokeFinish && IsDragging())
            {
                if (LOCAL_LOGD)
                {
                    Log.Debug(TAG, "dragging finished  --- result = " + result);
                }

                FinishDragging(result);
            }

            return handled;
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private bool HandleActionMoveWhileNotDragging(RecyclerView rv, MotionEvent e)
        {
            if (mInitiateOnMove)
            {
                return CheckConditionAndStartDragging(rv, e, true);
            }
            else
            {
                return false;
            }
        }
        private bool CheckConditionAndStartDragging(RecyclerView rv, MotionEvent e, bool checkTouchSlop)
        {
            if (mDraggingItemInfo != null)
            {
                return false;
            }

            int touchX = (int)(e.GetX() + 0.5F);
            int touchY = (int)(e.GetY() + 0.5F);
            mLastTouchX = touchX;
            mLastTouchY = touchY;
            if (mInitialTouchItemId == RecyclerView.NoId)
            {
                return false;
            }

            if (checkTouchSlop)
            {
                if (!((mCanDragH && (Math.Abs(touchX - mInitialTouchX) > mTouchSlop)) || (mCanDragV && (Math.Abs(touchY - mInitialTouchY) > mTouchSlop))))
                {
                    return false;
                }
            }

            RecyclerView.ViewHolder holder = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(rv, mInitialTouchX, mInitialTouchY);
            if (holder == null)
            {
                return false;
            }

            if (!CanStartDrag(holder, touchX, touchY))
            {
                return false;
            }

            RecyclerView.Adapter rootAdapter = mRecyclerView.GetAdapter();
            AdapterPath path = new AdapterPath();
            int wrappedItemPosition = WrapperAdapterUtils.UnwrapPosition(rootAdapter, mWrapperAdapter, null, holder.AdapterPosition, path);
            ItemDraggableRange range = mWrapperAdapter.GetItemDraggableRange(holder, wrappedItemPosition);
            if (range == null)
            {
                range = new ItemDraggableRange(0, Math.Max(0, mWrapperAdapter.ItemCount - 1));
            }

            VerifyItemDraggableRange(range, wrappedItemPosition);
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "dragging started");
            }

            StartDragging(rv, e, holder, range, path, wrappedItemPosition, path.LastSegment().tag);
            return true;
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private bool CanStartDrag(RecyclerView.ViewHolder holder, int touchX, int touchY)
        {
            int origRootPosition = holder.AdapterPosition;
            int wrappedItemPosition = WrapperAdapterUtils.UnwrapPosition(mRecyclerView.GetAdapter(), mWrapperAdapter, null, origRootPosition);
            if (wrappedItemPosition == RecyclerView.NoPosition)
            {
                return false;
            }

            View view = holder.ItemView;
            int translateX = (int)(view.TranslationX + 0.5F);
            int translateY = (int)(view.TranslationY + 0.5F);
            int viewX = touchX - (view.Left + translateX);
            int viewY = touchY - (view.Top + translateY);
            if (mWrapperAdapter.CanStartDrag(holder, wrappedItemPosition, viewX, viewY))
            {
                return (holder.AdapterPosition == origRootPosition);
            }
            else
            {
                return false;
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private void VerifyItemDraggableRange(ItemDraggableRange range, int position)
        {
            int start = 0;
            int end = Math.Max(0, mWrapperAdapter.ItemCount - 1);
            if (range.GetStart() > range.GetEnd())
            {
                throw new InvalidOperationException("Invalid wrappedAdapterRange specified --- start > wrappedAdapterRange (wrappedAdapterRange = " + range + ")");
            }

            if (range.GetStart() < start)
            {
                throw new InvalidOperationException("Invalid wrappedAdapterRange specified --- start < 0 (wrappedAdapterRange = " + range + ")");
            }

            if (range.GetEnd() > end)
            {
                throw new InvalidOperationException("Invalid wrappedAdapterRange specified --- end >= count (wrappedAdapterRange = " + range + ")");
            }

            if (!range.CheckInRange(position))
            {
                throw new InvalidOperationException("Invalid wrappedAdapterRange specified --- does not contain drag target item" + " (wrappedAdapterRange = " + range + ", position = " + position + ")");
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private void HandleActionMoveWhileDragging(RecyclerView rv, MotionEvent e)
        {
            mLastTouchX = (int)(e.GetX() + 0.5F);
            mLastTouchY = (int)(e.GetY() + 0.5F);
            mNestedScrollViewScrollX = (mNestedScrollView != null) ? mNestedScrollView.ScrollX : 0;
            mNestedScrollViewScrollY = (mNestedScrollView != null) ? mNestedScrollView.ScrollY : 0;
            mDragMinTouchX = Math.Min(mDragMinTouchX, mLastTouchX);
            mDragMinTouchY = Math.Min(mDragMinTouchY, mLastTouchY);
            mDragMaxTouchX = Math.Max(mDragMaxTouchX, mLastTouchX);
            mDragMaxTouchY = Math.Max(mDragMaxTouchY, mLastTouchY);
            UpdateDragDirectionMask();
            bool updated = mDraggingItemDecorator.Update(GetLastTouchX(), GetLastTouchY(), false);
            if (updated)
            {
                if (mSwapTargetItemOperator != null)
                {
                    mSwapTargetItemOperator.Update(mDraggingItemDecorator.GetDraggingItemTranslationX(), mDraggingItemDecorator.GetDraggingItemTranslationY());
                }
                CheckItemSwapping(rv);
                OnItemMoveDistanceUpdated();
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private void UpdateDragDirectionMask()
        {
            switch (CustomRecyclerViewUtils.GetOrientation(mRecyclerView))
            {
                case CustomRecyclerViewUtils.ORIENTATION_VERTICAL:
                {
                    int lastTouchY = GetLastTouchY();
                    if (((mDragStartTouchY - mDragMinTouchY) > mScrollTouchSlop) || ((mDragMaxTouchY - lastTouchY) > mScrollTouchSlop))
                    {
                        mScrollDirMask |= SCROLL_DIR_UP;
                    }

                    if (((mDragMaxTouchY - mDragStartTouchY) > mScrollTouchSlop) || ((lastTouchY - mDragMinTouchY) > mScrollTouchSlop))
                    {
                        mScrollDirMask |= SCROLL_DIR_DOWN;
                    }

                    break;
                }

                case CustomRecyclerViewUtils.ORIENTATION_HORIZONTAL:
                {
                    int lastTouchX = GetLastTouchX();
                    if (((mDragStartTouchX - mDragMinTouchX) > mScrollTouchSlop) || ((mDragMaxTouchX - lastTouchX) > mScrollTouchSlop))
                    {
                        mScrollDirMask |= SCROLL_DIR_LEFT;
                    }

                    if (((mDragMaxTouchX - mDragStartTouchX) > mScrollTouchSlop) || ((lastTouchX - mDragMinTouchX) > mScrollTouchSlop))
                    {
                        mScrollDirMask |= SCROLL_DIR_RIGHT;
                    }

                    break;
                }
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private int GetLastTouchX()
        {
            int touchX = mLastTouchX;
            if (mNestedScrollView != null)
            {
                touchX += (mNestedScrollView.ScrollX - mNestedScrollViewScrollX);
            }

            return touchX;
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private int GetLastTouchY()
        {
            int touchY = mLastTouchY;
            if (mNestedScrollView != null)
            {
                touchY += (mNestedScrollView.ScrollY - mNestedScrollViewScrollY);
            }

            return touchY;
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        public virtual void CheckItemSwapping(RecyclerView rv)
        {
            RecyclerView.ViewHolder draggingItem = mDraggingItemViewHolder;
            FindSwapTargetContext fc = mFindSwapTargetContext;
            fc.Setup(rv, mDraggingItemViewHolder, mDraggingItemInfo, GetLastTouchX(), GetLastTouchY(), mDraggableRange, mRootDraggableRange, mCheckCanDrop);
            int draggingItemInitialPosition = mWrapperAdapter.GetDraggingItemInitialPosition();
            int draggingItemCurrentPosition = mWrapperAdapter.GetDraggingItemCurrentPosition();
            SwapTarget swapTarget;
            bool canSwap = false;
            swapTarget = FindSwapTargetItem(mTempSwapTarget, fc, false);
            if (swapTarget.position != RecyclerView.NoPosition)
            {
                if (!mCheckCanDrop)
                {
                    canSwap = true;
                }

                if (!canSwap)
                {
                    canSwap = mWrapperAdapter.CanDropItems(draggingItemInitialPosition, swapTarget.position);
                }

                if (!canSwap)
                {
                    swapTarget = FindSwapTargetItem(mTempSwapTarget, fc, true);
                    if (swapTarget.position != RecyclerView.NoPosition)
                    {
                        canSwap = mWrapperAdapter.CanDropItems(draggingItemInitialPosition, swapTarget.position);
                    }
                }
            }

            if (canSwap && swapTarget.holder == null)
            {
                throw new InvalidOperationException("bug check");
            }

            if (canSwap)
            {
                SwapItems(rv, draggingItemCurrentPosition, draggingItem, swapTarget.holder);
            }

            if (mSwapTargetItemOperator != null)
            {
                mSwapTargetItemOperator.SetSwapTargetItem((canSwap) ? swapTarget.holder : null);
            }

            if (canSwap)
            {
                mHandler.ScheduleDraggingItemViewSizeUpdateCheck();
            }

            swapTarget.Clear();
            fc.Clear();
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private void OnItemMoveDistanceUpdated()
        {
            if (mItemDragEventListener == null)
            {
                return;
            }

            int moveX = mDragScrollDistanceX + mDraggingItemDecorator.GetDraggingItemMoveOffsetX();
            int moveY = mDragScrollDistanceY + mDraggingItemDecorator.GetDraggingItemMoveOffsetY();
            mItemDragEventListener.OnItemDragMoveDistanceUpdated(moveX, moveY);
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        public virtual void HandleScrollOnDragging()
        {
            RecyclerView rv = mRecyclerView;
            bool horizontal;
            switch (CustomRecyclerViewUtils.GetOrientation(rv))
            {
                case CustomRecyclerViewUtils.ORIENTATION_VERTICAL:
                    horizontal = false;
                    break;
                case CustomRecyclerViewUtils.ORIENTATION_HORIZONTAL:
                    horizontal = true;
                    break;
                default:
                    return;
                    break;
            }

            if (mNestedScrollView != null)
            {
                HandleScrollOnDraggingInternalWithNestedScrollView(rv, horizontal);
            }
            else
            {
                HandleScrollOnDraggingInternalWithRecyclerView(rv, horizontal);
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private void HandleScrollOnDraggingInternalWithNestedScrollView(RecyclerView rv, bool horizontal)
        {
            NestedScrollView nestedScrollView = mNestedScrollView;
            int nestedScrollViewScrollOffsetX = nestedScrollView.ScrollX;
            int nestedScrollViewScrollOffsetY = nestedScrollView.ScrollY;
            Rect rect = new Rect();
            rect.Left = rect.Right = GetLastTouchX();
            rect.Top = rect.Bottom = GetLastTouchY();
            OffsetDescendantRectToAncestorCoords(mRecyclerView, nestedScrollView, rect);
            int nestedScrollViewTouchX = rect.Left - nestedScrollViewScrollOffsetX;
            int nestedScrollViewTouchY = rect.Top - nestedScrollViewScrollOffsetY;
            int edge = (horizontal) ? nestedScrollView.Width : nestedScrollView.Height;
            float invEdge = (1F / edge);
            float normalizedTouchPos = (horizontal ? nestedScrollViewTouchX : nestedScrollViewTouchY) * invEdge;
            float threshold = SCROLL_THRESHOLD;
            float invThreshold = (1F / threshold);
            float centerOffset = normalizedTouchPos - 0.5F;
            float absCenterOffset = Math.Abs(centerOffset);
            float acceleration = Math.Max(0F, threshold - (0.5F - absCenterOffset)) * invThreshold;
            int mask = mScrollDirMask;
            int scrollAmount = (int)Math.Sign(centerOffset) * (int)(SCROLL_AMOUNT_COEFF * mDragEdgeScrollSpeed * mDisplayDensity * acceleration + 0.5F);
            if (scrollAmount > 0)
            {
                if ((mask & (horizontal ? SCROLL_DIR_RIGHT : SCROLL_DIR_DOWN)) == 0)
                {
                    scrollAmount = 0;
                }
            }
            else if (scrollAmount < 0)
            {
                if ((mask & (horizontal ? SCROLL_DIR_LEFT : SCROLL_DIR_UP)) == 0)
                {
                    scrollAmount = 0;
                }
            }
            if (scrollAmount != 0)
            {
                SafeEndAnimationsIfRequired(rv);
                if (horizontal)
                {
                    nestedScrollView.ScrollBy(scrollAmount, 0);
                }
                else
                {
                    nestedScrollView.ScrollBy(0, scrollAmount);
                }
            }

            bool updated = mDraggingItemDecorator.Update(GetLastTouchX(), GetLastTouchY(), false);
            if (updated)
            {
                if (mSwapTargetItemOperator != null)
                {
                    mSwapTargetItemOperator.Update(mDraggingItemDecorator.GetDraggingItemTranslationX(), mDraggingItemDecorator.GetDraggingItemTranslationY());
                }
                CheckItemSwapping(rv);
                OnItemMoveDistanceUpdated();
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private void HandleScrollOnDraggingInternalWithRecyclerView(RecyclerView rv, bool horizontal)
        {
            int edge = (horizontal) ? rv.Width : rv.Height;
            if (edge == 0)
            {
                return;
            }

            float invEdge = (1F / edge);
            float normalizedTouchPos = (horizontal ? GetLastTouchX() : GetLastTouchY()) * invEdge;
            float threshold = SCROLL_THRESHOLD;
            float invThreshold = (1F / threshold);
            float centerOffset = normalizedTouchPos - 0.5F;
            float absCenterOffset = Math.Abs(centerOffset);
            float acceleration = Math.Max(0F, threshold - (0.5F - absCenterOffset)) * invThreshold;
            int mask = mScrollDirMask;
            DraggingItemDecorator decorator = mDraggingItemDecorator;
            int scrollAmount = (int)Math.Sign(centerOffset) * (int)(SCROLL_AMOUNT_COEFF * mDragEdgeScrollSpeed * mDisplayDensity * acceleration + 0.5F);
            int actualScrolledAmount = 0;
            ItemDraggableRange range = mRootDraggableRange;
            int firstVisibleChild = CustomRecyclerViewUtils.FindFirstCompletelyVisibleItemPosition(mRecyclerView);
            int lastVisibleChild = CustomRecyclerViewUtils.FindLastCompletelyVisibleItemPosition(mRecyclerView);
            bool reachedToFirstHardLimit = false;
            bool reachedToFirstSoftLimit = false;
            bool reachedToLastHardLimit = false;
            bool reachedToLastSoftLimit = false;
            if (firstVisibleChild != RecyclerView.NoPosition)
            {
                if (firstVisibleChild <= range.GetStart())
                {
                    reachedToFirstSoftLimit = true;
                }

                if (firstVisibleChild <= (range.GetStart() - 1))
                {
                    reachedToFirstHardLimit = true;
                }
            }

            if (lastVisibleChild != RecyclerView.NoPosition)
            {
                if (lastVisibleChild >= range.GetEnd())
                {
                    reachedToLastSoftLimit = true;
                }

                if (lastVisibleChild >= (range.GetEnd() + 1))
                {
                    reachedToLastHardLimit = true;
                }
            }
            if (scrollAmount > 0)
            {
                if ((mask & (horizontal ? SCROLL_DIR_RIGHT : SCROLL_DIR_DOWN)) == 0)
                {
                    scrollAmount = 0;
                }
            }
            else if (scrollAmount < 0)
            {
                if ((mask & (horizontal ? SCROLL_DIR_LEFT : SCROLL_DIR_UP)) == 0)
                {
                    scrollAmount = 0;
                }
            }
            if ((!reachedToFirstHardLimit && (scrollAmount < 0)) || (!reachedToLastHardLimit && (scrollAmount > 0)))
            {
                SafeEndAnimationsIfRequired(rv);
                actualScrolledAmount = (horizontal) ? ScrollByXAndGetScrolledAmount(scrollAmount) : ScrollByYAndGetScrolledAmount(scrollAmount);
                if (scrollAmount < 0)
                {
                    decorator.SetIsScrolling(!reachedToFirstSoftLimit);
                }
                else
                {
                    decorator.SetIsScrolling(!reachedToLastSoftLimit);
                }

                decorator.Refresh(true);
                if (mSwapTargetItemOperator != null)
                {
                    mSwapTargetItemOperator.Update(decorator.GetDraggingItemTranslationX(), decorator.GetDraggingItemTranslationY());
                }
            }
            else
            {
                decorator.SetIsScrolling(false);
            }

            if (mEdgeEffectDecorator != null)
            {
                float edgeEffectPullDistance = 0;
                if (mOrigOverScrollMode != OverScrollMode.Never)
                {
                    bool actualIsScrolling = (actualScrolledAmount != 0);
                    float edgeEffectStrength = 0.005F;
                    int draggingItemTopLeft = (horizontal) ? decorator.GetTranslatedItemPositionLeft() : decorator.GetTranslatedItemPositionTop();
                    int draggingItemBottomRight = (horizontal) ? decorator.GetTranslatedItemPositionRight() : decorator.GetTranslatedItemPositionBottom();
                    int draggingItemCenter = (draggingItemTopLeft + draggingItemBottomRight) / 2;
                    int nearEdgePosition;
                    if (firstVisibleChild == 0 && lastVisibleChild == 0)
                    {
                        nearEdgePosition = (scrollAmount < 0) ? draggingItemTopLeft : draggingItemBottomRight;
                    }
                    else
                    {
                        nearEdgePosition = (draggingItemCenter < (edge / 2)) ? draggingItemTopLeft : draggingItemBottomRight;
                    }

                    float nearEdgeOffset = (nearEdgePosition * invEdge) - 0.5F;
                    float absNearEdgeOffset = Math.Abs(nearEdgeOffset);
                    if ((absNearEdgeOffset > 0.4F) && (scrollAmount != 0) && !actualIsScrolling)
                    {
                        if (nearEdgeOffset < 0)
                        {
                            if (horizontal ? decorator.IsReachedToLeftLimit() : decorator.IsReachedToTopLimit())
                            {
                                edgeEffectPullDistance = -mDisplayDensity * edgeEffectStrength;
                            }
                        }
                        else
                        {
                            if (horizontal ? decorator.IsReachedToRightLimit() : decorator.IsReachedToBottomLimit())
                            {
                                edgeEffectPullDistance = mDisplayDensity * edgeEffectStrength;
                            }
                        }
                    }
                }

                UpdateEdgeEffect(edgeEffectPullDistance);
            }

            ViewCompat.PostOnAnimation(mRecyclerView, mCheckItemSwappingRunnable);
            if (actualScrolledAmount != 0)
            {
                if (horizontal)
                {
                    mDragScrollDistanceX += actualScrolledAmount;
                }
                else
                {
                    mDragScrollDistanceY += actualScrolledAmount;
                }

                OnItemMoveDistanceUpdated();
            }
        }
        /*package*/
        /*package*/
        /*package*/
        /*package */
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        /*package*/
        private void UpdateEdgeEffect(float distance)
        {
            if (distance != 0F)
            {
                if (distance < 0)
                {
                    mEdgeEffectDecorator.PullFirstEdge(distance);
                }
                else
                {
                    mEdgeEffectDecorator.PullSecondEdge(distance);
                }
            }
            else
            {
                mEdgeEffectDecorator.ReleaseBothGlows();
            }
        }
        
        private readonly IRunnable mCheckItemSwappingRunnable;
        private sealed class AnonymousCheckItemSwappingRunnable : Java.Lang.Object, IRunnable
        {
            public AnonymousCheckItemSwappingRunnable(RecyclerViewDragDropManager parent)
            {
                this._parent = parent;
            }

            private readonly RecyclerViewDragDropManager _parent;
            public void Run()
            {
                if (_parent.mDraggingItemViewHolder != null)
                {
                    _parent.CheckItemSwapping(_parent.GetRecyclerView());
                }
            }
        }
        private static NestedScrollView FindAncestorNestedScrollView(View v)
        {
            var target = v.Parent;
            while (target != null)
            {
                if (target is NestedScrollView)
                {
                    return (NestedScrollView)target;
                }

                target = target.Parent;
            }

            return null;
        }
        private static bool OffsetDescendantRectToAncestorCoords(View descendant, View ancestor, Rect rect)
        {
            View view = descendant;
            IViewParent parent;
            do
            {
                parent = view.Parent;
                if (!(parent is ViewGroup))
                {
                    return false;
                }

                ((ViewGroup)parent).OffsetDescendantRectToMyCoords(view, rect);
                view = (View)parent;
            }
            while (parent != ancestor);
            return true;
        }
        
        private int ScrollByYAndGetScrolledAmount(int ry)
        {
            mActualScrollByYAmount = 0;
            mInScrollByMethod = true;
            mRecyclerView.ScrollBy(0, ry);
            mInScrollByMethod = false;
            return mActualScrollByYAmount;
        }
        private int ScrollByXAndGetScrolledAmount(int rx)
        {
            mActualScrollByXAmount = 0;
            mInScrollByMethod = true;
            mRecyclerView.ScrollBy(rx, 0);
            mInScrollByMethod = false;
            return mActualScrollByXAmount;
        }
        public virtual RecyclerView GetRecyclerView()
        {
            return mRecyclerView;
        }
        private void StartScrollOnDraggingProcess()
        {
            mScrollOnDraggingProcess.Start();
        }
        private void StopScrollOnDraggingProcess()
        {
            if (mScrollOnDraggingProcess != null)
            {
                mScrollOnDraggingProcess.Stop();
            }
        }
        private void SwapItems(RecyclerView rv, int draggingItemAdapterPosition, RecyclerView.ViewHolder draggingItem, RecyclerView.ViewHolder swapTargetHolder)
        {
            Rect swapTargetMargins = CustomRecyclerViewUtils.GetLayoutMargins(swapTargetHolder.ItemView, mTmpRect1);
            int fromPosition = draggingItemAdapterPosition;
            int toPosition = GetWrappedAdapterPosition(swapTargetHolder);
            int diffPosition = Math.Abs(fromPosition - toPosition);
            bool performSwapping = false;
            if (fromPosition == RecyclerView.NoPosition || toPosition == RecyclerView.NoPosition)
            {
                return;
            }

            long wrappedAdapterItemId = ItemIdComposer.ExtractWrappedIdPart(mWrapperAdapter.GetItemId(fromPosition));
            long wrappedItemId = ItemIdComposer.ExtractWrappedIdPart(mDraggingItemInfo.id);
            if (wrappedAdapterItemId != wrappedItemId)
            {
                if (LOCAL_LOGV)
                {
                    Log.Verbose(TAG, "RecyclerView state has not been synchronized to data yet");
                }

                return;
            }

            bool isLinearLayout = CustomRecyclerViewUtils.IsLinearLayout(CustomRecyclerViewUtils.GetLayoutType(rv));
            bool swapNextItemSmoothlyInLinearLayout = isLinearLayout && (!mCheckCanDrop);
            if (diffPosition == 0)
            {
            }
            else if ((diffPosition == 1) && (draggingItem != null) && swapNextItemSmoothlyInLinearLayout)
            {
                View v1 = draggingItem.ItemView;
                View v2 = swapTargetHolder.ItemView;
                Rect m1 = mDraggingItemInfo.margins;
                Rect m2 = swapTargetMargins;
                if (mCanDragH)
                {
                    int left = Math.Min(v1.Left - m1.Left, v2.Left - m2.Left);
                    int right = Math.Max(v1.Right + m1.Right, v2.Right + m2.Right);
                    float midPointOfTheItems = left + ((right - left) * 0.5F);
                    float midPointOfTheOverlaidItem = (GetLastTouchX() - mDraggingItemInfo.grabbedPositionX) + (mDraggingItemInfo.width * 0.5F);
                    if (toPosition < fromPosition)
                    {
                        if (midPointOfTheOverlaidItem < midPointOfTheItems)
                        {
                            performSwapping = true;
                        }
                    }
                    else
                    {
                        if (midPointOfTheOverlaidItem > midPointOfTheItems)
                        {
                            performSwapping = true;
                        }
                    }
                }

                if (!performSwapping && mCanDragV)
                {
                    int top = Math.Min(v1.Top - m1.Top, v2.Top - m2.Top);
                    int bottom = Math.Max(v1.Bottom + m1.Bottom, v2.Bottom + m2.Bottom);
                    float midPointOfTheItems = top + ((bottom - top) * 0.5F);
                    float midPointOfTheOverlaidItem = (GetLastTouchY() - mDraggingItemInfo.grabbedPositionY) + (mDraggingItemInfo.height * 0.5F);
                    if (toPosition < fromPosition)
                    {
                        if (midPointOfTheOverlaidItem < midPointOfTheItems)
                        {
                            performSwapping = true;
                        }
                    }
                    else
                    {
                        if (midPointOfTheOverlaidItem > midPointOfTheItems)
                        {
                            performSwapping = true;
                        }
                    }
                }
            }
            else
            {
                performSwapping = true;
            }

            if (performSwapping)
            {
                PerformSwapItems(rv, draggingItem, swapTargetHolder, swapTargetMargins, fromPosition, toPosition);
            }
        }
        private void PerformSwapItems(RecyclerView rv, RecyclerView.ViewHolder draggingItemHolder, RecyclerView.ViewHolder swapTargetHolder, Rect swapTargetMargins, int fromPosition, int toPosition)
        {
            if (LOCAL_LOGD)
            {
                Log.Debug(TAG, "item swap (from: " + fromPosition + ", to: " + toPosition + ")");
            }

            if (mItemDragEventListener != null)
            {
                mItemDragEventListener.OnItemDragPositionChanged(fromPosition, toPosition);
            }

            RecyclerView.LayoutManager layoutManager = mRecyclerView.GetLayoutManager();
            int layoutType = CustomRecyclerViewUtils.GetLayoutType(mRecyclerView);
            bool isVertical = (CustomRecyclerViewUtils.ExtractOrientation(layoutType) == CustomRecyclerViewUtils.ORIENTATION_VERTICAL);
            int firstVisible = CustomRecyclerViewUtils.FindFirstVisibleItemPosition(mRecyclerView, false);
            View fromView = (draggingItemHolder != null) ? draggingItemHolder.ItemView : null;
            View toView = swapTargetHolder.ItemView;
            View firstView = CustomRecyclerViewUtils.FindViewByPosition(layoutManager, firstVisible);
            int rootFromPosition = (draggingItemHolder != null) ? draggingItemHolder.LayoutPosition : RecyclerView.NoPosition;
            int rootToPosition = swapTargetHolder.LayoutPosition;
            int? fromOrigin = GetItemViewOrigin(fromView, isVertical);
            int? toOrigin = GetItemViewOrigin(toView, isVertical);
            int? firstOrigin = GetItemViewOrigin(firstView, isVertical);
            mWrapperAdapter.MoveItem(fromPosition, toPosition, layoutType);
            if ((firstVisible == rootFromPosition) && (firstOrigin != null) && (toOrigin != null))
            {
                ScrollBySpecifiedOrientation(rv, -(toOrigin.Value - firstOrigin.Value), isVertical);
                SafeEndAnimations(rv);
            }
            else if ((firstVisible == rootToPosition) && (fromView != null) && (fromOrigin != null) && (!fromOrigin.Equals(toOrigin)))
            {
                ViewGroup.MarginLayoutParams lp = (ViewGroup.MarginLayoutParams)fromView.LayoutParameters;
                int amount = (isVertical) ? -(layoutManager.GetDecoratedMeasuredHeight(fromView) + lp.TopMargin + lp.BottomMargin) : -(layoutManager.GetDecoratedMeasuredWidth(fromView) + lp.LeftMargin + lp.RightMargin);
                ScrollBySpecifiedOrientation(rv, amount, isVertical);
                SafeEndAnimations(rv);
            }
        }
        private static void ScrollBySpecifiedOrientation(RecyclerView rv, int amount, bool vertical)
        {
            if (vertical)
            {
                rv.ScrollBy(0, amount);
            }
            else
            {
                rv.ScrollBy(amount, 0);
            }
        }
        private static int? GetItemViewOrigin(View itemView, bool vertical)
        {
            return (itemView != null) ? ((vertical) ? itemView.Top : itemView.Left) : null;
        }
        private bool CheckTouchedItemState(RecyclerView rv, RecyclerView.ViewHolder holder)
        {
            if (!(holder is IDraggableItemViewHolder))
            {
                return false;
            }

            int wrappedItemPosition = GetWrappedAdapterPosition(holder);
            RecyclerView.Adapter adapter = mWrapperAdapter;
            if (!(wrappedItemPosition >= 0 && wrappedItemPosition < adapter.ItemCount))
            {
                return false;
            }

            return true;
        }
        private static bool SupportsEdgeEffect()
        {
            return Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich;
        }
        
        private static void SafeEndAnimation(RecyclerView rv, RecyclerView.ViewHolder holder)
        {
            RecyclerView.ItemAnimator itemAnimator = (rv != null) ? rv.GetItemAnimator() : null;
            if (itemAnimator != null)
            {
                itemAnimator.EndAnimation(holder);
            }
        }
        
        private static void SafeEndAnimations(RecyclerView rv)
        {
            RecyclerView.ItemAnimator itemAnimator = (rv != null) ? rv.GetItemAnimator() : null;
            if (itemAnimator != null)
            {
                itemAnimator.EndAnimations();
            }
        }
        
        private void SafeEndAnimationsIfRequired(RecyclerView rv)
        {
            if (mSwapTargetItemOperator != null)
            {
                SafeEndAnimations(rv);
            }
        }
        
        private SwapTarget FindSwapTargetItem(SwapTarget dest, FindSwapTargetContext fc, bool alternative)
        {
            RecyclerView.ViewHolder swapTargetHolder = null;
            dest.Clear();
            if ((fc.draggingItem == null) || (GetWrappedAdapterPosition(fc.draggingItem) != RecyclerView.NoPosition && fc.draggingItem.ItemId == fc.draggingItemInfo.id))
            {
                switch (fc.layoutType)
                {
                    case CustomRecyclerViewUtils.LAYOUT_TYPE_GRID_HORIZONTAL:
                    case CustomRecyclerViewUtils.LAYOUT_TYPE_GRID_VERTICAL:
                        swapTargetHolder = FindSwapTargetItemForGridLayoutManager(fc, alternative);
                        break;
                    case CustomRecyclerViewUtils.LAYOUT_TYPE_STAGGERED_GRID_HORIZONTAL:
                    case CustomRecyclerViewUtils.LAYOUT_TYPE_STAGGERED_GRID_VERTICAL:
                        swapTargetHolder = FindSwapTargetItemForStaggeredGridLayoutManager(fc, alternative);
                        break;
                    case CustomRecyclerViewUtils.LAYOUT_TYPE_LINEAR_HORIZONTAL:
                    case CustomRecyclerViewUtils.LAYOUT_TYPE_LINEAR_VERTICAL:
                        swapTargetHolder = FindSwapTargetItemForLinearLayoutManager(fc, alternative);
                        break;
                    default:
                        break;
                }
            }

            if (swapTargetHolder == fc.draggingItem)
            {
                swapTargetHolder = null;
                dest.self = true;
            }

            int swapTargetWrappedItemPosition = GetWrappedAdapterPosition(swapTargetHolder);
            if (swapTargetHolder != null && fc.wrappedAdapterRange != null)
            {
                if (!fc.wrappedAdapterRange.CheckInRange(swapTargetWrappedItemPosition))
                {
                    swapTargetHolder = null;
                }
            }

            dest.holder = swapTargetHolder;
            dest.position = (swapTargetHolder != null) ? swapTargetWrappedItemPosition : RecyclerView.NoPosition;
            return dest;
        }
        private static RecyclerView.ViewHolder FindSwapTargetItemForGridLayoutManager(FindSwapTargetContext fc, bool alternative)
        {
            if (alternative)
            {
                return null;
            }

            RecyclerView.ViewHolder swapTargetHolder;
            swapTargetHolder = FindSwapTargetItemForGridLayoutManagerInternal1(fc);
            if (swapTargetHolder == null)
            {
                swapTargetHolder = FindSwapTargetItemForGridLayoutManagerInternal2(fc);
            }

            return swapTargetHolder;
        }
        private static RecyclerView.ViewHolder FindSwapTargetItemForStaggeredGridLayoutManager(FindSwapTargetContext fc, bool alternative)
        {
            if (alternative)
            {
                return null;
            }

            if (fc.draggingItem == null)
            {
                return null;
            }

            int sx = fc.overlayItemLeft + 1;
            int cx = fc.overlayItemLeft + fc.draggingItemInfo.width / 2 - 1;
            int ex = fc.overlayItemLeft + fc.draggingItemInfo.width - 2;
            int sy = fc.overlayItemTop + 1;
            int cy = fc.overlayItemTop + fc.draggingItemInfo.height / 2 - 1;
            int ey = fc.overlayItemTop + fc.draggingItemInfo.height - 2;
            RecyclerView.ViewHolder csvh, ccvh, cevh;
            if (fc.vertical)
            {
                csvh = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, sx, cy);
                cevh = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, ex, cy);
                ccvh = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, cx, cy);
            }
            else
            {
                csvh = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, cx, sy);
                cevh = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, cx, cy);
                ccvh = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, cx, ey);
            }

            RecyclerView.ViewHolder swapTargetHolder = null;
            if ((ccvh != fc.draggingItem) && (ccvh == csvh || ccvh == cevh))
            {
                swapTargetHolder = ccvh;
            }

            return swapTargetHolder;
        }
        private static RecyclerView.ViewHolder FindSwapTargetItemForGridLayoutManagerInternal1(FindSwapTargetContext fc)
        {
            return CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, fc.lastTouchX, fc.lastTouchY);
        }
        private static RecyclerView.ViewHolder FindSwapTargetItemForGridLayoutManagerInternal2(FindSwapTargetContext fc)
        {
            int spanCount = CustomRecyclerViewUtils.GetSpanCount(fc.rv);
            int height = fc.rv.Height;
            int width = fc.rv.Width;
            int paddingLeft = (fc.vertical) ? fc.rv.PaddingLeft : 0;
            int paddingTop = (!fc.vertical) ? fc.rv.PaddingTop : 0;
            int paddingRight = (fc.vertical) ? fc.rv.PaddingRight : 0;
            int paddingBottom = (!fc.vertical) ? fc.rv.PaddingBottom : 0;
            int columnWidth = (width - paddingLeft - paddingRight) / spanCount;
            int rowHeight = (height - paddingTop - paddingBottom) / spanCount;
            int cx = fc.lastTouchX;
            int cy = fc.lastTouchY;
            int rangeStartIndex = fc.rootAdapterRange.GetStart();
            int rangeEndIndex = fc.rootAdapterRange.GetEnd();
            int scanStartIndex = (int)((fc.vertical) ? (float)(cx - paddingLeft) / columnWidth : (float)(cy - paddingTop) / rowHeight);
            scanStartIndex = Math.Min(Math.Max(scanStartIndex, 0), (spanCount - 1));
            for (int i = scanStartIndex; i >= 0; i--)
            {
                int cx2 = (fc.vertical) ? (paddingLeft + (columnWidth * i) + (columnWidth / 2)) : cx;
                int cy2 = (!fc.vertical) ? (paddingTop + (rowHeight * i) + (rowHeight / 2)) : cy;
                RecyclerView.ViewHolder vh2 = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, cx2, cy2);
                if (vh2 != null)
                {
                    int pos = vh2.AdapterPosition;
                    if ((pos != RecyclerView.NoPosition) && pos >= rangeStartIndex && pos <= rangeEndIndex)
                    {
                        return vh2;
                    }

                    break;
                }
            }

            return null;
        }
        private static RecyclerView.ViewHolder FindSwapTargetItemForLinearLayoutManager(FindSwapTargetContext fc, bool alternative)
        {
            RecyclerView.ViewHolder swapTargetHolder = null;
            if (fc.draggingItem == null)
            {
                return null;
            }

            if (!fc.checkCanSwap && !alternative)
            {
                int draggingItemPosition = fc.draggingItem.AdapterPosition;
                int draggingViewOrigin = (fc.vertical) ? fc.draggingItem.ItemView.Top : fc.draggingItem.ItemView.Left;
                int overlayItemOrigin = (fc.vertical) ? fc.overlayItemTop : fc.overlayItemLeft;
                if (overlayItemOrigin < draggingViewOrigin)
                {
                    if (draggingItemPosition > 0)
                    {
                        swapTargetHolder = fc.rv.FindViewHolderForAdapterPosition(draggingItemPosition - 1);
                    }
                }
                else if (overlayItemOrigin > draggingViewOrigin)
                {
                    if (draggingItemPosition < (fc.rv.GetAdapter().ItemCount - 1))
                    {
                        swapTargetHolder = fc.rv.FindViewHolderForAdapterPosition(draggingItemPosition + 1);
                    }
                }
            }
            else
            {
                float gap = fc.draggingItem.ItemView.Resources.DisplayMetrics.Density * 8;
                float hgap = Math.Min(fc.draggingItemInfo.width * 0.2F, gap);
                float vgap = Math.Min(fc.draggingItemInfo.height * 0.2F, gap);
                float cx = fc.overlayItemLeft + fc.draggingItemInfo.width * 0.5F;
                float cy = fc.overlayItemTop + fc.draggingItemInfo.height * 0.5F;
                RecyclerView.ViewHolder swapTargetHolder1 = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, cx - hgap, cy - vgap);
                RecyclerView.ViewHolder swapTargetHolder2 = CustomRecyclerViewUtils.FindChildViewHolderUnderWithoutTranslation(fc.rv, cx + hgap, cy + vgap);
                if (swapTargetHolder1 == swapTargetHolder2)
                {
                    swapTargetHolder = swapTargetHolder1;
                }
            }

            return swapTargetHolder;
        }
        public virtual void SetItemSettleBackIntoPlaceAnimationDuration(int duration)
        {
            mItemSettleBackIntoPlaceAnimationDuration = duration;
        }
        public virtual int GetItemSettleBackIntoPlaceAnimationDuration()
        {
            return mItemSettleBackIntoPlaceAnimationDuration;
        }
        public virtual void SetItemSettleBackIntoPlaceAnimationInterpolator(IInterpolator interpolator)
        {
            mItemSettleBackIntoPlaceAnimationInterpolator = interpolator;
        }
        public virtual IInterpolator GetItemSettleBackIntoPlaceAnimationInterpolator()
        {
            return mItemSettleBackIntoPlaceAnimationInterpolator;
        }
        public virtual void SetDragStartItemAnimationDuration(int duration)
        {
            mDraggingItemEffectsInfo.durationMillis = duration;
        }
        public virtual int GetDragStartItemAnimationDuration()
        {
            return mDraggingItemEffectsInfo.durationMillis;
        }
        public virtual void SetDragStartItemScaleAnimationInterpolator(IInterpolator interpolator)
        {
            mDraggingItemEffectsInfo.scaleInterpolator = interpolator;
        }
        public virtual IInterpolator GetDragStartItemScaleAnimationInterpolator()
        {
            return mDraggingItemEffectsInfo.scaleInterpolator;
        }
        public virtual void SetDragStartItemRotationAnimationInterpolator(IInterpolator interpolator)
        {
            mDraggingItemEffectsInfo.rotationInterpolator = interpolator;
        }
        public virtual IInterpolator GetDragStartItemRotationAnimationInterpolator()
        {
            return mDraggingItemEffectsInfo.rotationInterpolator;
        }
        public virtual void SetDragStartItemAlphaAnimationInterpolator(IInterpolator interpolator)
        {
            mDraggingItemEffectsInfo.alphaInterpolator = interpolator;
        }
        public virtual IInterpolator GetDragStartItemAlphaAnimationInterpolator()
        {
            return mDraggingItemEffectsInfo.alphaInterpolator;
        }
        public virtual void SetDraggingItemScale(float scale)
        {
            mDraggingItemEffectsInfo.scale = scale;
        }
        public virtual float GetDraggingItemScale()
        {
            return mDraggingItemEffectsInfo.scale;
        }
        public virtual void SetDraggingItemRotation(float rotation)
        {
            mDraggingItemEffectsInfo.rotation = rotation;
        }
        public virtual float GetDraggingItemRotation()
        {
            return mDraggingItemEffectsInfo.rotation;
        }
        public virtual void SetDraggingItemAlpha(float alpha)
        {
            mDraggingItemEffectsInfo.alpha = alpha;
        }
        public virtual float GetDraggingItemAlpha()
        {
            return mDraggingItemEffectsInfo.alpha;
        }
        public virtual void OnItemViewRecycled(RecyclerView.ViewHolder holder)
        {
            if (holder == mDraggingItemViewHolder)
            {
                OnDraggingItemViewRecycled();
            }
            else
            {
                if (mSwapTargetItemOperator != null)
                {
                    mSwapTargetItemOperator.OnItemViewRecycled(holder);
                }
            }
        }
        public virtual RecyclerView.ViewHolder GetDraggingItemViewHolder()
        {
            return mDraggingItemViewHolder;
        }
        public virtual void OnNewDraggingItemViewBound(RecyclerView.ViewHolder holder)
        {
            if (mDraggingItemViewHolder != null)
            {
                OnDraggingItemViewRecycled();
            }

            mDraggingItemViewHolder = holder;
            mDraggingItemDecorator.SetDraggingItemViewHolder(holder);
        }
        private void OnDraggingItemViewRecycled()
        {
            if (LOCAL_LOGI)
            {
                Log.Info(TAG, "a view holder object which is bound to currently dragging item is recycled");
            }

            mDraggingItemViewHolder = null;
            mDraggingItemDecorator.InvalidateDraggingItem();
        }
        private int GetWrappedAdapterPosition(RecyclerView.ViewHolder vh)
        {
            if (vh == null)
            {
                return RecyclerView.NoPosition;
            }

            return WrapperAdapterUtils.UnwrapPosition(mRecyclerView.GetAdapter(), mWrapperAdapter, mComposedAdapterTag, vh.AdapterPosition);
        }
        private ItemDraggableRange ConvertToRootAdapterRange(AdapterPath path, ItemDraggableRange src)
        {
            RecyclerView.Adapter rootAdapter = mRecyclerView.GetAdapter();
            int start = WrapperAdapterUtils.WrapPosition(path, mWrapperAdapter, rootAdapter, src.GetStart());
            int end = WrapperAdapterUtils.WrapPosition(path, mWrapperAdapter, rootAdapter, src.GetEnd());
            return new ItemDraggableRange(start, end);
        }
        private class ScrollOnDraggingProcessRunnable : Java.Lang.Object, IRunnable
        {
            private readonly WeakReference<RecyclerViewDragDropManager> mHolderRef;
            private bool mStarted;
            public ScrollOnDraggingProcessRunnable(RecyclerViewDragDropManager holder)
            {
                mHolderRef = new WeakReference<RecyclerViewDragDropManager>(holder);
            }

            public virtual void Start()
            {
                if (mStarted)
                {
                    return;
                }

                if (!mHolderRef.TryGetTarget(out RecyclerViewDragDropManager holder))
                {
                    return;
                }

                RecyclerView rv = holder.GetRecyclerView();
                if (rv == null)
                {
                    return;
                }

                ViewCompat.PostOnAnimation(rv, this);
                mStarted = true;
            }

            public virtual void Stop()
            {
                if (!mStarted)
                {
                    return;
                }

                mStarted = false;
            }

            public virtual void Release()
            {
                mHolderRef.SetTarget(null);
                mStarted = false;
            }

            public virtual void Run()
            {
                if (!mHolderRef.TryGetTarget(out RecyclerViewDragDropManager holder))
                {
                    return;
                }

                if (!mStarted)
                {
                    return;
                }
                holder.HandleScrollOnDragging();
                RecyclerView rv = holder.GetRecyclerView();
                if (rv != null && mStarted)
                {
                    ViewCompat.PostOnAnimation(rv, this);
                }
                else
                {
                    mStarted = false;
                }
            }
        }
        
        private class InternalHandler : Handler
        {
            private const int MSG_LONGPRESS = 1;
            private const int MSG_DEFERRED_CANCEL_DRAG = 2;
            private const int MSG_CHECK_ITEM_VIEW_SIZE_UPDATE = 3;
            private RecyclerViewDragDropManager mHolder;
            private MotionEvent mDownMotionEvent;
            public InternalHandler(RecyclerViewDragDropManager holder)
            {
                mHolder = holder;
            }

            public virtual void Release()
            {
                RemoveCallbacksAndMessages(null);
                mHolder = null;
            }

            public override void HandleMessage(Message msg)
            {
                switch (msg.What)
                {
                    case MSG_LONGPRESS:
                        mHolder.HandleOnLongPress(mDownMotionEvent);
                        break;
                    case MSG_DEFERRED_CANCEL_DRAG:
                        mHolder.CancelDrag(true);
                        break;
                    case MSG_CHECK_ITEM_VIEW_SIZE_UPDATE:
                        mHolder.HandleOnCheckItemViewSizeUpdate();
                        break;
                }
            }

            public virtual void StartLongPressDetection(MotionEvent e, int timeout)
            {
                CancelLongPressDetection();
                mDownMotionEvent = MotionEvent.Obtain(e);
                SendEmptyMessageAtTime(MSG_LONGPRESS, e.DownTime + timeout);
            }

            public virtual void CancelLongPressDetection()
            {
                RemoveMessages(MSG_LONGPRESS);
                if (mDownMotionEvent != null)
                {
                    mDownMotionEvent.Recycle();
                    mDownMotionEvent = null;
                }
            }

            public virtual void RemoveDeferredCancelDragRequest()
            {
                RemoveMessages(MSG_DEFERRED_CANCEL_DRAG);
            }

            public virtual void RequestDeferredCancelDrag()
            {
                if (IsCancelDragRequested())
                {
                    return;
                }

                SendEmptyMessage(MSG_DEFERRED_CANCEL_DRAG);
            }

            public virtual bool IsCancelDragRequested()
            {
                return HasMessages(MSG_DEFERRED_CANCEL_DRAG);
            }

            public virtual void ScheduleDraggingItemViewSizeUpdateCheck()
            {
                SendEmptyMessage(MSG_CHECK_ITEM_VIEW_SIZE_UPDATE);
            }

            public virtual void RemoveDraggingItemViewSizeUpdateCheckRequest()
            {
                RemoveMessages(MSG_CHECK_ITEM_VIEW_SIZE_UPDATE);
            }
        }
    }
}