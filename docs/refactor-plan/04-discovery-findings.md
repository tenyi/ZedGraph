# Batch 4 Discovery — Typo / Correctness 候選清單（2026-06-30）

> **任務輸入**：重新做一次 source code 掃描，廣泛找 typo / 正確性 bug 候選，詳列後供使用者裁示修補順序。
> **前置**：Batch 0（測試安全網）已完成；B3-1（StockPt 序列化 typo）已示範修補模式。
> **紀律**：本檔只記錄候選 + 評估，**不**動產品碼。修補需另開 commit 鏈。

---

## ✅ 完工總覽（2026-07-01 更新）

**12 個 actionable 候選全部修畢**，測試 **108 → 115 全綠**（+7）。

| 編號 | commit | 修補內容 | 測試 |
|------|--------|---------|------|
| C2 | ec99952 | HiLowBarItem.GetObjectData 補寫 schema3 鍵 | round-trip ✅ |
| C3 | b78d7c0 | GasGaugeRegion 鍵名 minVal/maxVal → minValue/maxValue | round-trip ✅ |
| C5 | 0da7cdf | PointPair4 鍵 schema2 → schema3 | round-trip ✅ |
| C1 | e1f3bdb | ArrowObj 值來源 schema2 → schema3 | 鍵護衛 ✅ |
| C4 | 78ff659 | PointPairCV 值來源 schema2 → schema3 | 鍵護衛 ✅ |
| C6 | 47913e5 | Axis.CalcCrossFraction 還原 min/max 對調 | reflection ✅（0.75→0.25） |
| C7 | a54bcf7 | Bar.DrawSingleBar 補 SortedOverlay case | fix-only（pos 為內部局部變數，難測） |
| C8 | 7672eb5 | Scale 版本判斷 schema → sch | fix-only（無法從外部區分） |
| C9 | 12ae775 | Legend 版本判斷 schema → sch（兩處） | fix-only |
| C10 | 41b58b8 | JapaneseCandleStick 版本判斷 schema2 → sch | fix-only |
| C11 | e0889ba | PolyObj 版本判斷 schema3 → sch | fix-only |
| C12 | adfd52a | FillType.GradientByY XML 註解 Z→Y | doc-only |

**test commits**：c5d1783（Phase A 紅測試）、5bb50a7（Phase B 護衛測試）、0830d6e（Phase C 紅測試）

### 測試守護策略分層
- **可重現失敗 → regression test**（TDD red→green）：C2/C3/C5（round-trip 拋例外）、C6（frac 值 0.75→0.25）
- **鍵名護衛**（守住「不能改更糟」）：C1/C4（schema3 鍵存在，值來源靠 review）
- **fix-only + code review 守護**：C7（pos 為方法內部局部變數）、C8~C11（schema const vs sch 巧合同值，無法從外部測）
- **doc-only**：C12（純 XML 註解）

### 觀察類（§1.3）不進實作，保留記錄
- O1~O4（風格 / 文件）：不動
- O5/O6（架構級，Fill/FontSpec setter Dispose）：併入 `02` 之 `H1-A 完整版` 藍圖

---

## 0. 掃描方法

| 維度 | 範圍 | 信心度門檻 |
|------|------|----------|
| 序列化對稱性 | 65 個 ISerializable 實作，每對 ctor / GetObjectData 全檢 | 高 / 中 |
| 算術 / 變數 substitution | Scale.cs、Axis.cs、GraphPane.cs、XDate.cs、Line.cs、Bar.cs、Symbol.cs | 中以上 |
| Dispose / 資源所有權 | 全核心 + 已變更的 Fill 派生鏈 | 高 / 中（架構級） |
| 列舉 / 切換 / 常數 | Types.cs + 所有 switch-on-enum | 中以上 |

**信心度定義**：
- **高（功能性 bug）**：可構造 input 重現失敗 → 進入「必修」清單
- **中（語意瑕疵）**：行為巧合正確但 source-level 與兄弟檔不一致 → 進入「與 convention 對齊」清單
- **低（觀察）**：code smell 或設計選擇，需要 maintainer 評估 → 不動，僅記錄

---

## 1. 候選清單總覽

### 1.1 「必看」—— 高信心度 functional bug

