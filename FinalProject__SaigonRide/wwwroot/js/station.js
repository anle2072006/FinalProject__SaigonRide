document.addEventListener('DOMContentLoaded', function () {

    // ==========================================
    // XỬ LÝ THANH SIDEBAR (Chuyển tab active)
    // ==========================================
    const items = document.querySelectorAll(".sidebar li");

    if (items.length > 0) {
        items.forEach(item => {
            item.addEventListener("click", () => {
                // Xóa class active ở tất cả các thẻ li
                items.forEach(i => i.classList.remove("active"));
                // Thêm class active vào thẻ li vừa được click
                item.classList.add("active");
            });
        });
    }

});