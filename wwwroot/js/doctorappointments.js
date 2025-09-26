document.addEventListener('DOMContentLoaded', function () {
    // -------------------------
    // Calendar Initialization
    // -------------------------
    const appointments = window.doctorAppointments || [];
    console.log('FullCalendar events:', appointments); // Debug: make sure data is present

    const events = appointments.map(a => ({
        id: a.id,
        title: `${a.title} - ${a.patientName || a.walkInName}`,
        start: a.startDate,
        end: a.endDate,
        extendedProps: {
            patient: a.patientName || a.walkInName,
            status: a.status || 'Scheduled'
        }
    }));

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
            events: events,
            select: function (info) {
                document.getElementById('startDate').value = info.startStr.slice(0, 16);
                document.getElementById('endDate').value = info.endStr ? info.endStr.slice(0, 16) : '';
                const modalEl = document.getElementById('newAppointmentModal');
                if (modalEl && bootstrap?.Modal) new bootstrap.Modal(modalEl).show();
            },
            eventClick: function (info) {
                alert(`👤 Appointment: ${info.event.title}\nPatient: ${info.event.extendedProps?.patient || ''}\nStatus: ${info.event.extendedProps?.status || ''}`);
            }
        });

        calendar.render();

        // Responsive
        window.addEventListener('resize', () => {
            calendar.setOption('height', getCalendarHeight());
            calendar.setOption('headerToolbar', { left: 'prev,next today', center: 'title', right: getRightToolbar() });
            calendar.updateSize();
        });

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

    // -------------------------
    // Patient Type Toggle
    // -------------------------
    const registeredContainer = document.getElementById('registeredPatientContainer');
    const walkinContainer = document.getElementById('walkinPatientContainer');
    const patientInput = document.getElementById('patientName');
    const firstNameInput = document.getElementById('walkinFirstName');
    const lastNameInput = document.getElementById('walkinLastName');
    const patientDropdown = document.getElementById('patientDropdown');

    function togglePatientFields() {
        const selected = document.querySelector('input[name="patientType"]:checked').value;

        if (selected === 'registered') {
            registeredContainer.classList.remove('d-none');
            walkinContainer.classList.add('d-none');
            patientInput.readOnly = false;
            firstNameInput.readOnly = true;
            lastNameInput.readOnly = true;
            firstNameInput.value = '';
            lastNameInput.value = '';
        } else {
            registeredContainer.classList.add('d-none');
            walkinContainer.classList.remove('d-none');
            patientInput.readOnly = true;
            patientInput.value = '';
            firstNameInput.readOnly = false;
            lastNameInput.readOnly = false;
        }
    }

    document.querySelectorAll('input[name="patientType"]').forEach(r => r.addEventListener('change', togglePatientFields));
    togglePatientFields();

    // -------------------------
    // Patient Autocomplete
    // -------------------------
    let debounceTimer;
    function debounce(func, delay) {
        return (...args) => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => func.apply(this, args), delay);
        };
    }

    async function searchPatients(query) {
        if (!query || query.length < 2) {
            patientDropdown.classList.remove('show');
            return;
        }
        try {
            const res = await fetch('/Doctor/SearchPatients?query=' + encodeURIComponent(query));
            if (!res.ok) return;
            const list = await res.json();
            patientDropdown.innerHTML = '';
            if (Array.isArray(list) && list.length) {
                list.forEach(p => {
                    const item = document.createElement('button');
                    item.type = 'button';
                    item.className = 'dropdown-item';
                    item.innerHTML = `<strong>${p.name}</strong> <small class="text-muted">${p.contact || ''}</small>`;
                    item.addEventListener('click', () => {
                        patientInput.value = p.name;
                        patientInput.dataset.patientId = p.id;
                        patientDropdown.classList.remove('show');
                    });
                    patientDropdown.appendChild(item);
                });
                patientDropdown.classList.add('show');
            } else patientDropdown.classList.remove('show');
        } catch (err) {
            console.error(err);
        }
    }

    patientInput.addEventListener('input', debounce(e => searchPatients(e.target.value.trim()), 250));
    document.addEventListener('click', e => {
        if (!patientInput.contains(e.target) && !patientDropdown.contains(e.target))
            patientDropdown.classList.remove('show');
    });

    // -------------------------
    // Form Submit
    // -------------------------
    const form = document.getElementById('newAppointmentForm');
    form.addEventListener('submit', async e => {
        e.preventDefault();

        const patientType = document.querySelector('input[name="patientType"]:checked').value;
        let patientId = null, walkinName = null;

        if (patientType === 'registered') {
            patientId = patientInput.dataset.patientId || null;
            walkinName = null; // registered patient, use patientId
        } else {
            walkinName = `${firstNameInput.value.trim()} ${lastNameInput.value.trim()}`;
        }

        const title = document.getElementById('title').value.trim();
        const startDate = document.getElementById('startDate').value;
        const endDate = document.getElementById('endDate').value;

        const payload = { PatientId: patientId, WalkInName: walkinName, Title: title, StartDate: startDate, EndDate: endDate };

        try {
            const res = await fetch('/Doctor/CreateAppointment', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (!res.ok) throw new Error('Server error');
            const data = await res.json();

            const displayName = walkinName || data.patientName;

            window.doctorCalendar?.addEvent({
                id: data.id || String(Math.random()),
                title: `${title} - ${displayName}`,
                start: startDate,
                end: endDate,
                extendedProps: { patient: displayName, status: data.status || 'Scheduled' }
            });

            form.reset();
            delete patientInput.dataset.patientId;
            togglePatientFields();
            bootstrap.Modal.getInstance(document.getElementById('newAppointmentModal'))?.hide();
        } catch (err) {
            console.error(err);
            alert('Failed to create appointment.');
        }
    });
});