這五個**確實**會讓某些 input 走異常路徑或拋例外。**每個都有可寫的 regression test**。

| # | 檔案 | 行 | 問題一句話 | 影響面 |
|---|------|---|------------|-------|
| **C1** | `ArrowObj.cs` | 227 | `AddValue("schema3", schema2)` —— 鍵用自己名、值用父類別 | 序列化源碼正確性（巧合無害） |
| **C2** | `HiLowBarItem.cs` | 142-145 | GetObjectData **完全沒寫** `schema3` 鍵；反序列化會拋 `SerializationException` | **functional**：round-trip 爆炸 |
| **C3** | `GasGaugeRegion.cs` | 121-122 vs 143-144 | 讀 `minValue` / `maxValue`，寫 `minVal` / `maxVal` | **functional**：round-trip 爆炸 |
| **C4** | `PointPairCV.cs` | 100 | 同 C1 模式 | 序列化源碼正確性（巧合無害） |
| **C5** | `PointPair4.cs` | 127 | `AddValue("schema2", schema3)` —— 鍵是父類、值是自己，但子 ctor 期望 `"schema3"` 鍵 | **functional**：子 ctor 拋例外（父+子覆蓋同一 `"schema2"` 鍵） |
| **C6** | `Axis.cs` | 981-982 | `CalcCrossFraction` 的 `min` / `max` 對調（與同檔 `EffectiveCrossValue` 908-909 行相反） | 軸交叉位置**方向反相**（渲染 bug） |
| **C7** | `Bar.cs` | 428-430 | `DrawSingleBar` 漏 `BarType.SortedOverlay` case（與 `DrawBars` 373-375 行不一致） | `SortedOverlay` bar 繪製 pos 不歸零，導致 bar cluster 位置錯亂 |

### 1.2 「與 convention 對齊」—— 中信心度語意瑕疵

這四個**目前**巧合行為正確，但與 codebase 同類別 pattern 不一致。下次有人 bump schema 版本就會爆。

| # | 檔案 | 行 | 問題 | 風險 |
|---|------|---|------|------|
| **C8** | `Scale.cs` | 887 | 條件式用 class const `schema` 而非 local `sch` | 編譯時 11==讀出 11 巧合對齊；bump 版本即壞 |
| **C9** | `Legend.cs` | 507, 510 | 同 C8；第 507 行 `schema >= 11`、第 510 行 `schema >= 12` | 同上 |
| **C10** | `JapaneseCandleStick.cs` | 256 | 用 class const `schema2` 而非 `sch` | 同上 |
| **C11** | `PolyObj.cs` | 203 | 用 class const `schema3` 而非 `sch` | 同上 |
| **C12** | `Fill.cs` enum XML 註解 | Types.cs:118-132 | `GradientByY` XML doc 描述為「based on the Z value of the data」（應為 Y），doc-only copy-paste | 文件錯誤，不影響執行 |

### 1.3 「觀察類」—— 低信心度，不進實作

| # | 檔案 | 行 | 觀察 | 評估 |
|---|------|---|------|------|
| O1 | `MajorTic.cs` | 119, 133, 148 | 本檔用 `schema2` 鍵/值（同層次兄弟如 `MinorTic` 用 `schema`，命名不一致） | 無功能影響；屬風格 |
| O2 | `OHLCBarItem.cs` | 188, 201 | 鍵 `"stick"` 對應欄位 `_bar`，name/value 不一致 | 無功能影響 |
| O3 | `ErrorBarItem.cs` | 196, 211 | 讀端 dead-read `barBase`、寫端固定寫 `BarBase.X` | 向後相容刻意設計 |
| O4 | `XDate.cs` | 71 | `JulDayMax = 5373483.5` doc 說 9999-12-31，但數值對應 12-30（差 1） | 數值與 doc 哪個錯需另行評估 |
| O5 | `FontSpec.cs` | 372-386 | `Fill` / `Border` setter 重複指派舊 instance 未 Dispose（與 H1 已修的 `Fill.Brush` setter 同型） | 架構級，不在 typo 範圍 |
| O6 | `Link.cs` | 233-244 | `MakeCurveItemUrl` 對 `null _url` 會 NRE | 接 null 接得來的 NRE 路徑 |
| O7 | `SamplePointList.cs` | 132 | `timeDiff <= 0` 邊界判斷（瞬間平均速度） | 屬合理設計 |

