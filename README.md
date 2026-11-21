# 💎 Block Merge Puzzle — Unity Project

Dự án game giải đố trượt khối đá quý (**Sliding Block Merge & Escape Puzzle**) xây dựng trên **Unity `6000.0.74f1`** (Target: **Android**), tại `D:\BlockMergePuzzle`. Hiện tại dự án đã hoàn thiện trọn vẹn **10/10 màn chơi (`Level 1` – `Level 10`)**, đầy đủ vòng lặp gameplay, hiệu ứng VFX/Shader, cơ chế khối đóng băng, hệ thống Booster, Pause/Settings, Victory Popup và âm thanh 2D.

---

## 🕹️ Cơ Chế Gameplay Chính

1. **Trượt Đa Hướng Linh Hoạt (`Multi-Directional Drag & Snap`)**:
   - Chạm và rê ngón tay/chuột dẫn hướng khối đá quý lướt mượt qua các khúc cua và hành lang mà không cần nhả tay (`_currentOrigin`).
   - Hỗ trợ phím `W/A/S/D` hoặc các phím mũi tên để dịch chuyển từng bước trên bàn cờ.
   - Hiệu ứng **Lift & Tilt**: Nhấc khối nổi lên (`scale 1.075x`, `sortingOrder = 50`), nghiêng nhẹ theo vận tốc kéo ngang (`±3.5°`) và hạ mượt mà khi đặt xuống.

2. **Hợp Nhất Thẳng Hàng & Merge Slam VFX (`Collinear Straight-Line Merge`)**:
   - Hai khối **cùng hình dạng**, **cùng màu sắc**, **cùng giá trị số** và **nằm thẳng hàng/cột** khi va chạm sẽ gộp thành khối mới có giá trị `+1` (`+50 × value` điểm).
   - Hiệu ứng **Merge Slam**: Khối nhấc lơ lửng lao tới, đập squash/stretch, chớp silhouette trắng (`GUI/Text Shader`), bung hào quang sao (`SpawnMergeFlash`) và rung nhẹ camera (`KickCameraImpact`).

3. **Thoát Cổng Chính Xác, Khóa Ô Cổng & Portal Dissolve (`Gate Escape`)**:
   - **Khóa ô cổng (`Gate Cell Blocking`)**: Cổng thoát đóng vai trò tường chắn cứng — khối tuyệt đối không thể dừng hoặc đè lên ô cổng khi chưa đủ điều kiện.
   - **Khớp điều kiện chính xác**: Khối phải **cùng màu**, **đúng hướng cổng**, khớp bề rộng cổng đa ô và có giá trị **bằng đúng `RequiredValue`** (`piece.Value == gate.RequiredValue`); thông báo lỗi chi tiết nếu sai màu, thiếu số hoặc quá số.
   - **Portal Dissolve VFX**: Khi thoát cổng, khối trượt vào tâm cổng và tan biến qua shader `Custom/SpritePortalDissolve` kèm hạt vỡ (`+150 × value` điểm). Giải phóng toàn bộ khối để chiến thắng màn chơi.

4. **Cơ Chế Khối Đóng Băng (`IceCount` — Từ Level 7, 8, 9)**:
   - Một số khối bị bọc trong lớp băng (`ice_block_overlay.png`) kèm chữ số đếm ngược 3D, ẩn hoàn toàn khối bên trong và không thể di chuyển hay gộp.
   - Mỗi lần một khối khác thoát cổng thành công, giá trị băng giảm đi `1`. Khi về `0`, lớp băng phát nổ thành mảnh băng kèm âm thanh `icecrak.wav` và giải phóng khối bên trong.

---

## 🗺️ Hệ Thống 10 Màn Chơi (`Level 1` -> `Level 10`)

Mỗi màn chơi được cô lập thành Scene (`Assets/Scenes/Level{n}.unity`) và Prefab (`Assets/Prefabs/Levels/Level{n}_Board.prefab`) riêng, bảo vệ không ghi đè lẫn nhau qua `SceneSetupHelper.LoadLevel()`.

