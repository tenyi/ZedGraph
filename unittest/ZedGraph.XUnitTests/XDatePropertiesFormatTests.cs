// <copyright file="XDatePropertiesFormatTests.cs" company="ZedGraph Project">
//   This library is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
// </copyright>

// <summary>
//   Batch 5 (B5-A1) 系統性 characterization 測試：鎖定 XDate 的 Properties、
//   Julian/Decimal Year 雙向轉換、邊界截斷、以及 ToString 格式化 token 行為。
//
//   設計重點：
//     1. XDate 無 Year/Month/Day 獨立 property（經 XLDateToCalendarDate out params），
//        本檔僅測實際存在的 properties：JulianDay、DecimalYear、IsValidDate。
//     2. 格式化 token（[d][hh][mm][ss]）測試同時作為 B5-B1（IndexOf→Ordinal）的
//        characterization：鎖定「token 替換後的字串」，確保現代化前後輸出不變。
//     3. token 替換後 fmtStr 為純數字字面，DateTime.ToString 對純字面照抄，
//        故格式化測試文化無關、可重現。
// </summary>

namespace ZedGraph.XUnitTests
{
    using System;

    using Xunit;

    public class XDatePropertiesFormatTests
    {
        // ====================================================================
        // Julian Day
        // ====================================================================

        /// <summary>
        /// JulianDay property 的 get/set 應為 round-trip（設什麼取什麼）。
        /// 鎖定 JulianDay property 的單純存取語意。
        /// </summary>
        [Fact]
        public void JulianDay_Property_GetSet_RoundTrip()
        {
            var x = new XDate(2000, 1, 1);
            double saved = x.JulianDay;

            // 將 JulianDay 設為另一個值再取回，應一致
            x.JulianDay = saved + 10.0;

            Assert.Equal(saved + 10.0, x.JulianDay);
        }

        /// <summary>
        /// XLDateToJulianDay 的公式應為 xlDate + XLDay1（XLDay1 = 2415018.5）。
        /// 鎖定此線性關係，供未來重構驗證。
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(36526.0)]   // 約 2000-01-01
        public void XLDateToJulianDay_Is_XLDate_Plus_XLDay1(double xlDate)
        {
            Assert.Equal(xlDate + XDate.XLDay1, XDate.XLDateToJulianDay(xlDate));
        }

        // ====================================================================
        // DecimalYear
        // ====================================================================

        /// <summary>
        /// DecimalYear 雙向轉換應為 round-trip：XLDate → DecimalYear → XLDate 還原。
        /// 鎖定 XLDateToDecimalYear / DecimalYearToXLDate 的可逆性（精度 3 位小數）。
        /// </summary>
        [Fact]
        public void DecimalYear_RoundTrip_RestoresXLDate()
        {
            var x = new XDate(2000, 6, 15, 12, 30, 0);
            double dec = x.DecimalYear;

            var y = new XDate { DecimalYear = dec };

            // DecimalYear 內部為年分數近似，容許小數位等級誤差
            Assert.Equal(x.XLDate, y.XLDate, 3);
        }

        /// <summary>
        /// 年初（1月1日）的 DecimalYear 應非常接近該年份整數值。
        /// 鎖定 DecimalYear 的絕對量尺（年份整數對齊）。
        /// </summary>
        [Fact]
        public void DecimalYear_AtNewYear_CloseToYearInteger()
        {
            var x = new XDate(2000, 1, 1);

            // 年初應落在 2000.0 附近（容許 ±0.01 年 ≈ 3.6 天的演算法差異）
            Assert.InRange(x.DecimalYear, 1999.99, 2000.01);
        }

        // ====================================================================
        // 邊界：IsValidDate / MakeValidDate
        // ====================================================================

        /// <summary>
        /// IsValidDate 對合法日期應為 true，對超出 [XLDayMin, XLDayMax] 應為 false。
        /// 鎖定有效範圍邊界語意。
        /// </summary>
        [Fact]
        public void IsValidDate_TrueForValid_FalseForOutOfRange()
        {
            var valid = new XDate(2000, 1, 1);
            Assert.True(valid.IsValidDate);

            // 故意構造超出下界的 XLDate（XLDayMin 對應 4713 B.C.）
            var tooLow = new XDate { XLDate = XDate.XLDayMin - 1.0 };
            Assert.False(tooLow.IsValidDate);
        }

        /// <summary>
        /// MakeValidDate 應將超出範圍的值截斷（clamp）至邊界。
        /// 鎖定截斷行為：低於下界→XLDayMin，高於上界→XLDayMax。
        /// </summary>
        [Fact]
        public void MakeValidDate_Clamps_ToBoundary()
        {
            Assert.Equal(XDate.XLDayMin, XDate.MakeValidDate(XDate.XLDayMin - 100.0));
            Assert.Equal(XDate.XLDayMax, XDate.MakeValidDate(XDate.XLDayMax + 100.0));

            // 範圍內的值應原樣返回
            double inside = XDate.XLDayMin + 1.0;
            Assert.Equal(inside, XDate.MakeValidDate(inside));
        }

        // ====================================================================
        // 格式化 token（同時為 B5-B1 之 characterization）
        // ====================================================================

        /// <summary>
        /// ToString 對無效日期應回傳固定字串 "Date Error"。
        /// 鎖定無效日期的錯誤路徑（CheckValidDate 為 false）。
        /// </summary>
        [Fact]
        public void ToString_InvalidDate_ReturnsDateError()
        {
            double invalid = XDate.XLDayMin - 1.0;

            Assert.Equal("Date Error", XDate.ToString(invalid, "yyyy-MM-dd"));
        }

        /// <summary>
        /// ToString 的 token 替換：[d] 移除整數日後，[hh][mm][ss] 才對應當日時間。
        /// 以 2000-01-01 12:30:45 驗證，格式 "[d]T[hh]:[mm]:[ss]" 應以 "T12:30:45" 結尾。
        ///
        /// 注意：必須先有 [d] 移除整數日，否則 [hh] 會混入天數（這是現有實作語意）。
        /// 本測試即鎖定此語意，供 B5-B1（IndexOf→Ordinal）驗證行為不變。
        /// </summary>
        [Fact]
        public void ToString_TokenReplacement_HHmmss_MatchesDateTime()
        {
            var x = new XDate(2000, 1, 1, 12, 30, 45);
            string result = XDate.ToString(x.XLDate, "[d]T[hh]:[mm]:[ss]");

            // 時間部分必須精確為 12:30:45；日期([d])為天數數字，僅斷言存在
            Assert.EndsWith("T12:30:45", result);
        }

        /// <summary>
        /// ToString 對包含 token 的格式，替換後不應殘留任何 "[" 字元（所有 token 皆被替換）。
        /// 鎖定「token 全數解析」的契約，文化無關。
        /// </summary>
        [Fact]
        public void ToString_AllTokens_Replaced_NoBracketRemains()
        {
            var x = new XDate(2024, 6, 15, 8, 5, 3);
            string result = XDate.ToString(x.XLDate, "[d]d[hh]h[mm]m[ss]s");

            Assert.DoesNotContain("[", result);
            Assert.DoesNotContain("]", result);
        }
    }
}
