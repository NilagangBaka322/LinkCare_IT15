function applyFilters() {
    const search = document.getElementById("searchPatient").value.toLowerCase();
    const start = new Date(document.getElementById("filterStart").value);
    const end = new Date(document.getElementById("filterEnd").value);

    document.querySelectorAll(".accordion-item").forEach(item => {
        const name = item.querySelector("button").innerText.toLowerCase();
        const dateText = item.querySelector("button").innerText.split("•")[1]?.trim();
        const date = dateText ? new Date(dateText) : null;

        let visible = true;
        if (search && !name.includes(search)) visible = false;
        if (start && date && date < start) visible = false;
        if (end && date && date > end) visible = false;

        item.style.display = visible ? "" : "none";
    });
}