### 1.4 「架構級」—— 已存於其他批次

| 觀察 | 來源 | 去處 |
|------|------|------|
| `Fill` IDisposable 之後無 composed owner 接 Dispose 鏈 | Agent-3 #4 | 併入 02 `H1-A 完整版` 藍圖 |
| `FontSpec` setter 漏 Dispose | Agent-3 #19 | 併入 `H1-A 完整版` |
| `ImageObj.Image` setter 漏 Dispose | Agent-3 #13/20 | 併入 `H1-A 完整版` |
| `ZedGraphControl.Dispose` 不 dispose `_masterPane` | Agent-3 #5 | 併入 `H1-A 完整版` |

---

## 2. 每個 high-confidence 候選的詳細評估

### C1 — `ArrowObj.cs:227` 序列化 schema 鍵/值來源錯置

#### 源碼
**`source/ZedGraph/ArrowObj.cs:199`**：`public const int schema3 = 10;`
**`source/ZedGraph/ArrowObj.cs:213`**（讀端）：`int sch = info.GetInt32( "schema3" );`
**`source/ZedGraph/ArrowObj.cs:227`**（寫端）：
```csharp
public override void GetObjectData( SerializationInfo info, StreamingContext context )
{
    base.GetObjectData( info, context );
    info.AddValue( "schema3", schema2 );   // ← 寫 schema3 鍵但值用父類 GraphObj.schema2
    ...
}
```

#### 評估
- 讀端期望 `"schema3"` 鍵（合理），值也期望是 `ArrowObj.schema3 = 10` 的源頭。
- 寫端卻用 `schema2`（父類 `GraphObj` 的常數，值也是 10）。**巧合相同所以 round-trip 無害**。
- 與 B3-1 修復的 StockPt 同型：作者複製父類 `GetObjectData` 後改鍵但忘記換值來源。

#### 與 B3-1 差異
| 項目 | StockPt (B3-1 已修) | ArrowObj (C1) |
|------|---------------------|---------------|
| 鍵 | "schema3" | "schema3" |
| 當前寫的值 | PointPair.schema2 (11) | GraphObj.schema2 (10) |
| 自身常數 | schema3 = 11 | schema3 = 10 |
| 巧合無害？ | ✅ 11==11 | ✅ 10==10 |
| 影響行為？ | ❌ 無 | ❌ 無 |

#### 修補方向
```csharp
info.AddValue( "schema3", schema3 );  // 與讀端鍵對應、與自身常數源頭
```
附加註解說明「修 B6-1 將 schema2 → schema3 統一值來源」。

#### 估計
- 1 行產品碼 + 1 行註解 + 1 個 round-trip regression test
- 2 ~ 3 commit（同 B3-1 鏈：test → fix → plan）

---

### C2 — `HiLowBarItem.cs:142-145` GetObjectData 完全漏寫 schema3

#### 源碼
**`source/ZedGraph\HiLowBarItem.cs:121`**：`public const int schema3 = 11;`
**`source/ZedGraph\HiLowBarItem.cs:130-145`**：
```csharp
protected HiLowBarItem( SerializationInfo info, StreamingContext context ) : base( info, context )
{
    int sch = info.GetInt32( "schema3" );  // ← 讀端期望 "schema3" 鍵
}

public override void GetObjectData( SerializationInfo info, StreamingContext context )
{
    base.GetObjectData( info, context );
    // ← 寫端沒有 info.AddValue( "schema3", ... )！
}
```

#### 評估
- **這是真正的 functional bug**：`base.GetObjectData`（繼承自 OHLCBarItem、CurveItem）會呼叫祖父輩的 `info.AddValue("schema", ...)`，但**沒有任何地方寫 `"schema3"` 這個鍵**。
- 反序列化端 `info.GetInt32( "schema3" )` 拿不到，**會拋 `SerializationException`**。
- 影響：實際序列化檔包含 `HiLowBarItem` 之後 deserialize 失敗。
- 對照 `StickItem.cs:208-212`（同層次兄弟 CurveItem 子類），那是正確寫了的。

