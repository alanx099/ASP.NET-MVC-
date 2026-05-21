(function () {
  "use strict";

  const isoPattern = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})(?::(\d{2}))?/;

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }

  function init() {
    document.querySelectorAll("[data-gc-datetime]").forEach(enhancePicker);
    formatDateOutputs();
  }

  function enhancePicker(root) {
    const display = root.querySelector("[data-gc-datetime-display]");
    const valueInput = root.querySelector("[data-gc-datetime-value]");
    const toggle = root.querySelector("[data-gc-datetime-toggle]");
    const popover = root.querySelector("[data-gc-datetime-popover]");
    const monthLabel = root.querySelector("[data-gc-datetime-month]");
    const weekdays = root.querySelector("[data-gc-datetime-weekdays]");
    const days = root.querySelector("[data-gc-datetime-days]");
    const hourInput = root.querySelector("[data-gc-datetime-hour]");
    const minuteInput = root.querySelector("[data-gc-datetime-minute]");

    if (!display || !valueInput || !toggle || !popover || !monthLabel || !weekdays || !days || !hourInput || !minuteInput) {
      return;
    }

    const state = {
      selected: parseDate(valueInput.value) || new Date(),
      view: startOfMonth(parseDate(valueInput.value) || new Date())
    };

    syncInputs(state, display, valueInput, hourInput, minuteInput);
    renderWeekdays(weekdays);
    render(root, state);

    toggle.addEventListener("click", function () {
      setOpen(root, popover, popover.getAttribute("aria-hidden") === "true");
    });

    display.addEventListener("focus", function () {
      setOpen(root, popover, true);
    });

    display.addEventListener("change", function () {
      const parsed = parseDate(display.value);
      if (!parsed) {
        valueInput.value = "";
        valueInput.dispatchEvent(new Event("change", { bubbles: true }));
        return;
      }

      state.selected = parsed;
      state.view = startOfMonth(parsed);
      syncInputs(state, display, valueInput, hourInput, minuteInput);
      render(root, state);
    });

    display.addEventListener("blur", function () {
      valueInput.dispatchEvent(new Event("blur", { bubbles: true }));
      valueInput.dispatchEvent(new Event("focusout", { bubbles: true }));
    });

    root.querySelector("[data-gc-datetime-prev]").addEventListener("click", function () {
      state.view = new Date(state.view.getFullYear(), state.view.getMonth() - 1, 1);
      render(root, state);
    });

    root.querySelector("[data-gc-datetime-next]").addEventListener("click", function () {
      state.view = new Date(state.view.getFullYear(), state.view.getMonth() + 1, 1);
      render(root, state);
    });

    root.querySelector("[data-gc-datetime-today]").addEventListener("click", function () {
      state.selected = new Date();
      state.view = startOfMonth(state.selected);
      syncInputs(state, display, valueInput, hourInput, minuteInput);
      render(root, state);
    });

    root.querySelector("[data-gc-datetime-apply]").addEventListener("click", function () {
      applyTime(state, hourInput, minuteInput);
      syncInputs(state, display, valueInput, hourInput, minuteInput);
      setOpen(root, popover, false);
    });

    [hourInput, minuteInput].forEach(function (input) {
      input.addEventListener("change", function () {
        applyTime(state, hourInput, minuteInput);
        syncInputs(state, display, valueInput, hourInput, minuteInput);
      });
    });

    document.addEventListener("click", function (event) {
      if (!root.contains(event.target)) {
        setOpen(root, popover, false);
      }
    });

    root.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        setOpen(root, popover, false);
        display.focus();
      }
    });

    function render(pickerRoot, pickerState) {
      monthLabel.textContent = new Intl.DateTimeFormat(currentLocale(), { month: "long", year: "numeric" }).format(pickerState.view);
      days.innerHTML = "";

      const firstDay = new Date(pickerState.view.getFullYear(), pickerState.view.getMonth(), 1);
      const offset = firstDay.getDay() === 0 ? 6 : firstDay.getDay() - 1;
      const start = new Date(firstDay);
      start.setDate(firstDay.getDate() - offset);

      for (let i = 0; i < 42; i += 1) {
        const day = new Date(start);
        day.setDate(start.getDate() + i);

        const button = document.createElement("button");
        button.type = "button";
        button.className = "gc-datetime__day";
        button.textContent = day.getDate().toString();
        button.dataset.outside = day.getMonth() === pickerState.view.getMonth() ? "false" : "true";
        button.dataset.selected = sameDate(day, pickerState.selected) ? "true" : "false";
        button.addEventListener("click", function () {
          pickerState.selected = new Date(day.getFullYear(), day.getMonth(), day.getDate(), pickerState.selected.getHours(), pickerState.selected.getMinutes(), 0);
          pickerState.view = startOfMonth(pickerState.selected);
          syncInputs(pickerState, display, valueInput, hourInput, minuteInput);
          render(pickerRoot, pickerState);
        });

        days.appendChild(button);
      }
    }
  }

  function renderWeekdays(container) {
    const base = new Date(2024, 0, 1);
    container.innerHTML = "";

    for (let i = 0; i < 7; i += 1) {
      const date = new Date(base);
      date.setDate(base.getDate() + i);
      const span = document.createElement("span");
      span.textContent = new Intl.DateTimeFormat(currentLocale(), { weekday: "short" }).format(date);
      container.appendChild(span);
    }
  }

  function formatDateOutputs() {
    document.querySelectorAll("[data-gc-date-display]").forEach(function (element) {
      const value = element.getAttribute("datetime") || element.dataset.gcDateDisplay || element.textContent;
      const parsed = parseDate(value);
      if (parsed) {
        element.textContent = formatForBrowser(parsed);
      }
    });
  }

  function syncInputs(state, display, valueInput, hourInput, minuteInput) {
    display.value = formatForBrowser(state.selected);
    valueInput.value = formatForServer(state.selected);
    valueInput.dispatchEvent(new Event("change", { bubbles: true }));
    hourInput.value = pad(state.selected.getHours());
    minuteInput.value = pad(state.selected.getMinutes());
  }

  function applyTime(state, hourInput, minuteInput) {
    const hour = clamp(parseInt(hourInput.value, 10), 0, 23);
    const minute = clamp(parseInt(minuteInput.value, 10), 0, 59);
    state.selected.setHours(hour, minute, 0, 0);
  }

  function setOpen(root, popover, isOpen) {
    popover.setAttribute("aria-hidden", isOpen ? "false" : "true");
    root.dataset.open = isOpen ? "true" : "false";
  }

  function parseDate(value) {
    if (!value || !value.trim()) {
      return null;
    }

    const trimmed = value.trim();
    const iso = trimmed.match(isoPattern);
    if (iso) {
      return new Date(Number(iso[1]), Number(iso[2]) - 1, Number(iso[3]), Number(iso[4]), Number(iso[5]), Number(iso[6] || 0));
    }

    const hr = trimmed.match(/^(\d{1,2})\.(\d{1,2})\.(\d{4})\.?\s+(\d{1,2}):(\d{2})$/);
    if (hr) {
      return new Date(Number(hr[3]), Number(hr[2]) - 1, Number(hr[1]), Number(hr[4]), Number(hr[5]), 0);
    }

    const en = trimmed.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4}),?\s+(\d{1,2}):(\d{2})(?:\s*(AM|PM))?$/i);
    if (en) {
      let hour = Number(en[4]);
      const marker = (en[6] || "").toUpperCase();
      if (marker === "PM" && hour < 12) {
        hour += 12;
      } else if (marker === "AM" && hour === 12) {
        hour = 0;
      }
      return new Date(Number(en[3]), Number(en[1]) - 1, Number(en[2]), hour, Number(en[5]), 0);
    }

    const parsed = new Date(trimmed);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
  }

  function formatForBrowser(date) {
    return new Intl.DateTimeFormat(currentLocale(), {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit"
    }).format(date);
  }

  function formatForServer(date) {
    return [
      date.getFullYear(),
      "-",
      pad(date.getMonth() + 1),
      "-",
      pad(date.getDate()),
      "T",
      pad(date.getHours()),
      ":",
      pad(date.getMinutes()),
      ":00"
    ].join("");
  }

  function currentLocale() {
    const language = navigator.language || "en-US";
    return language.toLowerCase().startsWith("hr") ? "hr-HR" : "en-US";
  }

  function startOfMonth(date) {
    return new Date(date.getFullYear(), date.getMonth(), 1);
  }

  function sameDate(a, b) {
    return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
  }

  function clamp(value, min, max) {
    if (Number.isNaN(value)) {
      return min;
    }

    return Math.min(max, Math.max(min, value));
  }

  function pad(value) {
    return value.toString().padStart(2, "0");
  }
})();
