document.addEventListener('DOMContentLoaded', function () {
    const appointments = window.doctorAppointments || [];

    // ======================
    // Initialize Calendar
    // ======================
    const calendarEl = document.getElementById('calendar');
    if (calendarEl) {
        const calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'dayGridMonth',
            height: getCalendarHeight(),
            selectable: true,
            editable: true,
            nowIndicator: true,
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: getRightToolbar()
            },
            events: appointments.map(a => ({
                id: a.id,
                title: `${a.title} - ${a.patientName || a.walkInName}`,
                start: a.startDate,
                end: a.endDate,
                extendedProps: {
                    patient: a.patientName || a.walkInName,
                    status: a.status || 'Scheduled'
                }
            })),
            eventClick: info => {
                Swal.fire({
                    title: info.event.title,
                    html: `<b>Patient:</b> ${info.event.extendedProps.patient}<br/>
                           <b>Status:</b> ${info.event.extendedProps.status}`,
                    icon: "info"
                });
            }
        });
        calendar.render();
        window.doctorCalendar = calendar;
    }

    function getCalendarHeight() {
        const w = window.innerWidth;
        if (w < 576) return 260;
        if (w < 768) return 300;
        if (w < 992) return 340;
        return 420;
    }

    function getRightToolbar() {
        return window.innerWidth < 576 ? 'dayGridMonth' : 'dayGridMonth,timeGridWeek,timeGridDay';
    }

    // ======================
    // Patient Type Toggle
    // ======================
    const registeredContainer = document.getElementById('registeredPatientContainer');
    const walkinContainer = document.getElementById('walkinPatientContainer');

    function togglePatientFields() {
        const selected = document.querySelector('input[name="patientType"]:checked')?.value || 'registered';
        if (selected === 'registered') {
            registeredContainer.classList.remove('d-none');
            walkinContainer.classList.add('d-none');
        } else {
            registeredContainer.classList.add('d-none');
            walkinContainer.classList.remove('d-none');
        }
    }

    document.querySelectorAll('input[name="patientType"]').forEach(r => r.addEventListener('change', togglePatientFields));
    togglePatientFields();

    // ======================
    // Form Submission
    // ======================
    const form = document.getElementById("newAppointmentForm");
    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const patientType = document.querySelector('input[name="patientType"]:checked').value;
        const title = document.getElementById("title").value.trim();
        const startDate = document.getElementById("startDate").value;
        const endDate = document.getElementById("endDate").value;

        let patientId = null;
        let walkInName = null;

        if (patientType === "registered") {
            // ⚠️ Make sure your dropdown has id="patientId"
            patientId = document.getElementById("patientId").value;
            if (!patientId) {
                Swal.fire("⚠️ Error", "Please select a registered patient.", "error");
                return;
            }
        } else {
            const first = document.getElementById("walkinFirstName").value.trim();
            const last = document.getElementById("walkinLastName").value.trim();
            if (!first || !last) {
                Swal.fire("⚠️ Error", "Please enter both first and last name for walk-in patient.", "error");
                return;
            }
            walkInName = first + " " + last;
        }

        if (!title || !startDate || !endDate) {
            Swal.fire("⚠️ Error", "Please fill in all required fields.", "error");
            return;
        }

        Swal.fire({
            title: "Confirm Appointment",
            html: `
            <p><b>Patient:</b> ${patientType === "registered" ? patientId : walkInName}</p>
            <p><b>Title:</b> ${title}</p>
            <p><b>Start:</b> ${startDate}</p>
            <p><b>End:</b> ${endDate}</p>
        `,
            width: 400, // smaller message box
            icon: "info",
            showCancelButton: true,
            confirmButtonText: "Confirm & Save",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                fetch("/Doctor/CreateAppointment", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']")?.value
                    },
                    body: JSON.stringify({
                        title,
                        startDate,
                        endDate,
                        patientId,
                        walkInName
                    })
                })
                    .then(r => {
                        if (!r.ok) throw new Error("Failed to save");
                        return r.json();
                    })
                    .then(data => {
                        Swal.fire("✅ Success", "Appointment registered!", "success");
                        bootstrap.Modal.getInstance(document.getElementById("newAppointmentModal")).hide();
                        location.reload();
                    })
                    .catch(err => Swal.fire("❌ Error", err.message, "error"));
            }
        });
    });
});