#### 為何之前測試沒抓到？
- 既有 108 個測試**沒有針對 HiLowBarItem 的 round-trip**。
- 寫 round-trip 測試即 red → 修補為 green 的標準 TDD 路徑。

#### 修補方向
```csharp
public override void GetObjectData( SerializationInfo info, StreamingContext context )
{
    base.GetObjectData( info, context );
    info.AddValue( "schema3", schema3 );  // B6-3 補：對應 ctor 第 134 行讀端
}
```

#### 估計
- 1 行產品碼 + 1 行註解 + 1 個 round-trip regression test
- 估 TDD red→green 1 鏈 3 commit

---

### C3 — `GasGaugeRegion.cs:121-122 vs 143-144` 鍵名不一致

#### 源碼
**`source/ZedGraph\GasGaugeRegion.cs:121-122`**（讀端）：
```csharp
_minValue = info.GetDouble( "minValue" );
_maxValue = info.GetDouble( "maxValue" );
```
**`source/ZedGraph\GasGaugeRegion.cs:143-144`**（寫端）：
```csharp
info.AddValue( "minVal", _minValue );
info.AddValue( "maxVal", _maxValue );
```

#### 評估
- **functional bug**：寫端用 `minVal` / `maxVal`，讀端用 `minValue` / `maxValue`。Round-trip 還原時 `_minValue` / `_maxValue` 取不到值（會擲 `SerializationException` 或拿到 `0.0` 預設，看 `GetDouble` 行為）。
- 推測：作者在 ctor 用 `minValue` / `maxValue`（正式命名），複製 GetObjectData 時用 `minVal` / `maxVal`（簡寫、與其他 GasGaugeRegion 屬性參數名同調），**漏對齊**。

#### 修補方向選項
- **選項 A**（推薦）：改寫端與讀端對齊，`info.AddValue( "minValue", _minValue );` / `info.AddValue( "maxValue", _maxValue );`
- **選項 B**：改讀端對齊寫端（不建議，因為寫端名稱較簡短但不一致）

#### 估計
- 2 行產品碼 + 1 個 round-trip test（一次序列化後反序列化驗 `_minValue` / `_maxValue` 還原）
- 估 3 commit

---

### C4 — `PointPairCV.cs:100` 同 C1 pattern

#### 源碼
**`source/ZedGraph\PointPairCV.cs:100`**：
```csharp
public override void GetObjectData( SerializationInfo info, StreamingContext context )
{
    base.GetObjectData( info, context );
    info.AddValue( "schema3", schema2 );   // ← 寫 schema3 鍵用 PointPair.schema2 值
    info.AddValue( "ColorValue", ColorValue );
}
```
讀端（第 86 行）期望 `"schema3"` 鍵；類別內部 `public const int schema3 = 11`。

#### 評估
- 完全同 C1 模式，巧合無害（schema2 == 11 == schema3）。
- 影響：僅 source-level 正確性。

#### 修補方向
```csharp
info.AddValue( "schema3", schema3 );
```

#### 估計
- 1 行產品碼 + 1 個 round-trip test
- 估 3 commit

---

### C5 — `PointPair4.cs:127` 鍵名是父類，值是子類

#### 源碼
**`source/ZedGraph\PointPair4.cs:101`**：`public const int schema3 = 11;`
**`source/ZedGraph\PointPair4.cs:114`**（讀端）：`int sch = info.GetInt32( "schema3" );`
**`source/ZedGraph\PointPair4.cs:127`**（寫端）：
```csharp
public override void GetObjectData( SerializationInfo info, StreamingContext context )
{
    base.GetObjectData( info, context );   // ← 父類 PointPair 寫 "schema2" = 11
    info.AddValue( "schema2", schema3 );   // ← 子類覆蓋 "schema2" 鍵（仍是 11）
    info.AddValue( "T", T );
}
```

#### 評估
- 父類 `PointPair.GetObjectData` 寫 `info.AddValue( "schema2", PointPair.schema2 )` → 鍵 `"schema2"` 值 11。
- 子類 `PointPair4.GetObjectData` **覆蓋** `"schema2"` 鍵為 `schema3` 值（巧合也是 11）。所以最終 SerializationInfo 中 `"schema2"` 鍵的值是 11（值未改變、鍵還是 `"schema2"`）。
- 子 ctor 第 114 行：`: base( info, context )` 後呼叫 `info.GetInt32( "schema3" )`——**找不到 `"schema3"` 鍵，會拋 `SerializationException`**。
- **這是真實 functional bug**（與 C2 同型：寫端沒寫到正確的鍵）。

