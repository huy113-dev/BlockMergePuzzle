# BÁO CÁO TIẾN ĐỘ & BÀN GIAO DỰ ÁN BLOCK MERGE PUZZLE

## 1. Trạng Thái Hiện Tại (Ready for Next Session)
- **Unity Scene (`MainScene.unity`)**: Đã bake đầy đủ 21 blocks, 7 gates, khung đá `Board_Frame` trực tiếp trong Edit Mode (idle). Không cần bấm Play vẫn thấy toàn bộ layout.
- **Git Repository**: `https://github.com/huy113-dev/BlockMergePuzzle.git`
  - `main`: Branch gốc tích hợp gitignore chuẩn Unity.
  - `feat/core-puzzle-mechanics`: Core gameplay sliding, collinear merge, gate escape.
  - `fix/screen-fit-1440x3200`: Auto-fit camera portrait 1440x3200 (ratio 0.45) không bị crop viền.
  - `feat/block-lift-and-drag-gamefeel`: Game feel nhấc khối khi chạm (scale pop 1.075x, nâng sorting order 50, âm thanh pickup, dynamic tilt).
  - `feat/smooth-multi-directional-drag`: Di chuyển đa hướng theo ngón tay qua mọi khoảng trống (corridor navigation, quẹo góc mượt mà, snap/merge/escape linh hoạt).
- **Hệ thống kết nối Unity MCP**: FastMCP HTTP bridge hoạt động ổn định trên `http://127.0.0.1:8080/mcp`. Tự động kết nối lại khi domain reload nhờ `AutoMcpStarter` trong `SceneSetupHelper.cs`.
- **Screenshot kiểm chứng**: `Assets/Screenshots/screenshot-20261017-174145.png` (khớp 100% layout tài liệu tham chiếu 21 block).

---

## 2. Các Lỗi Đã Xử Lý Triệt Để

### a. Chuyển động giật giật (Movement Jitter / Teleport)
- **Nguyên nhân cũ**: Chuột thả ra làm reset tọa độ khối về gốc rồi mới chạy tween, hàm nội suy `SmoothStep` giật khựng, thời gian di chuyển gán cứng theo số bước.
- **Đã sửa**:
  - `PlayerPieceController.cs`: Loại bỏ reset vị trí khi nhả chuột; tính toán `targetSteps` theo ngưỡng 35% khoảng cách (`FloorToInt(dist / cellDim + 0.65f)`); thêm rubber-band khi kéo vướng chướng ngại vật; tự động lướt mượt về vị trí cũ nếu hủy kéo.
  - `PuzzlePiece.cs`: Dùng đường cong vận tốc liên tục `EaseOutQuad` (`1f - (1f - t) * (1f - t)`); tính `duration` động theo khoảng cách thực tế `Mathf.Clamp(dist / 9.5f, 0.08f, 0.25f)`.

### b. Merge thẳng hàng (Collinear Straight-Line Merge)
- **Nguyên nhân cũ**: Khối trượt ngang/dọc nhưng merge cả khi lệch hàng/cột (chéo hoặc chạm góc).
- **Đã sửa** trong `LevelManager.cs`:
  - Ép điều kiện thẳng hàng tuyệt đối:
    - Di chuyển ngang: `piece.GridOrigin.y == hitPiece.GridOrigin.y`
    - Di chuyển dọc: `piece.GridOrigin.x == hitPiece.GridOrigin.x`
  - Khóa điều kiện va chạm đa điểm (`multipleHits == false`).
  - Kiểm tra đủ 3 yếu tố: cùng hình dạng (`Shape`), cùng màu (`PieceColor`), cùng giá trị (`Value`). Nếu lệch hàng xuất cảnh báo: *"Khối bị lệch hàng/cột, không thể gộp!"*.

### c. Căn giữa cổng khi thoát (Centered Gate Exit)
- **Nguyên nhân cũ**: Khối thoát bay từ tọa độ grid biên, lệch tâm cổng.
- **Đã sửa** trong `LevelManager.cs`:
  - `SlideActionType.Escape`: Gán `targetWorldPos = result.TargetGate.transform.position`. Khối trượt vào chính giữa cổng trước khi chạy hiệu ứng bay ra ngoài theo hướng `gate.ExitDirection`.

