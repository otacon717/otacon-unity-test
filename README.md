# Unity 曲面道具置放 Demo

以 Unity 6000.3.20f1（URP）製作的桌面互動 Demo：在一個高低起伏的有限範圍曲面上，透過右側道具下拉清單以拖曳方式置放 3D 道具，並可點選已置放的道具開啟互動選單刪除。

## 功能

- **可縮放視窗**：視窗模式可自由縮放，UI 以 Canvas Scaler 依螢幕比例自動適應。
- **程序化曲面**：以多層 Perlin noise 高度場產生的有限範圍曲面（含 MeshCollider）。
- **道具下拉清單**：畫面右側「道具」按鈕，點擊展開/收合六種不同形狀的三維道具（方塊、圓球、圓柱、膠囊、小樹、拱門）。
- **拖曳置放**：從清單拖曳道具會產生半透明預覽跟隨游標；在曲面上放開即於對應位置生成道具。放開時若游標位於 UI 上或不在曲面範圍內，該次置放自動取消。
- **置放指引線**：拖曳期間預覽物懸浮於落點上方，垂直指引線與圓形標記指出道具將掉落的準確 3D 位置。
- **道具互動選單**：點選場景中已置放的道具，會在其旁開啟選單，可刪除該道具或關閉選單。
- **桌面 / VR 雙模式**：同一套遊戲程式碼支援桌面（Windows exe）與 Meta Quest 2 單機 VR（APK）。啟動時 `PlatformSwitcher` 偵測 XR 裝置，自動切換相機（固定相機 ↔ XR Rig）、UI（Overlay ↔ World Space 看板）、輸入模組（`InputSystemUIInputModule` ↔ `XRUIInputModule`）與指向來源（滑鼠射線 ↔ 右手控制器射線）。

## 操作方式

| 操作 | 說明 |
| --- | --- |
| 點擊右上「道具 ▼」 | 展開 / 收合道具清單 |
| 從清單項目往場景拖曳 | 產生預覽並跟隨游標（綠=可置放、紅=不可） |
| 在曲面上放開 | 於指引線標記的落點生成道具 |
| 在 UI 上或曲面外放開 | 取消置放 |
| 點選已置放道具 | 開啟互動選單（刪除 / 關閉） |

## 專案結構

```
Assets/
  Scripts/
    Core/        指向抽象層（IPointerProvider、PlatformSwitcher）與共用定義
    Surface/     程序化曲面（CurvedSurface）
    Props/       道具資料（PropDefinition / PropCatalog / PlacedProp）
    Placement/   拖曳置放與指引線（DragPlacementController / PlacementGuide）
    Interaction/ 場景點選（PropSelectionController）
    UI/          下拉清單與互動選單（PropDropdownPanel / PropContextMenu）
  Editor/        場景與資產產生器、建置腳本（SceneSetup / PropAssetGenerator / BuildScript）
  Prefabs/ Materials/ Data/  由產生器輸出的資產
```

場景與資產皆由 Editor 腳本程式化產生（`Tools > Setup`），流程完全可重現。

## 建置方式

於編輯器選單 `Tools > Build > Windows x64`，或使用命令列：

```
Unity.exe -batchmode -quit -projectPath <專案路徑> -executeMethod BuildScript.BuildWindows
```

輸出於 `Build/Windows/UnityTest.exe`。

## Quest 2 單機版（VR）

### 建置

1. 一次性設定：編輯器選單 `Tools > Setup > Configure Quest Build`
   （啟用 OpenXR 的 Meta Quest Support 與 Oculus Touch Controller Profile，並套用 Android 播放器設定：IL2CPP / ARM64 / minSdk 29 / ASTC）
2. `Tools > Build > Quest APK`，或命令列：

```
Unity.exe -batchmode -quit -projectPath <專案路徑> -executeMethod BuildScript.BuildQuest
```

輸出於 `Build/Android/UnityTest.apk`。

### 安裝到 Quest 2

1. Quest 需開啟開發者模式（手機 Meta Horizon App > 裝置設定 > 開發者模式）
2. USB 連接電腦後：`adb install -r Build/Android/UnityTest.apk`（或使用 SideQuest）
3. 頭顯內於「應用程式庫 > 未知來源」啟動 UnityTest

### VR 操作

| 操作 | 說明 |
| --- | --- |
| 右手射線指向面板 + 扳機 | 點擊「道具」展開 / 收合清單 |
| 扳機按住清單項並拖向曲面 | 產生預覽與指引線，放開扳機即置放 |
| 在面板上或曲面外放開 | 取消置放 |
| 射線指向已置放道具 + 扳機 | 開啟互動選單（跟隨道具、面向頭顯） |
| 射線點選「刪除」 | 刪除該道具 |

## 桌面 / VR 切換設計

- 所有場景指向（置放射線、點選射線）都經由 `PointerService.Current`（`IPointerProvider`）取得：桌面為 `MousePointerProvider`（滑鼠射線），XR 為 `XRRayPointerProvider`（右手控制器射線）。
- `PlatformSwitcher` 於啟動時檢查 `XRSettings.isDeviceActive`，一次完成四件切換：相機（桌面固定相機 ↔ XR Origin rig）、UI 模式（Screen Space Overlay ↔ World Space）、EventSystem 輸入模組（`InputSystemUIInputModule` ↔ `XRUIInputModule`）、指向 provider。
- UI 互動共用同一套 EventSystem 事件流：桌面由滑鼠驅動，VR 由 XRI 的 `TrackedDeviceGraphicRaycaster` + 控制器射線驅動，清單拖曳程式碼完全共用。
- 已置放道具在 VR 模式下自動掛上 `XRSimpleInteractable`，扳機選取即開啟互動選單；桌面則以滑鼠點選判定。
- 桌面版（Standalone）不啟用任何 XR loader，行為與純桌面版完全一致。