#### 修補方向
```csharp
public override void GetObjectData( SerializationInfo info, StreamingContext context )
{
    base.GetObjectData( info, context );
    info.AddValue( "schema3", schema3 );   // B6-5 修：鍵/值都為 schema3，對應子 ctor 第 114 行讀端
    info.AddValue( "T", T );
}
```

#### 估計
- 1 行產品碼 + 1 個 round-trip test
- 估 3 commit

---

### C6 — `Axis.cs:981-982` `CalcCrossFraction` min/max 對調

#### 源碼
**`source\ZedGraph\Axis.cs:908-909`**（同檔的 `EffectiveCrossValue`，正確）：
```csharp
double min = crossAxis._scale.Linearize( crossAxis._scale._min );
double max = crossAxis._scale.Linearize( crossAxis._scale._max );
```

**`source\ZedGraph\Axis.cs:981-982`**（`CalcCrossFraction`，可疑）：
```csharp
double max = crossAxis._scale.Linearize( crossAxis._scale._min );  // ← 變數叫 max 但值來自 _min
double min = crossAxis._scale.Linearize( crossAxis._scale._max );  // ← 變數叫 min 但值來自 _max
```

#### 後續使用
```csharp
frac = (float)( ( effCross - min ) / ( max - min ) );  // 兩條路徑都用到 min/max
```

#### 評估
- `min` / `max` 變數被對調，使 `(max - min)` 取負值（除以負值），`frac` 為負。
- 後續 `if (frac < 0.0f) frac = 0.0f;` 會把它夾回 0。
- 在 isLabelsInside 條件不同分支，行為可能從「正位」變「靠邊」（0 or 1），具體視 axis type 而定。
- **可寫 regression test**：固定 `_scale._min` / `_scale._max` / `effCross`，驗 `frac` 落在 `[0, 1]` 區間內**且**與 `EffectiveCrossValue` 線性關係一致。
- 對調修復後會使 frac 在對稱情況下表現正常。

#### 為何之前沒抓到？
- 既有測試**沒單元測 `CalcCrossFraction`**，因為它是 `internal` 方法，需 reflection 或 InternalsVisibleTo。

#### 修補方向
```csharp
double max = crossAxis._scale.Linearize( crossAxis._scale._max );  // B6-6 修：值來自 _max
double min = crossAxis._scale.Linearize( crossAxis._scale._min );  // B6-6 修：值來自 _min
```

#### 估計
- 2 行產品碼（含一行註解）+ 1 個 reflection-based regression test
- 估 3 commit

---

### C7 — `Bar.cs:428-430` `DrawSingleBar` 漏 `SortedOverlay` case

#### 源碼
**`source\ZedGraph\Bar.cs:372-375`**（`DrawBars`，正確全列）：
```csharp
BarType barType = pane._barSettings.Type;
if ( barType == BarType.Overlay || barType == BarType.Stack || barType == BarType.PercentStack ||
        barType == BarType.SortedOverlay )
    pos = 0;
```

**`source\ZedGraph\Bar.cs:428-430`**（`DrawSingleBar`，漏 `SortedOverlay`）：
```csharp
if ( pane._barSettings.Type == BarType.Overlay || pane._barSettings.Type == BarType.Stack ||
        pane._barSettings.Type == BarType.PercentStack )
    pos = 0;
```

#### 評估
- `DrawSingleBar` 的設計意圖：「For Overlay and Stack bars, the position is always zero since the bars are on top of eachother」。
- `DrawBars` 的設計意圖類似，但**多列了 `SortedOverlay`**。
- `DrawSingleBar` 的 XML doc（第 386 行）：「It is intended to be used only for `BarType.SortedOverlay`, which requires special handling of each bar.」—— **這就是 SortedOverlay 走 DrawSingleBar 的場景！** 所以漏它是 bug。
- 影響：在 `SortedOverlay` 模式下 `DrawSingleBar` 不歸零 `pos` → bar cluster 位置錯亂。