| Level | Map Asset | Lưới | Cổng Thoát | Số Khối | Điểm Nhấn / Cơ Chế Đặc Biệt |
|---|---|---|---|---|---|
| **Level 1** | `map_lv1.png` | 5×3 | 1 (Đỏ `[2]`) | 2 (`1x1`) | Màn hướng dẫn `TutorialHandGuide` 2 pha |
| **Level 2** | `map_lv2.png` | 6×6 | 2 (Xanh `[2]`, Đỏ `[3]`) | 5 (`1x1`) | Chữ thập lệch tâm + `Level2CellPositions` |
| **Level 3** | `Map_lv3.png` | 5×7 | 2 (Xanh `[4]`, Đỏ `[4]`) | 8 (`1x1`) | Bản đồ 3 nhánh nối qua trục giữa |
| **Level 4** | `map_lv4.png` | 4×5 | 3 (Đỏ `[3]`, Xanh `[3]`, Hồng `[2]`) | 7 (`1x1`) | Bản đồ chữ E, kiểm chứng BFS 22 bước |
| **Level 5** | `map_lv5.png` | 8×11 | 2 (Vàng `[5]`, Xanh `[2]`) | 17 | Đa hình: `2x1`, `1x2`, chữ `L`, `2x2` |
| **Level 6** | `map_lv6 (1).png` | 7×8 | 4 (Đỏ `[2]`, Xanh `[2]`, Cam `[2]`, Hồng `[3]`) | 10 | Khối chữ thập `Cross_3x3`, 4 cổng bất đối xứng |
| **Level 7** | `map_lv7.png` | 8×4 | 3 (Xanh `[2]`, Vàng `[2]`, Hồng `[2]`) | 5 | Tháp bậc thang + khối `2x2` **đóng băng `Ice=2`** |
| **Level 8** | `map_lv8.png` | 8×9 | 5 (Xanh `[2]`, Cam `[2]`, Tím `[2]`, Dương `[2]`, Vàng `[2]`) | 15 | 2 lỗ chướng ngại nội bộ + 2 khối `2x2` **đóng băng `Ice=4`** |
| **Level 9** | `map_lv9.png` | 5×8 | 5 (Đỏ `[2]`, Tím `[3]`, Vàng `[3]`, Dương `[2]`, Xanh `[3]`) | 12 | Chốt 2D chuẩn + 1 khối `2x2` **đóng băng `Ice=4`** + **mở Booster Freeze** |
| **Level 10** | `map_lv10 (2).png` | 9×11 | 4 (Đỏ `[5]*`, Cam `[2]`, Xanh Trái `[5]`, Xanh Phải `[4]`) | 26 | Bản đồ cây thông Noel, đa hình (`1x1`, `2x1`, `1x2`, `L`) |

*\*Ghi chú đồng bộ scene: Trong `Level8.unity`, 2 khối `2x2` đã cấu hình `IceCount = 4`; trong `Level9.unity`, khối `2x2` tại `(1,3)` có `IceCount = 4`; trong `Level10.unity`, cổng Đỏ trên được tinh chỉnh `RequiredValue = 5` trên scene (mặc định code `LevelManager.cs` là `4`).*

---

## 📱 Giao Diện, Booster, Audio & Tối Ưu Mobile (`1440×3200`)

- **Thanh Header (`HeaderPanel`)**:
  - **Nút Replay (`Btn_Replay`)**: Tải lại nhanh màn chơi và đặt lại thời gian ban đầu.
  - **Banner Time (`Banner_Time` + `TimeVal`)**: Đếm ngược linh hoạt (hỗ trợ đặt qua `_customTimeMMSS`, `_customDurationSeconds` hoặc sửa trực tiếp text `TimeVal` trên Inspector); chuyển đỏ cảnh báo khi `< 20s` và xanh cyan khi đóng băng.
  - **Banner Level (`Banner_Level`)**: Tự động hiển thị `LEVEL 1` .. `LEVEL 10`.
  - **Nút Pause (`Btn_Pause`)**: Mở hộp thoại cài đặt / tạm dừng.
