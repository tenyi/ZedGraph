namespace ZedGraph
{
    using System.Drawing;

    /// <summary>
    /// Provides a callback-based interface for showing a tool tip with a caption at a
    /// given point on a chart.
    /// </summary>
    public interface IValuesToolTip
    {
        /// <summary>
        /// Disables the tool tip.
        /// </summary>
        void Disable();

        /// <summary>
        /// Enables the tool tip.
        /// </summary>
        void Enable();

        /// <summary>
        /// Sets the caption for the tool tip at the specified point.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="point">The point.</param>
        void Set(string caption, Point point);
    }
}
