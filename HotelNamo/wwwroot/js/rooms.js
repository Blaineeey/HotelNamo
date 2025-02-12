document.addEventListener("DOMContentLoaded", function () {
    function filterRooms(category) {
        let rooms = document.querySelectorAll('.room-card');
        rooms.forEach(room => {
            if (category === 'all' || room.classList.contains(category)) {
                room.style.display = 'block';
            } else {
                room.style.display = 'none';
            }
        });
    }

    document.querySelectorAll(".filter-btn").forEach(button => {
        button.addEventListener("click", function () {
            document.querySelectorAll(".filter-btn").forEach(btn => btn.classList.remove("active"));
            this.classList.add("active");
            filterRooms(this.getAttribute("data-category"));
        });
    });
});
