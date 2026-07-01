# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 專案概述

ZedGraph 是 .NET 平台的 2D 圖表類別庫，支援 Line、Bar、Pie 等圖表類型，由約 80 個 C# 檔案組成，採用巢狀結構設計。

## 方案架構（6 個專案）

| 專案 | 路徑 | 用途 |
|------|------|------|
| ZedGraph | source\ZedGraph\ | 核心類別庫 |
| ZedGraph.Web | web\ | ASP.NET Web 控制項 |
| ZedGraph.ControlTest | controltest\ | WinForms 控制項測試 |
| ZedGraph.Demo | demo\ | 範例展示 |
| ZedGraph.LibTest | libtest\ | 程式庫測試 |
| ZedGraph.UnitTest | unittest\ | 單元測試 |

## 核心類別層次

### 圖表容器層
- `MasterPane` → 含有多個 `GraphPane`
- `GraphPane` → 單一圖表區塊的主要容器（92KB，最大的類別）

### 資料模型
- `IPointList` → 資料點介面
- `PointPair` → 單一資料點（X, Y, Z, 標籤）
- `PointPairList` → `IPointList` 的主要實作
- `RollingPointPairList` → 滾動資料更新

### 圖形項目（所有實作 `CurveItem`）
- `LineItem` → 線圖
- `BarItem` → 柱狀圖
- `StickItem` → 棒圖
- `PieItem` → 餅圖
- `ErrorBarItem` → 誤差棒
- `JapaneseCandleStickItem` → K 線圖
- `HiLowBarItem` → 高低桿圖

### 座標軸系統
- `Axis` → 基底類別
- `XAxis` / `YAxis` / `Y2Axis` / `X2Axis` → 具體實作
- `Scale` → 負責資料座標 ↔ 螢幕座標轉換
- 各類型: `LinearScale`, `LogScale`, `DateScale`, `OrdinalScale`, `TextScale`, `LinearAsOrdinalScale`, `ExponentScale`

### UI 控制項
- `ZedGraphControl` → WinForms 使用者控制項
- 拆分多個檔案: `ZedGraphControl.cs`, `.Events.cs`, `.Properties.cs`, `.ContextMenu.cs`, `.ScrollBars.cs`, `.Printing.cs`

## 建置命令

```bash
# 建置整個方案
dotnet build ZedGraph.sln

# 建置特定專案
dotnet build source\ZedGraph\ZedGraph.csproj

# 執行單元測試
dotnet test unittest\ZedGraph.UnitTest.csproj

# 清除建置產出
dotnet clean ZedGraph.sln
```

## 重要檔案說明

- `source\ZedGraph\GraphPane.cs` - 核心繪圖邏輯，掌管所有圖形項目的配置與渲染
- `source\ZedGraph\Scale.cs` - 座標轉換的核心演算法（114KB）
- `source\ZedGraph\ZedGraphControl.cs` - 使用者互動（縮放、拖曳、工具提示）
- `source\ZedGraph\Types.cs` - 列舉型別定義（顏色、符號、線型等）
- `source\ZedGraph\XDate.cs` - 日期時間處理工具

## 在既有類別上新增功能

修改圖形項目（如 `StickItem`）時：
1. 確認該類別是否有 `Properties` 設定
2. 檢查 `GraphPane.Draw()` 方法中的繪製邏輯
3. 如需新增 `*Item` 類別，必須實作 `CurveItem` 介面並實例化於 `GraphPane` 的 `CurveList` 中

## 國際化

多語系資源檔位於 `source\ZedGraph\ZedGraphLocale.*.resx`，主要支援 12 種語言。

## NuGet 發布

使用 `.nuget\` 目錄中的 `nuget.exe` 進行套件封裝與發布。