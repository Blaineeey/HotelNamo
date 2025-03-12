document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchInput");
    const categoryFilter = document.getElementById("categoryFilter");

    searchInput.addEventListener("input", filterRooms);
    categoryFilter.addEventListener("change", filterRooms);

    function filterRooms() {
        const searchValue = searchInput.value.toLowerCase();
        const selectedCategory = categoryFilter.value;
        const roomCards = document.querySelectorAll(".room-card");

        roomCards.forEach(room => {
            const roomNumber = room.getAttribute("data-roomnumber").toLowerCase();
            const category = room.getAttribute("data-category");

            const matchesSearch = roomNumber.includes(searchValue);
            const matchesCategory = selectedCategory === "" || category === selectedCategory;

            if (matchesSearch && matchesCategory) {
                room.style.display = "block";
            } else {
                room.style.display = "none";
            }
        });
    }
});