#### 修補方向
```csharp
if ( pane._barSettings.Type == BarType.Overlay || pane._barSettings.Type == BarType.Stack ||
        pane._barSettings.Type == BarType.PercentStack ||
        pane._barSettings.Type == BarType.SortedOverlay )   // B6-7 修：補 SortedOverlay case
    pos = 0;
```

#### 可測性
- `DrawSingleBar` 是 public 方法、輸入為 `Graphics`、`GraphPane`、`CurveItem`、`index`——需要實際 `Graphics` 物件才能測繪製行為。**實務上難以表單化驗證「pos 是否歸零」**。
- 退階：可寫「行為一致性」的 characterization 測試——同樣 BarType 設定下，DrawBars 和 DrawSingleBar 對 `pos` 的處理應一致。但這需 reflection 或 internal helper 揭露 pos 影響。
- **或**：靠 code review 守護（與 `DrawBars` 對齊的常規）。

#### 估計
- 1 行產品碼（含註解）+ 0 ~ 1 個 regression test（可能改用 documentation test）
- 估 2 commit（fix + plan）

---

### C8 ~ C11 — `schema` 常數 vs `sch` 區域變數不一致

#### 共用模式（4 處）
讀端宣告 local `sch`（從 SerializationInfo 讀出的版本），但用於條件分支時卻用 class const：
| 檔案 | schema const | 寫該值的寫端 | 條件分支 | 期望 |
|------|--------------|--------------|----------|------|
| `Scale.cs` | `Scale.schema = 11` (L887) | Scale 自己 `AddValue("schema", schema)` (L) | `if (schema >= 11)` | **應為 sch** |
| `Legend.cs` | `Legend.schema = 12` (L507, L510) | Legend 自己 | `if (schema >= 11)` / `if (schema >= 12)` | **應為 sch** |
| `JapaneseCandleStick.cs` | `JapaneseCandleStick.schema2 = 11` (L256) | JCS | `if (schema2 >= 11)` | **應為 sch** |
| `PolyObj.cs` | `PolyObj.schema3 = 11` (L203) | PolyObj | `if (schema3 >= 11)` | **應為 sch** |

#### 對照 CurveItem.cs:293（正確）
```csharp
int sch = info.GetInt32( "schema" );
...
if ( sch >= 11 )   // ← 用 local sch
```

#### 評估
- **目前巧合正確**：所有 `schema` 常數值都等於「現行 stream schema 值」（因為讀寫兩端同步）。
- **下次 bump 版本即爆**：若有人把 `Legend.schema` 從 12 升到 13，但讀端繼續用 `schema >= 11`/`>= 12`，就會行為漂移。**改用 `sch`（讀出值）才能正確做版本兼容判斷**。
- 影響：維護風險，目前零功能 bug。

#### 修補方向
- 4 個檔案，把 `schema` / `schema2` / `schema3` 替換為 `sch`。
- 每個檔案 1 ~ 2 行變更。

#### 可測性
- 難寫：「故意構造舊版本 stream」才能驗。但**可寫 round-trip 守住「`sch >= 11` 邏輯路徑仍走得通」**。
- 實務上靠 review 守護更容易。

#### 估計
- 4 處各 1 ~ 2 行 = 4 ~ 8 行產品碼
- 可選：1 個 combined round-trip 測試
- 估 2 ~ 3 commit

---

### C12 — `FillType.GradientByY` XML 註解 copy-paste 錯

#### 源碼
**`source\ZedGraph\Types.cs`**：`FillType.GradientByY` 與 `FillType.GradientByZ` 兩個 enum 值的 XML doc 都寫「based on the Z value of the data」。

#### 評估
- doc-only bug，不影響執行。
- 修復簡單：把 `GradientByY` 該行 doc 改成「based on the Y value of the data」。

#### 可測性
- 無：純文件。
- 修復靠 review。

#### 估計
- 1 行 XML 文字 + 0 commit（可併入 Batch 5 「文件 / 註解統一」批次）

---

## 3. 觀察類（不進實作）

詳見 §1.3。**O5 / O6 屬架構級**，併入 Batch 2 `H1-A 完整版` 藍圖。

