# Commit Convention

Dự án này sử dụng tiêu chuẩn **Conventional Commits** để viết commit message. Điều này giúp lịch sử git luôn rõ ràng, dễ theo dõi tiến độ và liên kết với các Task đang làm.

## 1. Cấu trúc cơ bản

```text
<type>(<scope>): <description>

[optional body]
```

## 2. Các loại Commit (Type)

| Type       | Mục đích sử dụng                                                                                                                |
| :--------- | :------------------------------------------------------------------------------------------------------------------------------ |
| `feat`     | Thêm một tính năng mới của game.                                                                                                |
| `fix`      | Sửa lỗi bug (ví dụ lỗi logic, UI, lỗi không compile được).                                                                      |
| `docs`     | Thay đổi hoặc bổ sung tài liệu (Markdown docs, GDD, TDD...).                                                                    |
| `style`    | Cập nhật định dạng code (khoảng trắng, thụt lề, comment) - không ảnh hưởng đến logic.                                           |
| `refactor` | Chỉnh sửa, tái cấu trúc code nhưng **không** thêm tính năng cũng **không** sửa lỗi.                                             |
| `perf`     | Cải thiện hiệu năng thuật toán, tối ưu bộ nhớ.                                                                                  |
| `test`     | Thêm unit/playmode test mới hoặc sửa các test hiện tại.                                                                         |
| `chore`    | Các công việc thường ngày: cấu hình hệ thống build, cập nhật tool, sửa cấu hình project (ví dụ: cập nhật .gitignore, packages). |

## 3. Phạm vi (Scope) - Khuyến nghị

Trong dấu ngoặc đơn `()` nên là mã **Task ID** tương ứng trong file `TASK_MANAGER.md`, hoặc tên module/phần đang làm nếu không nằm trong task nào (vd: `Combat`, `Map`, `UI`).
Điều này giúp việc đối chiếu commit với task rành mạch hơn.

Ví dụ: `(TASK-001)`, `(TASK-012)`, `(Combat)`, `(UI)`.

## 4. Quy tắc viết Tiêu đề (Description)

- Bắt đầu bằng động từ mệnh lệnh: *thêm, sửa, xóa, tối ưu, ẩn, cập nhật...*
- Khuyến khích viết ngắn gọn, súc tích (dưới 70 ký tự).
- **KHÔNG** bắt đầu bằng chữ hoa (nếu dùng tiếng Anh) và **KHÔNG** có dấu chấm cuối câu. 

## 5. Ví dụ cụ thể

**Ví dụ khi làm xong một Task trọn vẹn:**
```text
feat(TASK-001): khởi tạo project unity và thiết lập folder structure
```

**Ví dụ code logic (có phần body giải thích chi tiết):**
```text
fix(TASK-013): sửa lỗi tính toán HP tối đa khi VIT > 100

- Sửa sai số trong công thức tính toán max_hp ở hàm RecalculateBaseAttributes.
- Thay vì lấy 1000 x VIT chia (VIT + 90), phép chia số nguyên đang làm sai lệch kết quả. Đã ép kiểu sang float.
```

**Ví dụ một thay đổi nhỏ không thuộc task:**
```text
docs: cập nhật hướng dẫn thiết lập hệ thống trong README.md
```
```text
chore: thêm plugin Newtonsoft Json vào project
```
