# Batch 0 — 測試安全網（Test Foundation）

> **目標**：在動任何產品碼之前，為「即將被觸碰」的 class/method 建立測試安全網。
> 這是所有後續批次（Batch 1–5）的**前置硬性門檻**。
>
> **退出條件**：所有 T0.x 完成且 `dotnet test` 全綠；每個後續批次欲修改的 method 都有對應測試。

---

## T0.1 — 測試基礎設施

- [x] **T0.1.0**（已先完成）建立 `unittest\ZedGraph.XUnitTests\` SDK 風格測試專案
  - 套件：`xunit.v3` 3.2.2、`Microsoft.NET.Test.Sdk` 17.12.0、`xunit.runner.visualstudio` 3.1.5、`coverlet.collector` 6.0.2
  - csproj：`<OutputType>Exe</OutputType>` + `<TargetFramework>net481</TargetFramework>` + `<UseWindowsForms>true</UseWindowsForms>`
  - 已驗證：`dotnet build` 0 錯誤；`dotnet test` 全綠
  - 原 `unittest\ZedGraph.UnitTest.csproj` 保留不動（已升級 NUnit 3.14，VS 2026 可執行，歷史 baseline）

- [x] **T0.1.1** 新測試專案穩定可重複執行：`dotnet test` 多次重跑皆綠（目前 **80 個測試全綠**）

- [x] **T0.1.2** 共用 helper（`unittest\ZedGraph.XUnitTests\TestHelpers\`）：
  - `GraphPaneFactory.cs` — 封裝 `new GraphPane(...)` + `AxisChange` 標準流程
  - `GraphicsFixture.cs` — `IDisposable` 包裝 `Bitmap` + `Graphics`，避免 GDI+ 洩漏
  - `PointListBuilder.cs` — 快速建立 `PointPairList`（空、線性、NaN/極值、Stock 序列）

- [x] **T0.1.3** Coverage 量測評估：
  - 命令列 coverlet / `dotnet test --collect:"XPlat Code Coverage"` 對 **net48 + OutputType=Exe** 不相容（持續報 0%）
  - 採 fallback：以「method 有無對應測試」人工清點（見下方覆蓋率記錄）
  - 精確 % 留待 VS 內 FineCodeCoverage 測量（使用者環境已裝）

---

## T0.2 — Critical Bug 的 Regression Tests（修復前應 **失敗**）

> 這些 test 描述「應有的正確行為」。Batch 1 已修復，全部轉綠。

- [x] **T0.2.1** `AxisLabelSerializationTests`（4 個，捕捉 C1）
- [x] **T0.2.2** `ScaleSetScaleMagTests`（3 個，捕捉 C2）
- [x] **T0.2.3** `RenderModeTypeParseTests`（4 個，隔離測 C3）
- [x] **T0.2.4** `WebSafeEncodingContractTests`（7 個，BCL 契約測 C4）
- [x] **T0.2.5** `HandlePointValuesEdgeCaseTests`（3 個，反射隔離測 C5）

---

## T0.3 — 重構目標的 Characterization Tests（重構前應 **通過**）

> 鎖定現有行為，確保 Batch 3/4 重構後外部行為不變。每完成一項即解鎖對應批次。
> **等級標記**：[有意義] = 鎖定具體可驗證行為；[sanity] = 守護「重構後不崩潰」的最低契約（GDI+ 重依賴類無法精確斷言）。

### Batch 4（效能）解鎖用

- [x] **T0.3.1** `ScaleTransformTests` — `Scale.Transform` / `ReverseTransform`（4 個）[sanity]
- [x] **T0.3.2** `Symbol.Draw` / `MakePath` — 涵蓋於 `GdiCharacterizationSanityTests.Draw_LineItemCurve_DoesNotThrow` [sanity]
- [x] **T0.3.3** `Line.Draw` / `BuildPointsArray` — 涵蓋於 `GdiCharacterizationSanityTests.Draw_LineItemCurve_DoesNotThrow` [sanity]
- [x] **T0.3.4** `CurveList.SortOverlay` / `DrawSingleBar` — 涵蓋於 `Draw_SortedOverlayBars_DoesNotThrow` [sanity]
- [x] **T0.3.5** `FontSpec.BoundingBox` — `GdiCharacterizationSanityTests`（4 個，含旋轉形狀反轉）[有意義]

### Batch 3（正確性/死碼）解鎖用

- [x] **T0.3.6** `ScaleMyModTests` — `Scale.MyMod`（8 個，鎖定負數數學模數行為）[有意義]
- [x] **T0.3.7** `LinearScalePickScaleTests` — `PickScale` 外部效果：AxisChange 後 Min/Max 自動範圍（4 個）[有意義]
- [x] **T0.3.8** `PointPairListEditTests` — Add/Insert/RemoveAt/indexer/Clone（8 個，鎖定 Add 拷貝但 Insert 不拷貝的不一致）[有意義]
- [x] **T0.3.9** `DOTNET1ComparerTests` — `CurveItem.Comparer` 泛型版（6 個，守護刪除 DOTNET1 死碼）[有意義]

### 純函式模組（測試缺口，順手補）

- [x] **T0.3.10** `XDateConversionTests` — 雙向轉換 / DayOfWeek / AddDays（10 個）[有意義]
- [x] **T0.3.11** `ValueHandlerTests` — `ValueHandler.GetValues`（4 個，鎖定防護契約；原計畫誤載 Average/Cum/High 已修正為實際 API）[有意義]

---

## T0.4 — 完成檢查

- [x] **T0.4.1** `dotnet test` 全綠（**80/80 通過**，無 skipped / failing）
- [x] **T0.4.2** 覆蓋率記錄：命令列 coverlet 不相容 net48+Exe，改記錄 method 覆蓋清單（見下表）
- [x] **T0.4.3** Batch 1–4 解鎖條件就緒：T0.2（Critical 已修）+ T0.3.1–3.11（重構目標皆有對應測試）

---

## 覆蓋率記錄

| 檢查點 | 日期 | line-rate | branch-rate | 備註 |
|--------|------|-----------|-------------|------|
| 基準（T0.1.3 後） | 2026-06-30 | 9.56% | 5.9% | 11 個測試（FineCodeCoverage） |
| Batch 1 後 | 2026-06-30 | 9.96% | 5.99% | 36 個測試 |
| **Batch 0 完成** | 2026-06-30 | _待 VS 內 FCC 測量_ | _待測_ | **80 個測試**；目標 ≥ 15%（預估已達標，待確認） |

### Method 覆蓋清單（fallback：重構目標皆有對應測試）

| 批次 | 待重構 method | 對應測試 | 等級 |
|------|--------------|----------|------|
| B4 | Scale.Transform / ReverseTransform | ScaleTransformTests | sanity |
| B4 | Symbol.Draw / MakePath | GdiCharacterizationSanityTests | sanity |
| B4 | Line.Draw / BuildPointsArray | GdiCharacterizationSanityTests | sanity |
| B4 | CurveList.SortOverlay | GdiCharacterizationSanityTests | sanity |
| B4 | FontSpec.BoundingBox | GdiCharacterizationSanityTests | 有意義 |
| B3 | Scale.MyMod | ScaleMyModTests | 有意義 |
| B3 | LinearScale.PickScale | LinearScalePickScaleTests | 有意義 |
| B3 | PointPairList 編輯 | PointPairListEditTests | 有意義 |
| B3 | CurveItem.Comparer（刪 DOTNET1） | DOTNET1ComparerTests | 有意義 |
| — | XDate 雙向轉換 | XDateConversionTests | 有意義 |
| — | ValueHandler.GetValues | ValueHandlerTests | 有意義 |

> **結論**：Batch 0 安全網已就緒。所有 Batch 2–4 欲觸碰的 method 皆有 characterization test 護航，可開始重構。
