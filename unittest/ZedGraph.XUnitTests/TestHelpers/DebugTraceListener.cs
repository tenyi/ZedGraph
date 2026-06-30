// <copyright file="DebugTraceListener.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   可重複使用的 TraceListener：攔截 <see cref="System.Diagnostics.Debug.WriteLine"/>
//   與 <see cref="System.Diagnostics.Debug.Write"/> 呼叫，把訊息累積於內部緩衝區。
//   測試結束後透過 Dispose 從 <see cref="Debug.Listeners"/> 移除，避免污染其他測試。
// </summary>

namespace ZedGraph.XUnitTests.TestHelpers
{
    using System;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// 為測試提供 <see cref="TraceListener"/> 攔截 <see cref="Debug.Write(string)"/>
    /// 與 <see cref="Debug.WriteLine(string)"/> 呼叫，累積輸出供後續斷言使用。
    /// 使用方式：
    /// <code>
    /// using var listener = new DebugTraceListener();
    /// Debug.Listeners.Add(listener);
    /// try { /* 執行受測程式碼 */ } finally { Debug.Listeners.Remove(listener); }
    /// Assert.Contains("expected", listener.Output);
    /// </code>
    /// </summary>
    public sealed class DebugTraceListener : TraceListener, IDisposable
    {
        /// <summary>累積所有 Write/WriteLine 的訊息字串。</summary>
        private readonly StringBuilder _buffer = new StringBuilder();

        /// <summary>取得目前累積的輸出內容。</summary>
        public string Output => _buffer.ToString();

        /// <inheritdoc/>
        public override void Write(string message)
        {
            _buffer.Append(message);
        }

        /// <inheritdoc/>
        public override void WriteLine(string message)
        {
            _buffer.AppendLine(message);
        }

        /// <summary>
        /// 從 <see cref="Debug.Listeners"/> 移除本 listener，避免污染其他測試。
        /// 注意：用 <c>new</c> 隱藏基礎 <see cref="TraceListener.Dispose()"/>，
        /// 因為測試語意是「從測試環境卸載」而非釋放資源（TraceListener 本身無 unmanaged 資源）。
        /// </summary>
        public new void Dispose()
        {
            Debug.Listeners.Remove(this);
        }
    }
}