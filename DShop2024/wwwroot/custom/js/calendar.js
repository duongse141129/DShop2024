/* =======================
   GLOBAL ELEMENTS
======================= */
const calendar = document.querySelector(".calendar");
const dateEl = document.querySelector(".date");
const daysContainer = document.querySelector(".days");
const prevBtn = document.querySelector(".prev");
const nextBtn = document.querySelector(".next");
const todayBtn = document.querySelector(".today-btn");
const gotoBtn = document.querySelector(".goto-btn");
const dateInput = document.querySelector(".date-input");

const eventDay = document.querySelector(".event-day");
const eventDate = document.querySelector(".event-date");
const eventsContainer = document.querySelector(".events");

const addEventBtn = document.querySelector(".add-event");
const addEventWrapper = document.querySelector(".add-event-wrapper");
const addEventCloseBtn = document.querySelector(".close");
const addEventTitle = document.querySelector(".event-name");
const addEventFrom = document.querySelector(".event-time-from");
const addEventTo = document.querySelector(".event-time-to");
const addEventSubmit = document.querySelector(".add-event-btn");


let dateString
/* =======================
   DATE STATE
======================= */
let today = new Date();
let activeDay = today.getDate();
let month = today.getMonth();
let year = today.getFullYear();
/* =======================
   DATE RANGE STATE
======================= */
/*let startDate = null; // Format: { day, month, year }*/
let startDate = {
    day: activeDay,
    month: month,
    year: year,
    timestamp: new Date(year, month, activeDay).getTime()
};
let endDate = null;   // Format: { day, month, year }


const months = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
];

/* =======================
   TASK DATA (FROM SERVER)
======================= */
const eventsArr = [];

/* =======================
   INIT
======================= */
loadTasks();

/* =======================
   LOAD TASKS FROM BACKEND
======================= */
function loadTasks() {
    fetch("/Admin/Assignment/calendar")
        .then(res => res.json())
        .then(data => {
            eventsArr.length = 0;

            data.forEach(t => {
                let dayBlock = eventsArr.find(e =>
                    e.day === t.day &&
                    e.month === t.month &&
                    e.year === t.year
                );

                if (!dayBlock) {
                    dayBlock = {
                        day: t.day,
                        month: t.month,
                        year: t.year,
                        events: []
                    };
                    eventsArr.push(dayBlock);
                }

                dayBlock.events.push({
                    id: t.id,
                    title: t.title,
                    time: t.time,
                    isAllowUpdateDelete: t.isAllowUpdateDelete,
                    status: t.status
                });
            });

            initCalendar();
            updateHeader(activeDay);
            updateEvents(activeDay);
        });
}

/* =======================
   CALENDAR RENDER
======================= */
function initCalendar() {
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const prevLastDay = new Date(year, month, 0);

    dateEl.innerHTML = `${months[month]} ${year}`;

    let days = "";

    for (let i = firstDay.getDay(); i > 0; i--) {
        days += `<div class="day prev-date">${prevLastDay.getDate() - i + 1}</div>`;
    }

    for (let i = 1; i <= lastDay.getDate(); i++) {
        const currentTimestamp = new Date(year, month, i).getTime();

        const hasEvent = eventsArr.some(e =>
             e.day === i && e.month === month + 1 && e.year === year
        );

        const isToday =
            i === today.getDate() &&
            month === today.getMonth() &&
            year === today.getFullYear();

        let rangeClass = "";
        if (startDate && i === startDate.day && month === startDate.month && year === startDate.year) {
            rangeClass = "active start-date"; 
        } else if (endDate && i === endDate.day && month === endDate.month && year === endDate.year) {
            rangeClass = "active end-date";
        } else if (startDate && endDate && currentTimestamp > startDate.timestamp && currentTimestamp < endDate.timestamp) {
            rangeClass = "in-range";
        }

        const classes = [
            "day",
            isToday ? "today" : "", 
            rangeClass,
            hasEvent ? "event" : "" 
        ].join(" ");

        days += `<div class="${classes}">${i}</div>`;
    }

    const nextDays = 7 - lastDay.getDay() - 1;
    for (let i = 1; i <= nextDays; i++) {
        days += `<div class="day next-date">${i}</div>`;
    }

    daysContainer.innerHTML = days;
    addDayListeners();
}

