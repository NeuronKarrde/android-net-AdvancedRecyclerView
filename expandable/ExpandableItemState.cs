using Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable.Annotation;

namespace Com.H6ah4i.Android.Widget.Advrecyclerview.Expandable
{
    /// <summary>
    /// Helper class for decoding {@link ExpandableItemViewHolder#getExpandStateFlags()} flag values.
    /// </summary>
    public class ExpandableItemState
    {
        public int Flags { get; set; }

        /// <summary>
        /// Checks whether the swiping is currently performed.
        /// </summary>
        /// <returns>True if the user is swiping an item, otherwise else.</returns>
        public virtual bool IsSwiping => (Flags & (int)ExpandableItemConstants.STATE_FLAG_IS_EXPANDED) != 0;

        /// <summary>
        /// Checks whether the item is a child item.
        /// </summary>
        /// <returns>True if the associated item is a child item, otherwise false.</returns>
        public virtual bool IsChild => (Flags & (int)ExpandableItemConstants.STATE_FLAG_IS_CHILD) != 0;

        /// <summary>
        /// Checks whether the item is a group item.
        /// </summary>
        /// <returns>True if the associated item is a child item, otherwise false.</returns>
        public virtual bool IsGroup => (Flags & (int)ExpandableItemConstants.STATE_FLAG_IS_GROUP) != 0;

        /// <summary>
        /// Checks whether the group is expanded.
        /// </summary>
        /// <returns>True if the expandable group is expanded, otherwise false.</returns>
        public virtual bool IsExpanded => (Flags & (int)ExpandableItemConstants.STATE_FLAG_IS_EXPANDED) != 0;

        /// <summary>
        /// Checks whether the expanded state is changed or not. You can use this method to determine the group item indicator should animate.
        /// </summary>
        /// <returns>True if the group's expanded state has changed, otherwise false.</returns>
        public virtual bool HasExpandedStateChanged => (Flags & (int)ExpandableItemConstants.STATE_FLAG_HAS_EXPANDED_STATE_CHANGED) != 0;

        /// <summary>
        /// Checks whether state flags are changed or not.
        /// </summary>
        /// <returns>True if flags are updated, otherwise false.</returns>
        public virtual bool IsUpdated => (Flags & (int)ExpandableItemConstants.STATE_FLAG_IS_UPDATED) != 0;
    }
}