- **Thanh Booster (`BoosterBar`)**:
  - **Freeze (`x3`, mở tại Level 9)**: Đóng băng bộ đếm thời gian trong `15s`.
  - **Magnet (`x2`, mở tại Level 12)**: Tự động ghép một cặp khối hợp lệ trên bàn cờ (`TryMagnetAutoMerge`).
  - **Wand (`x1`, mở tại Level 16)**: Đã dựng slot & nút kích hoạt (đang mở rộng logic phép thuật).
  - Các booster chưa mở khóa hiển thị mờ kèm ổ khóa và nhãn `Lv.X`.
- **Pause / Settings Panel (`PausePanel`)**:
  - Giao diện pha lê tím (`Group 331`, `X (1)`), công tắc **Sound**, **Music**, **Vibration** lưu tự động qua `PlayerPrefs`.
- **Victory Popup (`VictoryPanel`) & Chuyển Màn**:
  - Ruy-băng vàng `LEVEL X CLEAR!`, 3 ngôi sao vàng bật nảy tuần tự, hiển thị thời gian còn lại, nút `NEXT LEVEL X >` chuyển thẳng sang scene màn tiếp theo và nút `CHƠI LẠI`.
  - Đã import thêm bộ sprite Win/Lose mới (`Assets/Sprites/Win&Lose scene/win scene/UI B1 (15)/`: `WELL DONE`, `Coin`, `Coins`, `ADS`, `Button1/2`) cho đợt nâng cấp UI tiếp theo.
- **Audio 2D & VFX (`SoundManager`)**:
  - Âm thanh 2D (`DontDestroyOnLoad`), sử dụng clip thực tế (`block_move.wav`, `merge_block.wav` tăng 4 bậc âm lượng combo trong `1.5s`, `block_vanish.mp3`, `icecrak.wav`) cùng fallback procedural (bao gồm `VictoryFanfare`).
- **Camera Auto-Fit & Background**:
  - Phủ nền vũ trụ pha lê `background_main scene .png` theo cơ chế Aspect-Fill và tự động fit camera theo tỷ lệ dọc `1440×3200` (`0.45`).

---

## 🚀 Hướng Dẫn Mở Dự Án, Công Cụ Editor & Chạy Game

### 1. Mở dự án trong Unity
- Mở **Unity Hub** -> **Add project from disk** -> chọn `D:\BlockMergePuzzle` (khuyến nghị Unity **`6000.0.74f1`**).
- Scene mặc định: `Assets/Scenes/MainScene.unity`, hoặc mở trực tiếp `Assets/Scenes/Level1.unity` .. `Level10.unity`.

### 2. Công cụ Editor (`Tools/Block Merge Puzzle/...`)
- `Load Level 1` .. `Load Level 10`: Chuyển nhanh giữa 10 scene màn chơi (an toàn, không ghi đè).
- `Bake Level to Scene (Idle Display)`: Render bàn cờ của level đang chọn trong Edit Mode và lưu prefab tương ứng.
- `Verify Merge Slam`: Kiểm tra đầy đủ shader, sprite, booster, setting UI và audio clips.
- `Upgrade Settings Panel to New Style` / `Update Level Banner & Settings Close X` / `Ensure Time Text on Banner`: Các tiện ích đồng bộ UI nhanh.

### 3. Chạy & điều khiển game
- Nhấn **Play (▶)** trên Unity Editor để chơi thử.
- Kéo thả khối trực tiếp bằng chuột/cảm ứng, hoặc chọn khối và dùng phím `W/A/S/D` / mũi tên để trượt từng bước.

---

## 📁 Cấu Trúc Thư Mục Quan Trọng