/* =======================
   DAY CLICK
======================= */


function addDayListeners() {
    document.querySelectorAll(".day").forEach(day => {
        day.addEventListener("click", e => {
            const clickedDay = Number(day.innerText);
            const clickedDate = new Date(year, month, clickedDay).getTime();

            activeDay = clickedDay;
            updateEvents(activeDay);
            updateHeader(activeDay);

            // 2. Shift + Click Logic for Range
            if (e.shiftKey) {
                if (!startDate || (startDate && endDate)) {
                    // Start a new range if none exists or one just finished
                    startDate = { day: clickedDay, month, year, timestamp: clickedDate };
                    endDate = null;
                } else {
                    // Complete the range
                    if (clickedDate < startDate.timestamp) {
                        // Swap if the second click is earlier than the first
                        endDate = { ...startDate };
                        startDate = { day: clickedDay, month, year, timestamp: clickedDate };
                    } else {
                        endDate = { day: clickedDay, month, year, timestamp: clickedDate };
                    }
                }
            } else {
                // Regular Click: Reset selection to a single day
                startDate = { day: clickedDay, month, year, timestamp: clickedDate };
                endDate = null;
            }

            initCalendar(); 
        });
    });
}


/* =======================
   UPDATE HEADER
======================= */
function updateHeader(day) {
    const d = new Date(year, month, day);
    eventDay.innerHTML = d.toDateString().split(" ")[0];
    eventDate.innerHTML = `${day} ${months[month]} ${year}`;
}

/* =======================
   UPDATE EVENTS PANEL
======================= */
function updateEvents(day) {
    let html = "";

    const dayEvents = eventsArr.find(e =>
        e.day === day &&
        e.month === month + 1 &&
        e.year === year
    );

    if (!dayEvents || dayEvents.events.length === 0) {
        html = `<div class="no-event"><h3>No Tasks</h3></div>`;
    } else {
        dayEvents.events.forEach(ev => {
            html += `
        <div class="event" data-id="${ev.id}">
            <div class="event-content">
              <div class="title" >
                 <i class="fas fa-circle"></i>        
                 <h3 class="event-title" data-id="${ev.id}"  >${ev.title}</h3>                
              </div>
              <span class="event-time">
                  <i class="far fa-clock"></i> ${ev.time}
              </span>
            </div>
            <div class="event-status">
                <span id="event-task-${ev.id}" class="status-task-${ev.status}">${ev.status} </span>
            </div>

            <div class="event-actions">
                    ${ev.isAllowUpdateDelete == true ? `
                    <button type="button" class="btn-icon btn-task-edit" onclick="openModal('/Admin/Assignment/Edit/${ev.id}')" >
                        <i class="far fa-edit"></i>
                    </button>

                    <button type="button" class="btn-icon btn-task-delete"
                            onclick="openModal('/Admin/Assignment/Delete/${ev.id}')">
                      <i class="fas fa-trash-alt"></i>
                    </button>
                    ` : ''}
            </div>
        </div>`;
        });
    }

    eventsContainer.innerHTML = html;
}



/* =======================
   MONTH NAVIGATION
======================= */
prevBtn.onclick = () => {  prevMonth(); }
nextBtn.onclick = () => {  nextMonth(); }

function prevMonth() {
    month--;
    if (month < 0) {
        month = 11;
        year--;
    }
    activeDay = 1;
    startDate = { day: 1, month, year, timestamp: new Date(year, month, 1).getTime() };
    endDate = null;
    loadTasks();
}

function nextMonth() {
    month++;
    if (month > 11) {
        month = 0;
        year++;
    }
    activeDay = 1;
    startDate = { day: 1, month, year, timestamp: new Date(year, month, 1).getTime() };
    endDate = null;
    loadTasks();
}


