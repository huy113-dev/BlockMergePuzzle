# 💎 Block Merge Puzzle — Unity Project

Dự án game giải đố trượt khối đá quý (**Sliding Block Merge & Escape Puzzle**) xây dựng trên **Unity `6000.0.74f1`** (Target: **Android**), tại `D:\BlockMergePuzzle`. Hiện tại dự án đã hoàn thiện **19 màn chơi (`Level 1` – `Level 19`)**, màn hình **`HomeScene`**, scene khởi động lại từ đầu **`FreshStart`**, hệ thống **3 Booster hoàn chỉnh (Freeze / Magnet / Magic Wand)** với tutorial từng mốc mở khóa (**Level 8 / 12 / 16**), vòng lặp **Win / Lose / TimeOut / Revive**, **Level Editor** trực quan (Unity & Web HTML), VFX/Shader và âm thanh 2D.

---

## 🕹️ Cơ Chế Gameplay Chính

1. **Trượt Đa Hướng Linh Hoạt (`Multi-Directional Drag & Snap`)**:
   - Chạm và rê ngón tay/chuột dẫn hướng khối đá quý lướt mượt qua các khúc cua và hành lang mà không cần nhả tay (`_currentOrigin`).
   - Hỗ trợ phím `W/A/S/D` hoặc các phím mũi tên để dịch chuyển từng bước trên bàn cờ.
   - Hỗ trợ khóa trục di chuyển (`MoveAxisConstraint`: `HorizontalOnly`, `VerticalOnly`) và chướng ngại khối đá cố định (`IsStone`).
   - Hiệu ứng **Lift & Tilt**: Nhấc khối nổi lên (`scale 1.075x`, `sortingOrder = 50`), nghiêng nhẹ theo vận tốc kéo ngang (`±3.5°`) và hạ mượt mà khi đặt xuống.

2. **Hợp Nhất Thẳng Hàng & Merge Slam VFX (`Collinear Straight-Line Merge`)**:
   - Hai khối **cùng hình dạng**, **cùng màu sắc**, **cùng giá trị số** và **nằm thẳng hàng/cột** khi va chạm sẽ gộp thành khối mới có giá trị `+1` (`+50 × value` điểm).
   - Hiệu ứng **Merge Slam**: Khối nhấc lơ lửng lao tới, đập squash/stretch, chớp silhouette trắng (`GUI/Text Shader`), bung hào quang sao (`SpawnMergeFlash`) và rung nhẹ camera (`KickCameraImpact`).

3. **Thoát Cổng Chính Xác, Khóa Ô Cổng & Portal Dissolve (`Gate Escape`)**:
   - **Khóa ô cổng (`Gate Cell Blocking`)**: Cổng thoát đóng vai trò tường chắn cứng — khối tuyệt đối không thể dừng hoặc đè lên ô cổng khi chưa đủ điều kiện.
   - **Khớp điều kiện chính xác**: Khối phải **cùng màu**, **đúng hướng cổng**, khớp bề rộng cổng đa ô và có giá trị **bằng đúng `RequiredValue`** (`piece.Value == gate.RequiredValue`).
   - **Portal Dissolve VFX**: Khi thoát cổng, khối trượt vào tâm cổng và tan biến qua shader `Custom/SpritePortalDissolve` kèm hạt vỡ (`+150 × value` điểm). Giải phóng toàn bộ khối (trừ khối đá cố định `IsStone`) để chiến thắng màn chơi.
   - **Không hiện chữ đỏ báo lỗi**: Đã tắt toàn bộ banner cảnh báo chữ đỏ (`FeedbackBanner` / `ShowFeedbackError`) để trải nghiệm chơi mượt mà, sạch sẽ.

4. **Cơ Chế Khối Đóng Băng (`IceCount`)**:
   - Một số khối bị bọc trong lớp băng (`ice_block_overlay.png`) kèm chữ số đếm ngược 3D, ẩn hoàn toàn khối bên trong và không thể di chuyển hay gộp.
   - Mỗi lần một khối khác thoát cổng thành công, giá trị băng giảm đi `1`. Khi về `0`, lớp băng phát nổ thành mảnh băng kèm âm thanh `icecrak.wav` và giải phóng khối bên trong.

---

