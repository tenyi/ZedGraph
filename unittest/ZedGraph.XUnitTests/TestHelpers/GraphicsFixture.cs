// <copyright file="GraphicsFixture.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   可重複使用的 Graphics / Bitmap 容器。
//   透過 xUnit 的 using-statement Dispose 模式，自動釋放 GDI+ handle。
// </summary>

namespace ZedGraph.XUnitTests.TestHelpers
{
    using System;
    using System.Drawing;

    /// <summary>
    /// 為測試提供固定大小的 <see cref="Bitmap"/> 與其上的 <see cref="Graphics"/>。
    /// 實作 <see cref="IDisposable"/> 以確保 GDI+ 資源釋放。
    /// 使用方式：<c>using var fx = new GraphicsFixture(640, 480);</c>
    /// </summary>
    public sealed class GraphicsFixture : IDisposable
    {
        /// <summary>測試用的點陣圖（繪圖目標）。</summary>
        public Bitmap Bitmap { get; }

        /// <summary>從 <see cref="Bitmap"/> 取得的繪圖介面。</summary>
        public Graphics Graphics { get; }

        /// <summary>
        /// 建立指定尺寸的 Bitmap + Graphics 配對。
        /// </summary>
        /// <param name="width">寬度（像素），預設 640。</param>
        /// <param name="height">高度（像素），預設 480。</param>
        public GraphicsFixture(int width = 640, int height = 480)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "寬度必須大於 0。");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "高度必須大於 0。");
            }

            this.Bitmap = new Bitmap(width, height);
            this.Graphics = Graphics.FromImage(this.Bitmap);
        }

        /// <summary>
        /// 釋放 Graphics 與 Bitmap 資源。
        /// 注意順序：先釋放 Graphics，再釋放 Bitmap。
        /// </summary>
        public void Dispose()
        {
            this.Graphics?.Dispose();
            this.Bitmap?.Dispose();
        }
    }
}
