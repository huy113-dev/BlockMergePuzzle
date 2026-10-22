# BÁO CÁO TIẾN ĐỘ & BÀN GIAO DỰ ÁN BLOCK MERGE PUZZLE

## 1. Trạng Thái Hiện Tại (Ready for Next Session)
- **Hệ thống màn chơi hoàn chỉnh (Multi-Level Architecture)**:
  - **Level 1 (Tutorial Cross Map)**: Xây dựng theo asset gốc `Map lv1.png` (bàn cờ hình chữ thập 7×6, tâm tại `(0, 0, 0)`, 5 khối đá 1x1, 2 cổng thoát có badge tương phản số). Đã loại bỏ gạch sàn theo yêu cầu để giữ trọn vẹn nền tím tự nhiên của asset.
  - **Level 2 (Reference Board 6×8)**: Bàn cờ 6×8 đầy đủ 21 blocks, 7 cổng thoát đa hướng.
- **Lưu trữ Scene & Prefab độc lập**:
  - `Assets/Scenes/MainScene.unity`: Scene làm việc mặc định (đang kích hoạt Level 1 sạch sẽ).
  - `Assets/Scenes/Level1.unity`: Scene độc lập cho Level 1.
  - `Assets/Scenes/Level2.unity`: Scene độc lập cho Level 2.
  - `Assets/Prefabs/Levels/Level1_Board.prefab`: Prefab nguyên bản bàn cờ Level 1.
  - `Assets/Prefabs/Levels/Level2_Board.prefab`: Prefab nguyên bản bàn cờ Level 2.
- **Git Repository**: `https://github.com/huy113-dev/BlockMergePuzzle.git`
  - `main`: Branch gốc tích hợp gitignore chuẩn Unity.
  - `feat/core-puzzle-mechanics`: Core gameplay sliding, collinear merge, gate escape.
  - `fix/screen-fit-1440x3200`: Auto-fit camera portrait 1440x3200 (ratio 0.45) không bị crop viền.
  - `feat/block-lift-and-drag-gamefeel`: Game feel nhấc khối khi chạm (scale pop 1.075x, nâng sorting order 50, âm thanh pickup, dynamic tilt).
  - `feat/smooth-multi-directional-drag`: Di chuyển đa hướng theo ngón tay qua mọi khoảng trống (corridor navigation, quẹo góc mượt mà, snap/merge/escape linh hoạt).
  - `feat/level-1`: **[Branch mới nhất]** Cấu hình Level 1 Tutorial Cross, lưu scene và prefab riêng, bỏ gạch sàn, đồng bộ chuyển đổi Level 1 và Level 2.
- **Hệ thống kết nối Unity MCP**: FastMCP HTTP bridge hoạt động ổn định trên `http://127.0.0.1:8080/mcp`. Tự động kết nối lại khi domain reload nhờ `AutoMcpStarter` trong `SceneSetupHelper.cs`.
- **Screenshot kiểm chứng**:
  - `Assets/Screenshots/screenshot-20261021-022003.png`: Ảnh chụp Level 1 sạch không nền gạch, căn chỉnh hoàn hảo.
  - `Assets/Screenshots/screenshot-20261017-174145.png`: Ảnh chụp Level 2 (21 khối trên bàn 6×8).

---

## 2. Các Tính Năng & Lỗi Kỹ Thuật Đã Xử Lý

### a. Hệ thống Level 1 Tutorial Cross Map
- **Hệ tọa độ chữ thập**: Grid 7 cột (0..6) × 6 hàng (0..5). Hành lang hợp lệ: `(col == 3 && row >= 0 && row < 6) || (row == 2 && col >= 0 && col < 7)`.
- **Căn gốc tọa độ**: Giao điểm `(3, 2)` cố định tại `Vector3.zero`. Tọa độ chuyển đổi:
  - `x = (col - 3) * CurrentCellWidth`
  - `y = (2 - row) * CurrentCellHeight`
- **Bố cục cổng & khối**:
  - Cổng trên: Xanh lá [2] tại `(0, 3.15, 0)`.
  - Cổng trái: Đỏ [3] tại `(-3.90, 0, 0)`.
  - Khối: 1 Đỏ [2] tại tâm, 2 Đỏ [1] ở 2 đầu cánh ngang, 2 Xanh lá [1] ở 2 đầu cánh dọc.
- **Badge tương phản cổng (`ExitGateView.cs`)**: Tạo đĩa tròn tối màu `#141724E0` phía sau số cổng (`sortingOrder = 4`) giúp chữ số hiển thị rõ ràng trên nền mũi tên trắng.
- **Lược bỏ gạch sàn**: Đã loại bỏ các GameObject `FloorTile` trong hành lang, bảo toàn nền tím nguyên bản của asset `Map lv1.png`.

### b. Chuyển động giật giật (Movement Jitter / Teleport)
- **Đã sửa**:
  - `PlayerPieceController.cs`: Loại bỏ reset vị trí khi nhả chuột; tính toán `targetSteps` theo ngưỡng 35% khoảng cách; thêm rubber-band khi kéo vướng chướng ngại vật; tự động lướt mượt về vị trí cũ nếu hủy kéo.
  - `PuzzlePiece.cs`: Dùng đường cong vận tốc liên tục `EaseOutQuad` (`1f - (1f - t) * (1f - t)`); tính `duration` động theo khoảng cách thực tế `Mathf.Clamp(dist / 9.5f, 0.08f, 0.25f)`.