/* =======================
   ADD TASK WITH RANGE
======================= */
function openCreateAssignment() {
    if (!startDate) {
        alert("Please select at least a start date.");
        return;
    }

    const startStr = `${startDate.year}-${startDate.month + 1}-${startDate.day}`;

    const endStr = endDate
        ? `${endDate.year}-${endDate.month + 1}-${endDate.day}`
        : startStr;

    openModal(`/Admin/Assignment/Create?startDate=${startStr}&endDate=${endStr}`);
}

/* =======================
   DELETE TASK WITH RANGE
======================= */
function openDeleteAllAssignment() {
    if (!startDate) {
        alert("Please select at least a start date.");
        return;
    }

    const startStr = `${startDate.year}-${startDate.month + 1}-${startDate.day}`;

    const endStr = endDate
        ? `${endDate.year}-${endDate.month + 1}-${endDate.day}`
        : startStr;

    openModal(`/Admin/Assignment/DeleteAll?startDate=${startStr}&endDate=${endStr}`);
}



/* =======================
   Details TASK
======================= */
eventsContainer.onclick = e => {
    const eventEl = e.target.closest(".event");
    if (!eventEl) return;
    const id = eventEl.dataset.id;
    openModal(`/Admin/Assignment/Details/${id}`);
};

/* =======================
   ToDay
======================= */

todayBtn.onclick = () => {
    today = new Date();
    month = today.getMonth();
    year = today.getFullYear();
    activeDay = today.getDate();

    startDate = {
        day: activeDay,
        month: month,
        year: year,
        timestamp: new Date(year, month, activeDay).getTime()
    };
    endDate = null;

    loadTasks();
};



/* =======================
   GOTO DATE
======================= */
dateInput.addEventListener("input", (e) => {
    dateInput.value = dateInput.value.replace(/[^0-9/]/g, "");
    if (dateInput.value.length === 2) {
        dateInput.value += "/";
    }
    if (dateInput.value.length > 7) {
        dateInput.value = dateInput.value.slice(0, 7);
    }
    if (e.inputType === "deleteContentBackward") {
        if (dateInput.value.length === 3) {
            dateInput.value = dateInput.value.slice(0, 2);
        }
    }
});

gotoBtn.addEventListener("click", gotoDate);

function gotoDate() {
    const dateArr = dateInput.value.split("/");

    if (dateArr.length !== 2) {
        alert("Invalid Date");
        return;
    }

    const inputMonth = Number(dateArr[0]);
    const inputYear = Number(dateArr[1]);

    if (inputMonth < 1 || inputMonth > 12 || inputYear < 1900) {
        alert("Invalid Date");
        return;
    }

    month = inputMonth - 1;
    year = inputYear;
    activeDay = 1; 

    startDate = {
        day: activeDay,
        month: month,
        year: year,
        timestamp: new Date(year, month, activeDay).getTime()
    };
    endDate = null; 

    loadTasks();
}


/* =======================
   Get/ Set active day
======================= */
function getActiveDay(date) {
    const day = new Date(year, month, date);
    const dayName = day.toString().split(" ")[0];
    eventDay.innerHTML = dayName;
    eventDate.innerHTML = date + " " + months[month] + " " + year;
}


function setActiveDay(dateString) {
    const date = new Date(dateString);

    if (isNaN(date.getTime())) {
        console.error("Invalid date string provided to setActiveDay");      
        return;
    }
    year = date.getFullYear();
    month = date.getMonth();
    activeDay = date.getDate();

    loadTasks();
}

$('.clockpicker').clockpicker({
    afterShow: function () {
        $('.popover').css('z-index', 2051);
    }
});


function handleRangeSelection(day, month, year) {
    const clickedDate = new Date(year, month, day);

    if (!startDate || (startDate && endDate)) {
        startDate = { day, month, year, timestamp: clickedDate.getTime() };
        endDate = null;
    } else {
        if (clickedDate.getTime() < startDate.timestamp) {
            startDate = { day, month, year, timestamp: clickedDate.getTime() };
        } else {
            endDate = { day, month, year, timestamp: clickedDate.getTime() };
        }
    }
    initCalendar(); 
}