## ⚡ Hệ Thống 3 Booster Hoàn Chỉnh (Mở Khóa Level 8 / 12 / 16)

Giữ nguyên toàn bộ hình dáng icon & khung nút booster gốc (`BG booster.png`, `booster1.png`, `booster2.png`, `booster3.png`).

| Booster | Mở Khóa | Lượt Mặc Định | Controller | Cơ Chế & Hiệu Ứng |
|---|---|---|---|---|
| **1. Freeze (Đóng Băng)** | **Level 8** | `x3` | `FreezeBoosterEffectController` | Ở Level 8 lần đầu bật tutorial spotlight nút `Booster_Freeze` + bàn tay chỉ dẫn (`UI_hand`). Khi dùng: icon `booster1` bay lên đập vào đồng hồ `Banner_Time`, đóng băng thời gian **`15s`**, phủ viền băng sương toàn màn hình & tuyết rơi xung quanh viền màn hình. |
| **2. Magnet (Nam Châm)** | **Level 12** | `x2` *(3 ở tutorial)* | `MagnetBoosterController` | Ở Level 12 lần đầu bật tutorial spotlight nút `Booster_Magnet` + bàn tay chỉ dẫn. Chế độ ngắm làm tối vùng không hợp lệ (`MagnetBoardDimOverlay`, **vùng tối không thể ấn**). Chạm khối bất kỳ: nếu là **khối băng** thì giáng xuống trừ ngay **`-1` băng**; nếu là khối thường, chạm tiếp khối hợp lệ cùng dạng/màu/số để **nhảy đập merge (`JumpAndSlamMerge`)**, hoặc chạm thẳng **cổng hợp lệ** để **nhảy thoát cổng (`JumpToGate`)**. |
| **3. Magic Wand (Đũa Thần)** | **Level 16** | `x1` *(3 ở tutorial)* | `WandBoosterController` | Ở Level 16 lần đầu bật tutorial spotlight nút `Booster_Wand` + bàn tay chỉ dẫn. Khi chọn 1 khối block bất kỳ (vùng ngoài và khối đá làm tối không thể ấn), đũa phép `booster3` bay lên giữa bàn cờ tung sóng năng lượng + mưa sao và làm **tất cả khối cùng màu đó — bất kể hình dạng, giá trị số hay đang đóng băng — tự động bay thoát vào cổng cùng màu**. |

---

## 🗺️ Hệ Thống 19 Màn Chơi (`Level 1` – `Level 19`), `HomeScene` & `FreshStart`