### d. Căn giữa số trên thanh/khối
- **Đã sửa**: `PuzzlePiece.cs` và `ExitGateView.cs` căn `TextAnchor.MiddleCenter` tại tâm hình học bounding box.

### e. Tối ưu vừa vặn màn hình tỷ lệ hẹp dài 1440x3200 (Portrait 0.45)
- **Vấn đề cũ**: Màn hình 1440x3200 có tỉ lệ 9:20 (0.45) khiến camera mặc định cắt mất 2 viền bên của bàn cờ và cổng thoát.
- **Đã sửa**: Cập nhật `SceneSetupHelper.cs` và thêm `FitCameraToBoard()` trong `LevelManager.cs`. Camera tính toán theo cả width và height: `Mathf.Max(sizeByWidth, sizeByHeight)` với `orthographicSize = 10.5f` và vị trí `(0, 0, -10)`. Đảm bảo bàn cờ luôn nằm gọn gàng giữa màn hình trên mọi tỉ lệ.

### f. Game Feel: Cảm giác nhấc khối (Lift on Touch & Responsive Drag)
- **Tính năng**: Khi chạm hoặc kéo khối, khối phóng to nhẹ (1.075x), nhấc nổi lên `Z = -0.1f` và `Y +0.08f`, nâng `sortingOrder = 50` để nổi trên các khối khác; phát âm thanh click nhẹ có ngẫu nhiên cao độ (`SoundManager.PlayPickup`); nghiêng động (`tilt`) theo vận tốc di chuột và hạ mượt mà về kích thước gốc khi thả.

### g. Di chuyển đa hướng tự do theo ngón tay (Multi-Directional Corridor Navigation)
- **Vấn đề cũ**: Kéo khối bị khóa theo một tia thẳng cố định, không thể lượn theo ngón tay qua các khoảng trống quanh co hoặc góc ngoặc ("đơ và cứng").
- **Đã sửa**:
  - `PlayerPieceController.cs`: Thay thế raycast cứng bằng cơ chế dịch chuyển tâm ô động (`_currentOrigin`). Khi ngón tay kéo khối qua các ô trống kề cận (`CanPieceFitAt`), tâm ô tự động tịnh tiến theo hành trình ngón tay (`maxAdvances`). Khối có thể rẽ trái, xuống dưới, qua phải linh hoạt trong một lần kéo duy nhất mà không cần nhấc ngón tay.
  - Tự động cản góc chéo, tạo đàn hồi nhẹ khi kéo sát tường hoặc khối khác.
  - Phát hiện cơ hội Hợp nhất (Merge) hoặc Thoát cổng (Escape) thẳng hàng và thực thi ngay khi nhả chuột (`Input.GetMouseButtonUp`).


---

## 3. Bản Đồ File Quan Trọng

| Đường dẫn | Vai trò | Trạng thái |
|---|---|---|
| `Assets/Scripts/Core/LevelManager.cs` | Logic bàn cờ, raycast, merge thẳng hàng, thoát cổng, bake Edit Mode | Hoàn thiện |
| `Assets/Scripts/Gameplay/PlayerPieceController.cs` | Kéo thả trực quan Drag & Snap, không giật | Hoàn thiện |
| `Assets/Scripts/Gameplay/PuzzlePiece.cs` | Visual khối, render mảnh con, nhãn số, animation EaseOutQuad | Hoàn thiện |
| `Assets/Scripts/Gameplay/ExitGateView.cs` | Render 7 cổng thoát, căn giữa số | Hoàn thiện |
| `Assets/Scripts/Editor/SceneSetupHelper.cs` | Menu item Bake level + AutoMcpStarter tự reconnect | Hoàn thiện |
| `C:\Users\OS\.claude\jobs\fb04934c\tmp\mcp_client.py` | Script cầu nối MCP gọi tool/C# từ CLI | Sẵn sàng |

---

## 4. Hướng Dẫn Phiên Tiếp Theo
1. Mở Unity Editor và MCP bridge:
   - Tool `SceneSetupHelper.BakeLevelToScene` (Menu: `Tools/Block Merge Puzzle/Bake Level to Scene (Idle Display)`).
2. Kiểm tra/thêm các tính năng tiếp theo nếu người dùng yêu cầu:
   - Hệ thống màn chơi kế tiếp (Level 2, Level 3).
   - Hiệu ứng âm thanh / Particle FX khi merge và escape.
   - Hoàn thiện UI Popups (Win/Lose/Settings).