### O4 — `XDate.cs:71` `JulDayMax` 文/值差 1 的評估

#### 源碼
**`source\ZedGraph\XDate.cs:71`**：`public const double JulDayMax = 5373483.5;` ← 對應 Julian Day 5373483.5 = 9999-12-30 12:00 TT
**`source\ZedGraph\XDate.cs`** 周圍 doc string 說「9999-12-31」。

#### 真實數值
- Excel XL date 2958465 對應 9999-12-31 → Julian Day 約 5373484.0
- 9999-12-30 對應 Julian Day 5373483.5

#### 評估
- **不是 typo**，是 doc 寫得多 1 天。屬小文件瑕疵。
- 不影響功能（`JulDayMax` 是上界檢查值）。

---

## 4. 修補順序建議（按 risk × 變動面）

| 順序 | 編號 | 變動面 | 信心 | 為什麼這個順序 |
|------|------|--------|------|----------------|
| ① | **C2** | 1 行 + 1 個 round-trip test | 高（functional） | 真實 round-trip bug，影響實際序列化檔 |
| ② | **C3** | 2 行 + 1 個 round-trip test | 高（functional） | 同 C2 |
| ③ | **C5** | 1 行 + 1 個 round-trip test | 高（functional） | 同 C2，子 ctor 拋例外 |
| ④ | **C6** | 2 行 + 1 個內部測試 | 高（渲染） | 渲染 bug，可寫 reflection test 守護 |
| ⑤ | **C7** | 1 行（+ 0 個測試或 1 characterization） | 高（渲染） | XML doc 已明指 SortedOverlay → DrawSingleBar，漏它是 copy-paste 漏 |
| ⑥ | **C1 + C4** | 1 行 + 1 個 round-trip test（可合併） | 高（語意） | 同 B3-1 pattern，巧合無害但應修正 |
| ⑦ | **C8~C11** | 4 ~ 8 行 + 0 ~ 1 個測試 | 中（維護風險） | 統一 codebase convention |
| ⑧ | **C12** | 1 行 XML | 文件 | 低風險；可順手修 |

---

## 5. 變動面 × commit 數量估算

| 修補項 | 檔案數 | 預估 commit |
|--------|--------|-------------|
| C1 | 1 | 3（test + fix + plan） |
| C2 | 1 | 3 |
| C3 | 1 | 3 |
| C4 | 1 | 3 |
| C5 | 1 | 3 |
| C6 | 1 | 3 |
| C7 | 1 | 2 |
| C8~C11 | 4 | 3（test + fix + plan 合併） |
| C12 | 1 | 1（修文件） |
| **總計** | ~12 | **~24 commit** |

**整合建議**：
- C1 + C4（同 StockPt pattern）可合成單一 B6「序列化 schema 鍵值對齊」批次：1 個綜合 test 檔 + 多檔 fix + 1 plan → ~5 commit
- C8~C11（schema const → sch）可合 1 個「版本常數對齊」批次 → 3 commit
- C2 + C3 + C5（功能性 bug）保留獨立，因各自需獨立 round-trip 測試

---

## 6. 與其他計劃檔的關係

| 觀察 | 屬 | 處理 |
|------|----|------|
| **C1 ~ C12** | 本檔處理 | 見 §4 順序 |
| O5 / O6 / O7 | 架構級 | 併入 `02-high-safety-resource.md` 之 `H1-A 完整版` 藍圖 |
| O1 / O2 / O3 | 風格 / 觀察 | 不動 |
| O4 | doc | 不動 |

---

## 7. 統計總結

| 維度 | 高 | 中 | 低 | 合計 |
|------|----|----|----|------|
| 序列化 | 3 (C1, C2, C3, C4, C5 = 5) | 4 (C8~C11) | 1 | 10 |
| 算術 / 變數 | 1 (C6) | 0 | 0 | 1 |
| 渲染 / copy-paste | 1 (C7) | 0 | 0 | 1 |
| 文件 / 列舉 | 0 | 1 (C12) | 0 | 1 |
| 觀察（不進實作） | 0 | 0 | 7 | 7 |
| **合計 actionable** | **7** | **5** | **0** | **12** |
| **含觀察合計** | 7 | 5 | 7 | **19** |
