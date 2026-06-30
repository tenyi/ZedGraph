// <copyright file="GraphPaneFactory.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   為測試快速建立 <see cref="GraphPane"/> 與執行 <see cref="GraphPane.AxisChange"/>。
//   沿用既有 NUnit 測試（OHLCBarItemTests、ScaleTests）的模式，但加上 AxisType 等常用參數化。
// </summary>

namespace ZedGraph.XUnitTests.TestHelpers
{
    using System.Drawing;

    /// <summary>
    /// 測試用的 GraphPane 工廠。
    /// </summary>
    public static class GraphPaneFactory
    {
        /// <summary>預設測試尺寸。</summary>
        public const int DefaultWidth = 640;

        /// <summary>預設測試高度。</summary>
        public const int DefaultHeight = 480;

        /// <summary>
        /// 建立指定 X 軸類型、尺寸的 <see cref="GraphPane"/>，
        /// 並立即執行 <see cref="GraphPane.AxisChange"/>。
        /// </summary>
        /// <param name="graphics">繪圖介面（用於量測字型）。</param>
        /// <param name="xAxisType">X 軸類型，預設 <see cref="AxisType.DateAsOrdinal"/>。</param>
        /// <param name="width">Pane 寬度。</param>
        /// <param name="height">Pane 高度。</param>
        /// <returns>已完成 AxisChange 的 GraphPane。</returns>
        public static GraphPane Create(
            Graphics graphics,
            AxisType xAxisType = AxisType.DateAsOrdinal,
            int width = DefaultWidth,
            int height = DefaultHeight)
        {
            var pane = new GraphPane();
            pane.Rect = new RectangleF(0, 0, width, height);
            pane.XAxis.Type = xAxisType;
            pane.AxisChange(graphics);
            return pane;
        }
    }
}