```text
D:\BlockMergePuzzle\
├── Assets\
│   ├── Prefabs\Levels\
│   │   └── Level1_Board.prefab .. Level10_Board.prefab   # Prefab bàn cờ 10 level
│   ├── Resources\
│   │   ├── Shaders\SpritePortalDissolve.shader           # Shader tan biến khi thoát cổng
│   │   ├── Sound\                                        # block_move, merge_block, block_vanish, icecrak
│   │   └── Sprites\                                      # Blocks & LevelAssets nạp qua Resources
│   ├── Scenes\
│   │   ├── MainScene.unity                               # Scene khởi đầu mặc định
│   │   └── Level1.unity .. Level10.unity                 # 10 scene độc lập đã chốt
│   ├── Screenshots\                                      # Ảnh chụp nghiệm thu từng level & UI
│   ├── Scripts\
│   │   ├── Audio\SoundManager.cs                         # Âm thanh 2D + combo + procedural fallback
│   │   ├── Core\
│   │   │   ├── GameBootstrapper.cs                       # Tự khởi tạo hệ thống khi chạy
│   │   │   ├── GameConfig.cs                             # Enum màu, 8 hình khối, gate side, tên TV
│   │   │   └── LevelManager.cs                           # Grid 10 level, merge, escape, băng, VFX
│   │   ├── Editor\SceneSetupHelper.cs                    # Menu Load/Bake Level & AutoMcpStarter
│   │   ├── Gameplay\
│   │   │   ├── ExitGateView.cs                           # Cổng Tech Hologram & khóa ô cổng
│   │   │   ├── PlayerPieceController.cs                  # Kéo thả đa hướng & bàn phím
│   │   │   └── PuzzlePiece.cs                            # Khối đa hình, băng IceCount, slam merge
│   │   ├── UI\
│   │   │   ├── GameUI.cs                                 # Header, Timer, Booster, Pause, Victory
│   │   │   └── TutorialHandGuide.cs                      # Tay & banner hướng dẫn Level 1
│   │   └── Utils\
│   │       ├── GemSpriteManager.cs                       # Nạp sprite từ Resources / AssetDatabase / disk
│   │       └── SpriteGenerator.cs                        # Tạo sprite procedural cho UI & VFX
│   └── Sprites\
│       ├── Background\                                   # Background vũ trụ pha lê
│       ├── Main Scene\
│       │   ├── Block\Block\                              # Sprite các khối màu & lớp băng
│       │   ├── Map\                                      # map_lv1.png .. map_lv10 (2).png
│       │   ├── Pause game\                               # Asset Pause cũ
│       │   └── UI\                                       # Header, booster, setting tím pha lê
│       └── Win&Lose scene\                               # Bộ sprite Win&Lose Figma mới (WIP)
├── ProjectSettings\EditorBuildSettings.asset             # Build index 0..10 (MainScene + Level 1..10)
├── PROJECT_HANDOVER.md                                   # Báo cáo bàn giao kỹ thuật chi tiết
└── README.md                                             # Tài liệu tổng quan dự án
```

---

## 📌 Trạng Thái Phát Triển & Hạng Mục Tiếp Theo

- **Đã hoàn thành**: 10/10 màn chơi (`Level 1`–`Level 10`), cơ chế kéo thả & merge thẳng hàng, cổng Tech Hologram + shader tan biến, khối đóng băng `IceCount`, Booster Freeze/Magnet UI, Pause Settings tím pha lê, Victory Popup 3 sao + Next Level, âm thanh 2D.
- **Tiếp theo (WIP)**:
  1. Gắn bộ sprite Figma `Assets/Sprites/Win&Lose scene/win scene/UI B1 (15)/` vào màn hình Win và bổ sung **Lose Popup** khi hết giờ.
  2. Hoàn thiện hiệu ứng Booster **Wand**, tính năng **No Ads** & hệ thống **Coin**.
  3. Bổ sung nhạc nền (**BGM**) & rung (**Haptics**) gắn với toggle Settings, đồng bộ cấu hình Inspector Level 8–10 vào code và mở rộng **Level 11+**.
