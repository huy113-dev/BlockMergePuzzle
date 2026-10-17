# BÁO CÁO TIẾN ĐỘ & BÀN GIAO DỰ ÁN BLOCK MERGE PUZZLE

## 1. Trạng Thái Hiện Tại (Ready for Next Session)
- **Unity Scene (`MainScene.unity`)**: Đã bake đầy đủ 21 blocks, 7 gates, khung đá `Board_Frame` trực tiếp trong Edit Mode (idle). Không cần bấm Play vẫn thấy toàn bộ layout.
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
