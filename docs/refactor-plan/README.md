# ZedGraph 修正計畫（Refactor Plan）

> 本目錄為依據 2026-06-30 全面程式碼審查結果訂立的分批修正計畫。
> 審查共發現 **34 個問題**：5 Critical / 9 High / 14 Medium / 6 Low。

---

## 一、核心策略：測試先行（Test-First Safety Net）

**絕對原則：先補測試，覆蓋率足夠後才動產品碼。** 任何批次在修改 class/method 之前，
必須先在 Batch 0 建立對應測試。測試分兩種，用途不同：

| 測試類型 | 適用情境 | 撰寫時機 | 修復前是否通過 |
|----------|----------|----------|----------------|
| **Regression Test（回歸/TDD red）** | 修 **明確 Bug**（C1–C5 等） | 修復**之前** | **失敗**（描述「應有」的正確行為） |
| **Characterization Test（特徵化）** | **重構**（效能優化、死碼刪除、現代化） | 重構**之前** | **通過**（鎖定「現有」行為，確保重構不改外部行為） |

> ⚠️ 注意：純 bug 修復不寫 characterization test（那會鎖定錯誤行為）；
> 純重構不寫 failing test（現有行為不是 bug，只是次優）。

---

## 二、執行原則

1. **一批一提交**：每個 checkbox 項目完成後獨立 commit，訊息標註批次與編號（如 `[B1-C2]`）。
2. **每批結束跑全測**：`dotnet test unittest\ZedGraph.UnitTest.csproj`，全綠才進下一批。
3. **不跨批修改**：嚴格依賴順序，避免合併衝突與回歸難以追蹤。
4. **小步前進**：單次修改不超過一個 class/method，便於 review 與 rollback。
5. **未確認即停**（CLAUDE.md Rule 1/4）：行號於實作時須重新核對，發現與計畫不符先討論。

---

## 三、批次總覽與進度

| 批次 | 檔案 | 內容 | 項目數 | 相依 | 狀態 |
|------|------|------|--------|------|------|
| **Batch 0** | [00-tests-foundation.md](./00-tests-foundation.md) | 測試安全網（前置必做） | ~20 | — | `- [ ]` |
| **Batch 1** | [01-critical-fixes.md](./01-critical-fixes.md) | 5 個 Critical bug 修復 | 5 | Batch 0 (T0.2) | `- [ ]` |
| **Batch 2** | 02-high-safety-resource.md | 安全性 + 資源管理（H1,H3,H4,H7–H9, M10,M12–M14, L5） | ~12 | Batch 0 (T0.3) | `- [ ]` *(待產)* |
| **Batch 3** | 03-correctness-cleanup.md | 死碼清理 + 正確性防護（H2,H5, M8,M9,M11） | ~8 | Batch 0 (T0.3) | `- [ ]` *(待產)* |
| **Batch 4** | 04-performance.md | 效能熱路徑（H6, M1–M7） | ~10 | Batch 0 (T0.3) | `- [ ]` *(待產)* |
| **Batch 5** | 05-quality-modernization.md | 品質/現代化/測試衝刺（L1–L4,L6, 現代化, coverage≥30%） | ~12 | Batch 0–4 | `- [ ]` *(待產)* |

> **解鎖關係（細粒度）**：Batch 0 不必整批做完才能動後續。
> `T0.2`（Critical failing tests）完成即可開工 Batch 1；
> `T0.3.x`（各重構目標的 characterization test）完成即可開工對應的 Batch 3/4 子項。

---

## 四、測試覆蓋率目標

| 階段 | 預期覆蓋率（行覆蓋） | 衡量方式 |
|------|----------------------|----------|
| 現況 | < 5% | — |
| Batch 0 完成 | ≥ 15%（覆蓋所有即將修改的 method） | 「修改前必有鎖定該 method 的測試」為**硬性門檻** |
| Batch 1–4 完成 | ≥ 25% | 每個修復/重構伴隨測試 |
| Batch 5 完成 | ≥ 30% | 補 XDate/Scale 全系列/ValueHandler 系統性測試 |

> .NET Framework 4.8 + 非 SDK 風格 csproj，建議用 **Coverlet**（支援 net48）或 Visual Studio 兦建 coverage。
> 整體 % 為參考指標；**硬性門檻是「每個被修改的 method 都有對應測試」**。

---

## 五、測試環境

### 現況（已驗證 2026-06-30）

核心專案 `source\ZedGraph\ZedGraph.csproj` 已升級為 SDK 風格多 TFM（`net48;net6.0-windows;net8.0-windows`，commit 275dbe3）。
原 `unittest\ZedGraph.UnitTest.csproj` 仍是舊 packages.config + net48，**無法編譯**（target 衝突）。

### 雙測試專案策略（已決策）

| 專案 | 狀態 | 框架 | Target | 用途 |
|------|------|------|--------|------|
| `unittest\ZedGraph.UnitTest\` | **保留原樣，不動** | NUnit 2.6.4 | net48 | 歷史 baseline（不接 CI） |
| `unittest\ZedGraph.XUnitTests\` | **新建，已驗證** | xUnit v3.2.2 | net48 | 主要測試基地，與核心對齊 |

### 新測試專案規格

- **套件版本**：`xunit.v3` 3.2.2、`Microsoft.NET.Test.Sdk` 17.12.0、`xunit.runner.visualstudio` 3.1.5、`coverlet.collector` 6.0.2
- **關鍵設定**：`<OutputType>Exe</OutputType>`（xUnit v3 必須為 Exe，非 Library）
- **TFM**：`net48`（對齊核心；computed compatibility 已實測可運作）
- **namespace**：`ZedGraph.XUnitTests`（與產品碼 `ZedGraph` 分開）
- **既有 NUnit 風格對應**：`[TestFixture]` → `[Collection]` 或無；`[Test]` → `[Fact]`；`Assert.That(x, Is.EqualTo(y))` → `Assert.Equal(y, x)`；`AssertWasCalled` → xUnit 慣用 Assert 風格重寫

### 驗證

```powershell
# 建置新測試專案
dotnet build unittest\ZedGraph.XUnitTests\ZedGraph.XUnitTests.csproj

# 執行測試
dotnet test unittest\ZedGraph.XUnitTests\ZedGraph.XUnitTests.csproj

# 含覆蓋率
dotnet test unittest\ZedGraph.XUnitTests\ZedGraph.XUnitTests.csproj --collect:"XPlat Code Coverage"
```

---

## 六、全域驗證命令（PowerShell）

```powershell
# 建置核心
dotnet build source\ZedGraph\ZedGraph.csproj

# 執行單元測試（每批結束必跑）
dotnet test unittest\ZedGraph.UnitTest.csproj

# 建置整個方案（含 Web/Demo）
dotnet build ZedGraph.sln
```

---

## 七、檔案清單

```
docs/refactor-plan/
├── README.md                    # 本檔（主索引）
├── 00-tests-foundation.md       # Batch 0：測試安全網 ✅
├── 01-critical-fixes.md         # Batch 1：Critical 修復 ✅
├── 02-high-safety-resource.md   # Batch 2：安全 + 資源（待產）
├── 03-correctness-cleanup.md    # Batch 3：死碼 + 正確性（待產）
├── 04-performance.md            # Batch 4：效能（待產）
└── 05-quality-modernization.md  # Batch 5：品質/現代化（待產）
```
