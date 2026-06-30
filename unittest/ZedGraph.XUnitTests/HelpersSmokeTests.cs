// <copyright file="HelpersSmokeTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   測試 helper 自身的健全性測試。
//   確認 GraphicsFixture / GraphPaneFactory 可正常運作，作為 T0.1.2 的第一步。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System.Drawing;

    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class HelpersSmokeTests
    {
        [Fact]
        public void GraphicsFixture_BitmapAndGraphics_NotNull()
        {
            using var fx = new GraphicsFixture(640, 480);
            Assert.NotNull(fx.Bitmap);
            Assert.NotNull(fx.Graphics);
            Assert.Equal(640, fx.Bitmap.Width);
            Assert.Equal(480, fx.Bitmap.Height);
        }

        [Fact]
        public void GraphicsFixture_NegativeWidth_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GraphicsFixture(-1, 480));
        }

        [Fact]
        public void GraphPaneFactory_Create_RectAndAxisApplied()
        {
            using var fx = new GraphicsFixture();
            var pane = GraphPaneFactory.Create(fx.Graphics, AxisType.DateAsOrdinal);
            Assert.NotNull(pane);
            Assert.Equal(640f, pane.Rect.Width);
            Assert.Equal(AxisType.DateAsOrdinal, pane.XAxis.Type);
        }
    }
}
