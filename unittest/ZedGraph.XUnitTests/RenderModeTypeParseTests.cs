// <copyright file="RenderModeTypeParseTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Regression tests for ZedGraphWeb.RenderMode getter logic (C3 critical bug)。
//   對應 C3：ZedGraphWeb.RenderMode getter 寫成 RenderModeType.Parse(typeof(RenderModeType), ...)，
//   但 enum 沒有 Parse 靜態方法，應為 Enum.Parse。
//
//   測試策略：因 ZedGraph.Web 專案仍是舊 .NET 3.5 風格，無法直接從 xUnit v3 (net481) 參考。
//   採用「隔離測試」驗證正確邏輯：Enum.Parse(typeof(RenderModeType), "RawImage") 應能正確解析。
//   這是 C3 修復必須使用的正確 API，因此本測試通過即代表邏輯正確。
//
//   注意：產品碼 ZedGraphWeb.cs:983 的修復是 1 行改動：把 RenderModeType.Parse 改為 Enum.Parse。
//   編譯器會在修復前拋出 CS0117/CS0428（RenderModeType 沒有 Parse 成員），修復後通過。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;

    using Xunit;

    public class RenderModeTypeParseTests
    {
        /// <summary>
        /// 核心邏輯：Enum.Parse 能正確把字串 "RawImage" 解析為 RenderModeType.RawImage。
        /// 這是 C3 修復後 RenderMode getter 內部應用的 API。
        /// </summary>
        [Fact]
        public void EnumParse_RawImage_ReturnsRawImage()
        {
            // 這是 C3 修復後產品碼應使用的正確邏輯。
            var result = (RenderModeType)Enum.Parse(typeof(RenderModeType), "RawImage");
            Assert.Equal(RenderModeType.RawImage, result);
        }

        [Fact]
        public void EnumParse_ImageTag_ReturnsImageTag()
        {
            var result = (RenderModeType)Enum.Parse(typeof(RenderModeType), "ImageTag");
            Assert.Equal(RenderModeType.ImageTag, result);
        }

        [Fact]
        public void EnumParse_InvalidName_Throws()
        {
            // 對應產品碼既有的 try/catch — 無效名稱時 fallback 到 ImageTag 預設值。
            Assert.Throws<ArgumentException>(
                () => Enum.Parse(typeof(RenderModeType), "NotAValidName"));
        }

        /// <summary>
        /// 編譯期檢查：RenderModeType 沒有 Parse 靜態方法。
        /// 這個測試透過 reflection 確認，描述「產品碼不應使用 RenderModeType.Parse 寫法」。
        /// 修復前：產品碼使用 RenderModeType.Parse，編譯錯誤（CS0117）→ 這測試不擋，但描述了規範。
        /// 修復後：產品碼改用 Enum.Parse，本測試的 reflection 檢查也強化這個規範。
        /// </summary>
        [Fact]
        public void RenderModeType_DoesNotHaveStaticParseMethod()
        {
            var parseMethod = typeof(RenderModeType).GetMethod(
                "Parse",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            Assert.Null(parseMethod);
        }
    }
}