### 1. Màn hình chính & Scene Chơi Lại Từ Đầu
- **`HomeScene.unity` (`HomeUI.cs`)**: Thang tiến trình màn chơi, hiển thị Tim (`5/5`), Xu (`356` mặc định), nút Play vào thẳng level hiện tại, nhận thưởng Facebook (`+20` xu), **Shop**, **Collection / Levels**, **Remove Ads Popup**, và **Settings**.
- **`FreshStart.unity` (`FreshGameLauncher.cs`)**: Scene đặc biệt để tổng quan lại toàn bộ game từ đầu. Mỗi lần load/chạy `FreshStart`, script tự động xóa sạch toàn bộ key tiến trình & tutorial booster (`BMP_CurrentLevel`, `BMP_JustWonLevel`, `BMP_TotalCoins`, `BMP_Lives`, `BMP_FbRewardClaimed`, `BMP_NoAdsPurchased`, `BMP_BoosterFreeze/Magnet/Wand`, `BMP_Freeze/Magnet/WandTutorialSeen`, `BlockMerge_BestScore`) rồi chuyển sang `HomeScene` ở Level 1.
  - *Lưu ý khi kiểm thử trên Unity Editor*: Hãy mở [FreshStart.unity](file:///D:/BlockMergePuzzle/Assets/Scenes/FreshStart.unity) rồi bấm **Play (▶)** (vì bấm Play trong Editor luôn chạy scene đang mở trong Hierarchy; còn vị trí `index 0` trong Build Profiles áp dụng khi build APK/EXE).

### 2. Tổng quan 19 Level
- **Level 1 – Level 10 (Scene & Prefab thủ công đã chốt)**:
  - Giữ nguyên cấu trúc cô lập (`Level1.unity`..`Level10.unity`, `Level1_Board.prefab`..`Level10_Board.prefab`) và bảo vệ không ghi đè trong `SceneSetupHelper`.
  - Level 1 (`5×3` hướng dẫn `TutorialHandGuide`), Level 2 (`6×6`), Level 3 (`5×7`), Level 4 (`4×5` BFS 22 bước), Level 5 (`8×11` đa hình), Level 6 (`7×8` + `Cross_3x3`), Level 7 (`8×4` + băng `Ice=2`), Level 8 (`8×9` + 2 lỗ chướng ngại + 2 khối `Ice=4` + **mở Booster Freeze**), Level 9 (`5×8` + khối `Ice=4`), Level 10 (`9×11` bản đồ cây thông, 26 khối).
- **Level 11 – Level 19 (Dữ liệu JSON `Assets/Resources/Levels/level_11..19.json` + Scene & Prefab riêng)**:
  - **Level 11**: `8×10`, 4 cổng, 12 khối (`120s`).
  - **Level 12**: `10×12`, 2 cổng, 4 khối (`120s`) — **mở Booster Magnet + Tutorial**.
  - **Level 13**: `8×11`, 7 cổng, 16 khối, 1 khối băng (`500s`).
  - **Level 14**: `10×12`, 5 cổng, 18 khối (`120s`).
  - **Level 15**: `10×11`, 4 cổng, 15 khối, 2 khối băng (`180s`).
  - **Level 16**: `10×12`, 4 cổng, 12 khối (`120s`) — **mở Booster Magic Wand + Tutorial**.
  - **Level 17**: `10×12`, 5 cổng, 19 khối, 1 khối băng (`120s`).
  - **Level 18**: `10×12`, 3 cổng, 11 khối, 2 khối băng (`120s`).
  - **Level 19**: `10×12`, 2 cổng, 17 khối (`120s`).

---

## 📱 Giao Diện Win / Lose / Audio & Công Cụ Level Editor

- **Win / Lose / TimeOut / Revive (`GameUI.cs`)**:
  - Đã tích hợp bộ sprite Figma (`WinUI`, `LoseUI`, `RemoveAdsUI`): ruy-băng `WELL DONE`, 3 ngôi sao vàng, cộng xu bay động (`PlayCoinTick`), nhân thưởng quảng cáo, màn hình hết giờ (`TimeOutPanel`) với tùy chọn thêm thời gian / hồi sinh tim (`PlayHeartBreak`, `PlayRevive`).
- **Pause / Settings Panel (`PausePanel`)**:
  - Giao diện pha lê tím (`Group 331`, `X (1)`), công tắc **Sound**, **Music**, **Vibration** lưu tự động qua `PlayerPrefs`.
- **Audio 2D & Procedural SFX (`SoundManager.cs`)**:
  - Âm thanh 2D (`DontDestroyOnLoad`): clip thực tế (`block_move.wav`, `merge_block.wav` combo 4 bậc trong `1.5s`, `block_vanish.mp3`, `icecrak.wav`) cùng các SFX procedural đầy đủ (`VictoryFanfare`, `TimeOutTone`, `HeartBreakTone`, `ReviveTone`, `CoinTickTone`, `FreezeTimeTone`, `MagnetPullTone`, `WandCastTone`).
- **Level Editor & Web Tool (`LevelEditorWindow.cs`, `CustomLevelSceneBaker.cs`, `Tools/LevelEditor.html`)**:
  - Cho phép xếp bàn cờ trực quan trong Unity Editor hoặc import JSON từ `Tools/LevelEditor.html`, tự render bàn cờ (`ProceduralBoardRenderer.cs`), bake thành `.unity` + `_Board.prefab` và Play Test ngay mà không ghi đè tiến trình người chơi (`EditorPrefs`).

---

## 📁 Cấu Trúc Thư Mục Quan Trọng

```text
D:\BlockMergePuzzle\
├── Assets\
│   ├── Prefabs\Levels\
│   │   └── Level1_Board.prefab .. Level19_Board.prefab  # Prefab bàn cờ 19 level
│   ├── Resources\
│   │   ├── Levels\level_1.json .. level_19.json           # Dữ liệu JSON 19 level
│   │   ├── Shaders\SpritePortalDissolve.shader            # Shader tan biến khi thoát cổng
│   │   ├── Sound\                                         # block_move, merge_block, block_vanish, icecrak
│   │   └── Sprites\                                       # Blocks, BoosterUI, CustomMaps, HomeUI, LoseUI, RemoveAdsUI, WinUI
│   ├── Scenes\
│   │   ├── FreshStart.unity                               # Scene reset toàn bộ game về mới (Index 0)
│   │   ├── HomeScene.unity                                # Màn hình Home (Index 1)
│   │   ├── MainScene.unity                                # Scene mặc định dự phòng (Index 2)
│   │   └── Level1.unity .. Level19.unity                  # 19 scene màn chơi
│   ├── Screenshots\                                       # Ảnh kiểm chứng các level & UI
│   ├── Scripts\
│   │   ├── Audio\SoundManager.cs                          # Âm thanh 2D + combo + procedural SFX (Freeze/Magnet/Wand...)
│   │   ├── Core\
│   │   │   ├── CustomLevelData.cs                         # Schema JSON level, gate, piece, moveAxis, isStone
│   │   │   ├── CustomLevelRepository.cs                   # Nạp/lưu JSON & PlayTest backup
│   │   │   ├── FreshGameLauncher.cs                       # Reset toàn bộ tiến trình & mở HomeScene
│   │   │   ├── GameBootstrapper.cs                        # Tự khởi tạo hệ thống (có guard cho HomeScene & FreshStart)
│   │   │   ├── GameConfig.cs                              # Enum màu, hình khối, gate side, tên TV
│   │   │   └── LevelManager.cs                            # Grid 1..19, merge, escape, băng, Magnet/Wand jump helpers
│   │   ├── Editor\
│   │   │   ├── CustomLevelSceneBaker.cs                   # Bake CustomLevelData ra Scene & Prefab
│   │   │   ├── LevelEditorWindow.cs                       # Cửa sổ Level Editor & Importer trong Unity
│   │   │   └── SceneSetupHelper.cs                        # Menu Load/Bake Level & AutoMcpStarter
│   │   ├── Gameplay\
│   │   │   ├── ExitGateView.cs                            # Cổng Tech Hologram & khóa ô cổng
│   │   │   ├── PlayerPieceController.cs                   # Kéo thả đa hướng, bàn phím & điều phối tap Magnet/Wand
│   │   │   └── PuzzlePiece.cs                             # Khối đa hình, băng IceCount, đá IsStone, Jump Merge/Gate
│   │   ├── UI\
│   │   │   ├── FreezeBoosterEffectController.cs           # Tutorial & VFX Booster 1 (Freeze 15s)
│   │   │   ├── GameUI.cs                                  # Header, Timer, BoosterBar, Pause, Victory, Lose/TimeOut
│   │   │   ├── HomeUI.cs                                  # Màn hình Home, Shop, Collection, Remove Ads
│   │   │   ├── MagnetBoosterController.cs                 # Tutorial, Spotlight vùng tối & VFX Booster 2 (Magnet)
│   │   │   ├── TutorialHandGuide.cs                       # Tay & banner hướng dẫn Level 1
│   │   │   └── WandBoosterController.cs                   # Tutorial, Spotlight & VFX Booster 3 (Magic Wand)
│   │   └── Utils\
│   │       ├── GemSpriteManager.cs                        # Nạp sprite từ Resources / AssetDatabase / disk
│   │       ├── ProceduralBoardRenderer.cs                 # Vẽ bàn cờ & viền tường procedural cho Level Editor
│   │       └── SpriteGenerator.cs                         # Tạo sprite procedural cho UI, băng, tuyết & VFX
│   └── Sprites\                                           # Asset gốc (Background, Home scene, Main Scene, Win&Lose scene)
├── ProjectSettings\EditorBuildSettings.asset              # 22 scenes (0..21): FreshStart, HomeScene, MainScene, Level 1..19
├── Tools\LevelEditor.html                                 # Web tool thiết kế JSON màn chơi
├── PROJECT_HANDOVER.md                                    # Báo cáo bàn giao kỹ thuật chi tiết
└── README.md                                              # Tài liệu tổng quan dự án
```
