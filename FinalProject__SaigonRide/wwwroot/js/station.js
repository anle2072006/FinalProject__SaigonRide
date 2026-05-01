const items = document.querySelectorAll(".sidebar li");

items.forEach(item => {
    item.addEventListener("click", () => {
        items.forEach(i => i.classList.remove("active"));
        item.classList.add("active");
    });
});