### c. Merge thẳng hàng (Collinear Straight-Line Merge)
- **Đã sửa** trong `LevelManager.cs`:
  - Ép điều kiện thẳng hàng tuyệt đối:
    - Di chuyển ngang: `piece.GridOrigin.y == hitPiece.GridOrigin.y`
    - Di chuyển dọc: `piece.GridOrigin.x == hitPiece.GridOrigin.x`
  - Khóa điều kiện va chạm đa điểm (`multipleHits == false`).
  - Kiểm tra đủ 3 yếu tố: cùng hình dạng (`Shape`), cùng màu (`PieceColor`), cùng giá trị (`Value`).

### d. Căn giữa cổng khi thoát (Centered Gate Exit)
- **Đã sửa** trong `LevelManager.cs`:
  - `SlideActionType.Escape`: Gán `targetWorldPos = result.TargetGate.transform.position`. Khối trượt vào chính giữa cổng trước khi chạy hiệu ứng bay ra ngoài theo hướng `gate.ExitDirection`.

### e. Tối ưu vừa vặn màn hình tỷ lệ hẹp dài 1440x3200 (Portrait 0.45)
- **Đã sửa**: `SceneSetupHelper.cs` và `FitCameraToBoard()` trong `LevelManager.cs`. Camera tính toán theo cả width và height: `Mathf.Max(sizeByWidth, sizeByHeight)` với `orthographicSize = 10.45f` và vị trí `(0, 0, -10)`. Kèm clamp chống vỡ khung hình trong Edit Mode khi máy tính ở dạng landscape (`if (screenRatio > 0.75f) screenRatio = 1440f / 3200f;`).

### f. Game Feel: Cảm giác nhấc khối (Lift on Touch & Responsive Drag)
- **Tính năng**: Khi chạm hoặc kéo khối, khối phóng to nhẹ (1.075x), nhấc nổi lên `Z = -0.1f` và `Y +0.08f`, nâng `sortingOrder = 50` để nổi trên các khối khác; phát âm thanh click nhẹ (`SoundManager.PlayPickup`); nghiêng động (`tilt`) theo vận tốc di chuột và hạ mượt mà về kích thước gốc khi thả.

### g. Di chuyển đa hướng tự do theo ngón tay (Multi-Directional Corridor Navigation)
- **Tính năng**: Kéo khối qua các ô trống kề cận (`CanPieceFitAt`), tâm ô tự động tịnh tiến theo hành trình ngón tay (`maxAdvances`). Khối có thể rẽ trái, xuống dưới, qua phải linh hoạt trong một lần kéo duy nhất mà không cần nhấc ngón tay.

---

## 3. Bản Đồ File Quan Trọng

| Đường dẫn | Vai trò | Trạng thái |
|---|---|---|
| `Assets/Scenes/MainScene.unity` | Scene chính của game (mặc định nạp Level 1) | Hoàn thiện |
| `Assets/Scenes/Level1.unity` | Scene riêng biệt nạp sẵn Level 1 Tutorial | Hoàn thiện |
| `Assets/Scenes/Level2.unity` | Scene riêng biệt nạp sẵn Level 2 6x8 Board | Hoàn thiện |
| `Assets/Prefabs/Levels/Level1_Board.prefab` | Prefab trọn bộ bàn cờ, cổng, khối Level 1 | Hoàn thiện |
| `Assets/Prefabs/Levels/Level2_Board.prefab` | Prefab trọn bộ bàn cờ, cổng, khối Level 2 | Hoàn thiện |
| `Assets/Scripts/Core/LevelManager.cs` | Logic bàn cờ đa level, raycast, merge thẳng hàng, thoát cổng, auto-fit camera | Hoàn thiện |
| `Assets/Scripts/Gameplay/PlayerPieceController.cs` | Kéo thả đa hướng trực quan Drag & Snap, dynamic tilt | Hoàn thiện |
| `Assets/Scripts/Gameplay/PuzzlePiece.cs` | Visual khối, render mảnh con, nhãn số, animation EaseOutQuad | Hoàn thiện |
| `Assets/Scripts/Gameplay/ExitGateView.cs` | Render cổng thoát, đĩa badge tương phản số | Hoàn thiện |
| `Assets/Scripts/Editor/SceneSetupHelper.cs` | Menu items chuyển đổi level + AutoMcpStarter tự reconnect | Hoàn thiện |
| `Assets/Scripts/Utils/GemSpriteManager.cs` | Nạp sprite động từ Resources/Disk cho mọi level | Hoàn thiện |

---

## 4. Hướng Dẫn Thao Tác Nhanh

1. **Chuyển đổi màn chơi trên Unity Editor**:
   - `Tools/Block Merge Puzzle/Load Level 1 (Tutorial Cross)`: Nạp Level 1.
   - `Tools/Block Merge Puzzle/Load Level 2 (Reference 6x8)`: Nạp Level 2.
   - `Tools/Block Merge Puzzle/Bake Level to Scene (Idle Display)`: Cập nhật lại màn đang chọn.
2. **Kiểm tra tự động bằng Python qua FastMCP**:
   - Chạy các script test trong `C:\Users\OS\.claude\jobs\fb04934c\tmp\`:
     - `uv run --with mcp python test_level1_flow.py`: Kiểm tra layout, khối, cổng Level 1.
     - `uv run --with mcp python test_logic.py`: Kiểm tra logic di chuyển, merge và escape.
     - `uv run --with mcp python bake_and_screenshot.py`: Bake và chụp ảnh Game View.
