# BÁO CÁO TIẾN ĐỘ & BÀN GIAO DỰ ÁN BLOCK MERGE PUZZLE

> **Cập nhật lần cuối:** 25/11/2026 | **Unity:** `6000.0.74f1` (`WindowsEditor` · Build target: `Android`) | **Thư mục dự án:** `D:\BlockMergePuzzle`

---

## 1. Trạng Thái Hiện Tại (`Level 1` – `Level 19`, `HomeScene`, `FreshStart`, 3 Booster Hoàn Chỉnh)

- **Tổng quan tiến độ**:
  - Đã xây dựng và đóng gói **19/19 màn chơi (`Level 1` đến `Level 19`)** gồm `Assets/Scenes/Level{1..19}.unity`, `Assets/Prefabs/Levels/Level{1..19}_Board.prefab` và `Assets/Resources/Levels/level_{1..19}.json`.
  - Đã hoàn thiện màn hình chính **[HomeScene.unity](file:///D:/BlockMergePuzzle/Assets/Scenes/HomeScene.unity)** (`HomeUI.cs`) và scene reset chơi lại từ đầu **[FreshStart.unity](file:///D:/BlockMergePuzzle/Assets/Scenes/FreshStart.unity)** (`FreshGameLauncher.cs`).
  - Đã hoàn thiện đầy đủ **3 Booster** theo đúng thứ tự mở khóa **Level 8 / Level 12 / Level 16**, giữ nguyên hình dáng sprite gốc (`BG booster.png`, `booster1..3.png`), có tutorial spotlight ở màn mở khóa và đầy đủ VFX/SFX:
    - **Booster 1 — Freeze (`Level 8`, `FreezeBoosterEffectController.cs`)**: Đóng băng thời gian `15s` với hiệu ứng bay icon, va chạm đồng hồ, viền sương băng & tuyết rơi.
    - **Booster 2 — Magnet (`Level 12`, `MagnetBoosterController.cs`)**: Làm tối vùng không thể thao tác (không thể ấn vùng tối), hút nhảy đập merge (`JumpAndSlamMerge`), bay thoát cổng (`JumpToGate`), hoặc gõ `-1` băng ngay lượt chạm đầu.
    - **Booster 3 — Magic Wand (`Level 16`, `WandBoosterController.cs`)**: Chọn 1 khối block bất kỳ, đũa phép bay thi triển phép + sóng năng lượng + mưa sao và phóng **toàn bộ các khối cùng màu (bất kể hình dạng, số hay lớp băng)** tự động thoát qua cổng cùng màu.
  - Đã tắt toàn bộ banner chữ đỏ báo lỗi (`FeedbackBanner` / `ShowFeedbackError`) trong `GameUI.cs`.
  - Đã tích hợp bộ UI **Win / Lose / TimeOut / Revive / Remove Ads / Shop / Collection** bằng asset Figma (`WinUI`, `LoseUI`, `RemoveAdsUI`, `HomeUI`).
  - Đã bổ sung công cụ **Level Editor & Importer** (`LevelEditorWindow.cs`, `CustomLevelSceneBaker.cs`, `ProceduralBoardRenderer.cs`, `Tools/LevelEditor.html`).

### Danh Sách Chi Tiết 10 Màn Chơi Thủ Công Đã Chốt (`Level 1` – `Level 10`)

1. **Level 1 (`map_lv1.png` — Tutorial T-Shape Map — ĐÃ CHỐT)**:
   - Grid 5×3 (7 ô), 1 cổng trên Đỏ `[2]` tại `(2,0)`, 2 khối `1x1` Đỏ `[1]`. Kích hoạt `TutorialHandGuide` 2 pha (`Merge` -> `ExitGate`), timer `05:00`.
2. **Level 2 (`map_lv2.png` — Asymmetric Cross Map — ĐÃ CHỐT)**:
   - Grid 6×6 (11 ô), 2 cổng (Trên Xanh lá `[2]`, Phải Đỏ `[3]`), 5 khối `1x1`.
3. **Level 3 (`Map_lv3.png` — Branching Map — ĐÃ CHỐT)**:
   - Grid 5×7 (15 ô, 3 nhánh), 2 cổng (Trái Xanh lá `[4]`, Phải Đỏ `[4]`), 8 khối `1x1`.
4. **Level 4 (`map_lv4.png` — E-Shape Map — ĐÃ CHỐT)**:
   - Grid 4×5 (14 ô, hình chữ E), 3 cổng hướng phải (Đỏ `[3]`, Xanh lá `[3]`, Hồng `[2]`), 7 khối `1x1` (kiểm chứng BFS 22 bước).
5. **Level 5 (`map_lv5.png` — Diamond Cross Multi-Shape Map — ĐÃ CHỐT)**:
   - Grid 8×11 (40 ô + 4 ô cổng), 2 cổng rộng 2 ô (Trên Vàng `[5]`, Dưới Xanh lá `[2]`), 17 khối (`2x1`, `1x2`, `L`, `2x2`).
6. **Level 6 (`map_lv6 (1).png` — Cross Piece & 4-Gate Asymmetric Map — ĐÃ CHỐT)**:
   - Grid 7×8 (26 ô + 8 ô cổng), 4 cổng, 10 khối đa hình (bao gồm chữ thập `Cross_3x3`).
7. **Level 7 (`map_lv7.png` — Pyramid Stepped Map + Ice Mechanic — ĐÃ CHỐT)**:
   - Grid 8×4 (12 ô + 4 ô cổng), 3 cổng yêu cầu `[2]`, 5 khối (gồm khối Xanh lá `2x2` **đóng băng `IceCount = 2`**).
8. **Level 8 (`map_lv8.png` — Asymmetric Inner-Obstacle Map + Dual Ice + Mở Booster Freeze — ĐÃ CHỐT)**:
   - Grid 8×9 với 2 lỗ chướng ngại `(3,3)` & `(4,4)`, 5 cổng `[2]`, 15 khối (2 khối `2x2` **đóng băng `IceCount = 4`**).
   - **Mốc mở khóa Booster #1 (Freeze — Level 8)**: Hiện `FreezeTutorialOverlay` lần đầu khi vào màn.
9. **Level 9 (`map_lv9.png` — Vertical 5-Gate Map, 2D Restored — ĐÃ CHỐT)**:
   - Grid 5×8 (khôi phục 2D chuẩn), 5 cổng, 12 khối (gồm khối `2x2` đóng băng `IceCount = 4`).
10. **Level 10 (`map_lv10 (2).png` — Christmas-Tree Multi-Shape Map — ĐÃ CHỐT)**:
    - Grid 9×11, 4 cổng (`ApplyLevel10GateVisualScale`), 26 khối đa hình dạng.

### Danh Sách 9 Màn Chơi Mở Rộng (`Level 11` – `Level 19` via JSON & Baker)

Mỗi level lưu dữ liệu cấu trúc trong `Assets/Resources/Levels/level_{11..19}.json` và được bake thành scene/prefab riêng (`Assets/Scenes/Level{11..19}.unity`, `Assets/Prefabs/Levels/Level{11..19}_Board.prefab`):

| Level | Lưới | Số Cổng | Số Khối | Khối Băng (`Ice`) | Thời Gian | Ghi Chú / Mốc Tính Năng |
|---|---|---|---|---|---|---|
| **Level 11** | `8×10` | 4 | 12 | 0 | `120s` | Bàn cờ custom procedural tường bo góc |
| **Level 12** | `10×12` | 2 | 4 | 0 | `120s` | **Mở khóa Booster #2 (Magnet)** + Tutorial spotlight |
| **Level 13** | `8×11` | 7 | 16 | 1 | `500s` | Nhiều cổng xung quanh kết hợp khối băng |
| **Level 14** | `10×12` | 5 | 18 | 0 | `120s` | Mật độ khối cao, đa hình dạng |
| **Level 15** | `10×11` | 4 | 15 | 2 | `180s` | 2 khối đóng băng cần giải phóng dần qua cổng |
| **Level 16** | `10×12` | 4 | 12 | 0 | `120s` | **Mở khóa Booster #3 (Magic Wand)** + Tutorial spotlight |
| **Level 17** | `10×12` | 5 | 19 | 1 | `120s` | Màn nâng cao 19 khối + 1 khối băng |
| **Level 18** | `10×12` | 3 | 11 | 2 | `120s` | 3 cổng, 2 khối đóng băng |
| **Level 19** | `10×12` | 2 | 17 | 0 | `120s` | 2 cổng, nhiều khối đa hình cần sắp xếp merge |

### Build Settings Scenes (`ProjectSettings/EditorBuildSettings.asset` — 22 Scenes)
- `[0] Assets/Scenes/FreshStart.unity` *(reset tiến trình về mới khi build/chạy)*
- `[1] Assets/Scenes/HomeScene.unity`
- `[2] Assets/Scenes/MainScene.unity`
- `[3]..[13] Assets/Scenes/Level1.unity` .. `Assets/Scenes/Level11.unity`
- `[14] Assets/Scenes/Level13.unity`
- `[15] Assets/Scenes/Level15.unity`
- `[16] Assets/Scenes/Level14.unity`
- `[17] Assets/Scenes/Level17.unity`
- `[18] Assets/Scenes/Level19.unity`
- `[19] Assets/Scenes/Level18.unity`
- `[20] Assets/Scenes/Level12.unity`
- `[21] Assets/Scenes/Level16.unity`

### Lưu Trữ, Git & Kết Nối MCP
- **Git Repository**: `https://github.com/huy113-dev/BlockMergePuzzle.git` — nhánh hiện tại: `feat/level-4-map`.
- **Unity MCP**: FastMCP HTTP bridge (`http://127.0.0.1:8080/mcp`), tự khởi động lại sau domain reload qua `AutoMcpStarter` trong `SceneSetupHelper.cs`.

---

## 2. Các Hệ Thống & Tính Năng Đã Hoàn Thiện

### a. Kéo Thả, Merge Slam, Gate Escape, Khối Đá & Khóa Trục (`PlayerPieceController.cs`, `LevelManager.cs`, `PuzzlePiece.cs`)
- **Trượt đa hướng (`_currentOrigin`) & bàn phím `WASD`/mũi tên**.
- **Chướng ngại nâng cao**:
  - `PuzzlePiece.IsStone`: Khối đá cố định không thể di chuyển, bị bỏ qua khi tính `RemainingBlocksCount`, không thể chọn bằng Magnet/Magic Wand.
  - `PuzzlePiece.MoveAxisConstraint`: Khóa chỉ trượt ngang (`HorizontalOnly`) hoặc chỉ trượt dọc (`VerticalOnly`) kèm mũi tên `AxisOverlayRoot`.
- **Hợp nhất & Thoát cổng**: `SlideAndSlamMerge` + `GUI/Text Shader` white flash + `Custom/SpritePortalDissolve` shader + `EscapeVfxRunner`.
- **Loại bỏ chữ đỏ báo lỗi**: `GameUI.ShowFeedbackError()` đã vô hiệu hóa hiển thị `FeedbackBanner` chữ đỏ khi kéo sai màu/số/hình dạng hoặc đụng khối băng/đá.

### b. Hệ Thống 3 Booster Đầy Đủ (`Freeze`, `Magnet`, `Magic Wand`)
Thứ tự mở khóa trong `GameUI.cs`: `BoosterFreezeUnlockLevel = 8`, `BoosterMagnetUnlockLevel = 12`, `BoosterWandUnlockLevel = 16`. Tất cả đều bảo toàn nguyên sprite nút gốc (`booster1`, `booster2`, `booster3` và nền `BG booster.png`) và đồng bộ chặn xung đột chéo giữa 3 booster & kéo thả khối.

1. **Booster 1 — Freeze (`FreezeBoosterEffectController.cs`)**:
   - Tutorial lần đầu tại Level 8 (`PrefFreezeTutorialSeenKey = "BMP_FreezeTutorialSeen"`, đảm bảo tối thiểu 3 lượt).
   - Khi kích hoạt: icon `booster1` bay vòng cung đập vào `Banner_Time`, rung nhẹ màn hình, đóng băng timer **`15s`** (`FreezeBoosterDuration = 15f`), bật viền sương giá toàn màn hình (`GetFreezeScreenBorderSprite`) và 22 bông tuyết bay xung quanh, tự tan chảy mượt khi hết `15s`.
   - Âm thanh: `SoundManager.PlayFreezeTime()`.

2. **Booster 2 — Magnet (`MagnetBoosterController.cs`)**:
   - Tutorial lần đầu tại Level 12 (`PrefMagnetTutorialSeenKey = "BMP_MagnetTutorialSeen"`, đảm bảo tối thiểu 3 lượt).
   - Pha `SelectFirst`: Làm tối nền/khối không hợp lệ bằng `MagnetBoardDimOverlay` (`sortingOrder = 40`), nâng `sortingOrder` (+42) cho các khối có thể kéo merge/thoát cổng hoặc đang đóng băng — **vùng tối hoàn toàn không thể ấn**.
   - Nếu lượt ấn đầu tiên chọn **khối đóng băng (`IsFrozen`)**: tiêu tốn 1 lượt Magnet, icon `booster2` bay giáng xuống trừ ngay **`-1` băng** (`DecrementIceCount`) + `CoMagnetCrackIce`.
   - Pha `SelectTarget`: Giữ sáng + nâng badge nam châm trên khối nguồn, làm tối toàn bộ mọi thứ ngoại trừ các **khối cùng dạng/màu/số có thể merge** và **cổng đủ điều kiện thoát (`CanPieceMagnetEscapeGate`)** — vùng tối không thể ấn.
   - Khi chọn khối đích: chạy `ExecuteMagnetJumpMerge` -> `PuzzlePiece.JumpAndSlamMerge`. Khi chọn cổng đích: chạy `ExecuteMagnetJumpEscape` -> `PuzzlePiece.JumpToGate` -> `PlayEscapeAnimation`.
   - Âm thanh: `SoundManager.PlayMagnetPull()`.

3. **Booster 3 — Magic Wand (`WandBoosterController.cs`)**:
   - Tutorial lần đầu tại Level 16 (`PrefWandTutorialSeenKey = "BMP_WandTutorialSeen"`, đảm bảo tối thiểu 3 lượt).
   - Pha `SelectColor`: Làm tối nền, cổng & khối đá (`WandBoardDimOverlay` ở `sortingOrder = 40`), nâng sáng tất cả khối block hợp lệ (`!IsStone`), hiển thị prompt `"Magic Wand — Choose a Color to Eliminate"`.
   - Khi chọn 1 khối block bất kỳ (theo màu `PieceColor` của khối đó): giữ sáng tất cả các khối cùng màu đó trên bàn cờ, cho icon `booster3` bay cong từ nút booster lên giữa bàn cờ kèm vệt sao lấp lánh, nổ vòng sóng năng lượng + 14 ngôi sao tỏa sáng (`CoMagicShockwave`, `SpawnRadiatingMagicStars`).
   - Tự làm tan băng nếu có khối cùng màu đang đóng băng (`SetIceCount(0)` + `PlayIceCrack`), sắp xếp theo khoảng cách gần cổng nhất và phóng lần lượt (`0.065s` stagger) qua `LevelManager.FindBestGateForColor` + `ExecuteWandJumpEscape` -> `JumpToGate` -> `PlayEscapeAnimation` về cổng cùng màu bất kể hình dạng hay giá trị số của khối.
   - Âm thanh: `SoundManager.PlayWandCast()` + `PlayEscape()`.

### c. Màn Hình `HomeScene` & Scene Reset `FreshStart`
- **`HomeUI.cs` (`Assets/Scenes/HomeScene.unity`)**:
  - Đọc/khởi tạo `PlayerPrefs`: `BMP_CurrentLevel` (mặc định `1`), `BMP_Lives` (`5`), `BMP_TotalCoins` (`356`), `BMP_JustWonLevel`, `BMP_FbRewardClaimed`, `BMP_NoAdsPurchased`.
  - Hỗ trợ 3 tab: **Shop**, **Home** (thang level 4 bậc + nút Play + Facebook +20 xu + Remove Ads Popup), **Collection** (chọn nhanh các level đã mở), và **Settings Panel**.
- **`FreshGameLauncher.cs` (`Assets/Scenes/FreshStart.unity`)**:
  - Mỗi lần scene chạy (`Start()`), đặt `Time.timeScale = 1f`, `CustomLevelRepository.PendingRuntimeLevelNumber = 0`, `SoundManager.Instance?.ResetMergeCombo()`, và xóa sạch toàn bộ 12 key tiến trình + tutorial (`BMP_CurrentLevel`, `BMP_JustWonLevel`, `BMP_TotalCoins`, `BMP_Lives`, `BMP_FbRewardClaimed`, `BMP_NoAdsPurchased`, `BMP_BoosterFreeze`, `BMP_BoosterMagnet`, `BMP_BoosterWand`, `BMP_FreezeTutorialSeen`, `BMP_MagnetTutorialSeen`, `BMP_WandTutorialSeen`, `BlockMerge_BestScore`), tùy chọn `_resetAudioAndVibrationSettings`, rồi `SceneManager.LoadScene("HomeScene")` (có thể đổi `_startSceneName` sang `"Level1"` trong Inspector nếu muốn vào thẳng màn 1).
  - `GameBootstrapper.AutoBootstrapIfEmpty()` đã thêm guard cho `"FreshStart"` và `FindObj<FreshGameLauncher>() != null` để không tự sinh `LevelManager`/`GameUI` trong khung hình chuyển scene.

### d. Win / Lose / TimeOut / Revive UI & Kinh Tế Trong Game (`GameUI.cs`)
- **Victory Panel**: Đồng bộ bộ sprite `WinUI` (`WELL DONE`, 3 sao, xu bay động `PlayCoinTick`, nhân thưởng xem quảng cáo, cộng dồn xu & tăng `BMP_CurrentLevel` + bật cờ `BMP_JustWonLevel`).
- **TimeOut / Lose / Revive Panel**: Tích hợp bộ sprite `LoseUI`, trừ tim `BMP_Lives` khi thua, tùy chọn hồi sinh / thêm thời gian với SFX `PlayHeartBreak()` và `PlayRevive()`.

### e. Custom Level Pipeline & Level Editor (`LevelEditorWindow.cs`, `Tools/LevelEditor.html`)
- `CustomLevelData` / `CustomLevelRepository`: Đọc/ghi `Assets/Resources/Levels/level_{n}.json`, hỗ trợ Play Test an toàn mà không làm hỏng `BMP_CurrentLevel` / `BMP_TotalCoins` thật của người chơi (backup qua `EditorPrefs`).
- `ProceduralBoardRenderer.cs` & `CustomLevelSceneBaker.cs`: Vẽ lưới bàn cờ & bo tường tự động cho các level mới và bake thành `.unity` + `_Board.prefab`.

---

## 3. Bản Đồ File Quan Trọng

| Đường dẫn | Vai trò | Trạng thái |
|---|---|---|
| `Assets/Scenes/FreshStart.unity` | Scene reset toàn bộ tiến trình game về mới | Hoàn thiện |
| `Assets/Scenes/HomeScene.unity` | Màn hình chính (Home / Shop / Collection / Remove Ads) | Hoàn thiện |
| `Assets/Scenes/MainScene.unity` | Scene dự phòng mặc định | Hoàn thiện |
| `Assets/Scenes/Level1.unity` .. `Level10.unity` | 10 scene thủ công đã chốt | Hoàn thiện (Khóa) |
| `Assets/Scenes/Level11.unity` .. `Level19.unity` | 9 scene mở rộng (gồm Level 12 Magnet & Level 16 Wand) | Hoàn thiện |
| `Assets/Prefabs/Levels/Level1_Board.prefab` .. `Level19_Board.prefab` | Prefab bàn cờ Level 1..19 | Hoàn thiện |
| `Assets/Resources/Levels/level_1.json` .. `level_19.json` | File JSON cấu hình 19 màn chơi | Hoàn thiện |
| `Assets/Scripts/Core/FreshGameLauncher.cs` | Reset `PlayerPrefs` tiến trình & mở `HomeScene` | Hoàn thiện |
| `Assets/Scripts/Core/GameBootstrapper.cs` | Khởi tạo singleton runtime (guard `HomeScene` & `FreshStart`) | Hoàn thiện |
| `Assets/Scripts/Core/CustomLevelData.cs` & `CustomLevelRepository.cs` | Schema JSON & kho lưu trữ màn chơi custom | Hoàn thiện |
| `Assets/Scripts/Core/LevelManager.cs` | Quản lý bàn cờ, merge, escape, băng, đá, Magnet/Wand jump | Hoàn thiện |
| `Assets/Scripts/Gameplay/PlayerPieceController.cs` | Kéo thả đa hướng, bàn phím & route tap Magnet/Wand | Hoàn thiện |
| `Assets/Scripts/Gameplay/PuzzlePiece.cs` | Khối đa hình, băng `IceCount`, đá `IsStone`, `JumpAndSlamMerge`, `JumpToGate` | Hoàn thiện |
| `Assets/Scripts/Gameplay/ExitGateView.cs` | Cổng Tech Hologram & khóa ô cổng | Hoàn thiện |
| `Assets/Scripts/UI/HomeUI.cs` | Quản lý giao diện & luồng `HomeScene` | Hoàn thiện |
| `Assets/Scripts/UI/GameUI.cs` | Header, Timer, BoosterBar (Lv8/12/16), Pause, Victory, TimeOut/Revive | Hoàn thiện |
| `Assets/Scripts/UI/FreezeBoosterEffectController.cs` | Tutorial & VFX Booster 1 (Freeze 15s) | Hoàn thiện |
| `Assets/Scripts/UI/MagnetBoosterController.cs` | Tutorial, Spotlight vùng tối & VFX Booster 2 (Magnet) | Hoàn thiện |
| `Assets/Scripts/UI/WandBoosterController.cs` | Tutorial, Spotlight vùng tối & VFX Booster 3 (Magic Wand) | Hoàn thiện |
| `Assets/Scripts/UI/TutorialHandGuide.cs` | Bàn tay & banner hướng dẫn Level 1 | Hoàn thiện |
| `Assets/Scripts/Audio/SoundManager.cs` | Phát SFX 2D (`DontDestroyOnLoad`), combo merge & procedural audio | Hoàn thiện |
| `Assets/Scripts/Utils/GemSpriteManager.cs`, `SpriteGenerator.cs`, `ProceduralBoardRenderer.cs` | Nạp sprite, sinh texture VFX/băng/tuyết, vẽ bàn cờ procedural | Hoàn thiện |
| `Assets/Scripts/Editor/SceneSetupHelper.cs`, `LevelEditorWindow.cs`, `CustomLevelSceneBaker.cs` | Công cụ Editor, bảo vệ Level 1..10, bake & PlayTest Level 11+ | Hoàn thiện |
| `Tools/LevelEditor.html` | Web tool xếp bản đồ JSON cho `Level Editor & Importer` | Hoàn thiện |

---

## 4. Hướng Dẫn Thao Tác & Kiểm Thử Nhanh

1. **Chơi lại toàn bộ game từ đầu trong Unity Editor**:
   - Trong cửa sổ **Project**, mở `Assets/Scenes/FreshStart.unity` *(double-click để `Hierarchy` hiện `FreshStart`)*.
   - Bấm **Play (▶)** → toàn bộ dữ liệu chơi cũ & tutorial sẽ tự xóa, game tự chuyển vào `HomeScene` ở Level 1 (`5` tim, `356` xu) để trải nghiệm xuyên suốt tới Level 8 (Freeze), Level 12 (Magnet), Level 16 (Magic Wand).
2. **Test nhanh từng Booster trong Edit/Play Mode**:
   - Component `GameUI` trên scene có sẵn 3 ContextMenu tiện ích: `Reset Freeze Booster Tutorial`, `Reset Magnet Booster Tutorial`, `Reset Wand Booster Tutorial`.
3. **Mở rộng / Thiết kế màn chơi mới (`Level 20+`)**:
   - Dùng `Tools/LevelEditor.html` hoặc cửa sổ **Level Editor & Importer** trong Unity (`LevelEditorWindow.cs`) để tạo/sửa JSON trong `Assets/Resources/Levels/` và bake qua `CustomLevelSceneBaker.cs` thành `Level{n}.unity` + `Level{n}_Board.prefab` riêng biệt (tuyệt đối không ghi đè Level 1..10).

---

## 5. Hạng Mục Tiếp Theo (Next Session Roadmap)

1. **Nhạc nền (BGM) & Rung phản hồi (Haptics)**:
   - Kết nối kênh nhạc nền lặp (`BGM`) trong `SoundManager` với công tắc `Music`, và gọi rung phản hồi thiết bị (`Handheld.Vibrate` / plugin Android haptic) với công tắc `Vibration`.
2. **Tích hợp SDK Quảng Cáo & IAP thật**:
   - Nối **Remove Ads**, **x2 Coin sau màn thắng**, và **xem quảng cáo thêm thời gian / thêm tim** vào SDK quảng cáo & thanh toán thực tế khi đóng gói Android.
3. **Mở rộng bộ màn chơi `Level 20+` & Đóng gói bản build Android (`APK / AAB`)**:
   - Thiết kế thêm các màn từ Level 20 trở lên bằng `Level Editor & Importer`, kiểm tra cân bằng độ khó và test build APK trên máy Android thật.
4. **Tiện ích Editor Play-From-FreshStart (tùy chọn)**:
   - Nếu cần bấm Play ở bất kỳ scene nào cũng luôn bắt đầu từ `FreshStart.unity`, có thể bổ sung menu toggle `EditorSceneManager.playModeStartScene` trong `SceneSetupHelper.cs`.
