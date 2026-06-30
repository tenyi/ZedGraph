// <copyright file="LineInterpolatePointCatchTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   H3.1 補測：守護 Line.InterpolatePoint catch 路徑的兩個關鍵契約。
//   原 H3.1 修改將 `catch { }` 改為 `catch (Exception ex) { Debug.WriteLine(...) }`，
//   兩個關鍵行為：
//     (a) 內部例外被吞、不外拋（外部契約不變）
//     (b) 透過 Debug.WriteLine 輸出診斷訊息（新行為）
//
//   既有的 GdiCharacterizationSanityTests.Draw_LineItemCurve_DoesNotThrow 用正常輸入，
//   不會觸發 catch，因此無法驗證 catch 路徑。本測試用反射呼叫 private InterpolatePoint，
//   並傳入 null Graphics 觸發 NullReferenceException，穩定進入 catch 路徑。
//
//   T-H3.2.a（ZedGraphControl.OnPaint）：需要 Form-level 測試宿主觸發 Paint 事件，
//   xUnit v3 net48 測試主機無訊息迴圈，目前無法直接測；參考舊 NUnit 專案 ZGTest.cs:115
//   的 Form+ControlTestHelper 模式可於未來補測。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Reflection;

    using ZedGraph.XUnitTests.TestHelpers;

    using Xunit;

    public class LineInterpolatePointCatchTests
    {
        /// <summary>
        /// 取得 private 方法 Line.InterpolatePoint 的反射 MethodInfo。
        /// </summary>
        private static MethodInfo GetInterpolatePointMethod()
        {
            var method = typeof(Line).GetMethod(
                "InterpolatePoint",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            return method;
        }

        /// <summary>
        /// 構造標準測試環境：建立帶有 LineItem 的 GraphPane 並 AxisChange。
        /// </summary>
        private static (GraphPane pane, LineItem curve, PointPair pt) BuildTestFixture(
            GraphicsFixture fx)
        {
            var pane = new GraphPane(new RectangleF(0, 0, 640, 480), "title", "X", "Y");
            pane.AddCurve("c", new[] { 1.0, 2.0, 3.0, 4.0 },
                new[] { 10.0, 20.0, 15.0, 30.0 }, Color.Blue);
            pane.AxisChange(fx.Graphics);
            var curve = (LineItem)pane.CurveList[0];
            var pt = curve.Points[0];
            return (pane, curve, pt);
        }

        /// <summary>
        /// T-H3.1.a：傳入 null Graphics 觸發內部 NRE，驗證例外被吞、未外拋 TargetInvocationException。
        /// 守護「外部契約不變」——單一繪製例外不會中斷呼叫端的流程。
        /// </summary>
        [Fact]
        public void InterpolatePoint_NullGraphics_SwallowsException_DoesNotThrow()
        {
            using var fx = new GraphicsFixture(640, 480);
            var (pane, curve, pt) = BuildTestFixture(fx);
            var line = new Line(Color.Blue);
            var method = GetInterpolatePointMethod();

            using (var pen = new Pen(Color.Blue))
            {
                // 反射呼叫：傳 null Graphics 會在 InterpolatePoint 內 NRE → 進入 catch
                Exception caught = null;
                caught = Record.Exception(() => method.Invoke(
                    line,
                    new object[] { null, pane, curve, pt, 1.0f, pen, 0f, 0f, 100f, 100f }));

                // 若 catch 沒吞掉例外，Invoke 會拋 TargetInvocationException
                // 反之：catch 吞掉後 Invoke 直接成功回傳 → caught 為 null
                Assert.Null(caught);
            }
        }

        /// <summary>
        /// T-H3.1.b：透過 DebugTraceListener 攔截 Debug.WriteLine 輸出，
        /// 驗證 catch 路徑會輸出含「swallowed exception」的診斷訊息。
        /// 守護「新行為」——Debug 診斷輸出確實執行，不是 dead code。
        /// </summary>
        [Fact]
        public void InterpolatePoint_NullGraphics_DebugLogsSwallowedException()
        {
            using var fx = new GraphicsFixture(640, 480);
            var (pane, curve, pt) = BuildTestFixture(fx);
            var line = new Line(Color.Blue);
            var method = GetInterpolatePointMethod();

            using var listener = new DebugTraceListener();
            Debug.Listeners.Add(listener);

            try
            {
                using (var pen = new Pen(Color.Blue))
                {
                    Record.Exception(() => method.Invoke(
                        line,
                        new object[] { null, pane, curve, pt, 1.0f, pen, 0f, 0f, 100f, 100f }));
                }

                // 驗證 Debug 輸出包含「swallowed exception」字樣
                Assert.Contains(
                    "swallowed exception",
                    listener.Output,
                    StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Debug.Listeners.Remove(listener);
            }
        }

        /// <summary>
        /// 守護「正常輸入路徑」仍可走通——InterpolatePoint 對合法參數不應拋例外。
        /// 這是 baseline，避免 catch 邏輯改動（如 catch 改為 throw）誤傷正常路徑。
        /// </summary>
        [Fact]
        public void InterpolatePoint_NormalInput_DoesNotThrow()
        {
            using var fx = new GraphicsFixture(640, 480);
            var (pane, curve, pt) = BuildTestFixture(fx);
            var line = new Line(Color.Blue);
            var method = GetInterpolatePointMethod();

            using (var pen = new Pen(Color.Blue))
            {
                Exception caught = Record.Exception(() => method.Invoke(
                    line,
                    new object[] { fx.Graphics, pane, curve, pt, 1.0f, pen, 10f, 20f, 110f, 120f }));

                Assert.Null(caught);
            }
        }
    }
}