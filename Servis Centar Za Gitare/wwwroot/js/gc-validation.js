(function () {
  "use strict";

  document.addEventListener("submit", function (event) {
    const form = event.target;
    if (!(form instanceof HTMLFormElement) || !form.matches("form.gc-form")) {
      return;
    }

    const errors = collectFrontendErrors(form);
    if (errors.length > 0) {
      event.preventDefault();
      event.stopImmediatePropagation();
      showValidationErrors(form, errors, null);
    }
  }, true);

  if (!window.jQuery || !window.jQuery.validator) {
    return;
  }

  const $ = window.jQuery;

  $.validator.addMethod("phone", function (value, element) {
    if (this.optional(element)) {
      return true;
    }

    const digits = value.replace(/\D/g, "");
    return digits.length >= 6 && /^[+]?[\d\s().-]+$/.test(value);
  });

  $.validator.addMethod("gcmaxfuturedays", function (value, element, days) {
    const date = parseDateTime(value);
    if (!date) {
      return this.optional(element);
    }

    const maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + Number(days || 0));
    return date <= maxDate;
  });

  $.validator.addMethod("gcafterorequalto", function (value, element, otherSelector) {
    const date = parseDateTime(value);
    const otherElement = document.querySelector(otherSelector);
    const otherDate = parseDateTime(otherElement ? otherElement.value : "");

    if (!date) {
      return this.optional(element);
    }

    if (!otherDate) {
      return true;
    }

    return date >= otherDate;
  });

  if ($.validator.unobtrusive) {
    $.validator.unobtrusive.adapters.addSingleVal("gcmaxfuturedays", "days");
    $.validator.unobtrusive.adapters.addSingleVal("gcafterorequalto", "other");
  }

  $.validator.setDefaults({
    ignore: ":hidden:not([data-gc-datetime-value]):not([data-gc-autocomplete-value]):not(#knowledge-hidden-select)",
    errorElement: "span",
    errorClass: "invalid-feedback",
    validClass: "is-valid",
    focusInvalid: false,
    onfocusout: function (element) {
      this.element(element);
    },
    onkeyup: false,
    onclick: function (element) {
      this.element(element);
    },
    highlight: function (element) {
      setValidationState(element, true);
    },
    unhighlight: function (element) {
      setValidationState(element, false);
    }
  });

  $(function () {
    $("form").each(function () {
      const $form = $(this);
      $form.removeData("validator");
      $form.removeData("unobtrusiveValidation");
      $.validator.unobtrusive.parse($form);
    });

    $("form.gc-form").each(function () {
      const form = this;
      const validator = $(form).data("validator");
      if (validator && hasServerValidationErrors(form)) {
        window.setTimeout(function () {
          showValidationModal(form, validator);
        }, 0);
        return false;
      }

      return true;
    });

    $(document).on("blur focusout change", "[data-gc-datetime-display]", function () {
      const hiddenInput = this.closest("[data-gc-datetime]")?.querySelector("[data-gc-datetime-value]");
      if (hiddenInput) {
        $(hiddenInput).valid();
      }
    });

    $(document).on("blur focusout change", "input[data-val='true'], select[data-val='true'], textarea[data-val='true']", function () {
      validateField(this);
      validateDependentFields(this);
    });

    document.addEventListener("focusout", function (event) {
      const target = event.target;
      if (target instanceof HTMLInputElement ||
          target instanceof HTMLSelectElement ||
          target instanceof HTMLTextAreaElement) {
        validateField(target);
      }
    }, true);

    $(document).on("change gc:autocomplete-selected", "[data-gc-autocomplete-value], .gc-select__native", function () {
      validateField(this);
    });

    $(document).on("submit", "form.gc-form", function (event) {
      const form = this;
      const validator = $(form).data("validator");

      if (!validator) {
        return;
      }

      if (!$(form).valid()) {
        event.preventDefault();
        event.stopImmediatePropagation();
        showValidationErrors(form, collectJQueryValidationErrors(form, validator), validator);
      }
    });
  });

  function validateField(element) {
    const $element = $(element);
    const form = element.form;

    if (!form || !$element.attr("name")) {
      return;
    }

    const validator = $(form).data("validator");
    if (validator && $element.attr("data-val") === "true") {
      validator.element(element);
    }
  }

  function validateDependentFields(element) {
    if (!element.id) {
      return;
    }

    const selector = "[data-val-gcafterorequalto-other='#" + cssEscape(element.id) + "']";
    document.querySelectorAll(selector).forEach(function (dependent) {
      validateField(dependent);
    });
  }

  function setValidationState(element, isInvalid) {
    const input = $(element);
    input.toggleClass("is-invalid", isInvalid);
    input.toggleClass("is-valid", !isInvalid);

    const datePicker = element.closest("[data-gc-datetime]");
    if (datePicker) {
      datePicker.classList.toggle("is-invalid", isInvalid);
      datePicker.classList.toggle("is-valid", !isInvalid);
      const display = datePicker.querySelector("[data-gc-datetime-display]");
      if (display) {
        display.classList.toggle("is-invalid", isInvalid);
        display.classList.toggle("is-valid", !isInvalid);
      }
    }

    const select = element.closest(".gc-select, .gc-autocomplete");
    if (select) {
      select.classList.toggle("is-invalid", isInvalid);
      select.classList.toggle("is-valid", !isInvalid);
    }
  }

  function showValidationModal(form, validator) {
    showValidationErrors(form, collectJQueryValidationErrors(form, validator), validator);
  }

  function showValidationErrors(form, errors, validator) {
    const modal = getValidationModal();
    const list = modal.querySelector("[data-validation-list]");
    const intro = modal.querySelector("[data-validation-intro]");

    list.innerHTML = "";
    errors.forEach(function (error) {
      const item = document.createElement("li");
      item.textContent = error.label ? error.label + ": " + error.message : error.message;
      list.appendChild(item);
    });

    if (intro) {
      intro.textContent = errors.length === 1
        ? "Fix this field before saving."
        : "Fix these fields before saving.";
    }

    modal.setAttribute("aria-hidden", "false");
    modal.querySelector("[data-validation-close]")?.focus();

    const closeButton = modal.querySelector("[data-validation-close]");
    const closeHandler = function () {
      modal.setAttribute("aria-hidden", "true");
      focusFirstInvalidField(form, validator, errors[0]?.element);
      closeButton?.removeEventListener("click", closeHandler);
    };

    closeButton?.addEventListener("click", closeHandler);
  }

  function getValidationModal() {
    const existing = document.getElementById("gc-validation-modal");
    if (existing) {
      return existing;
    }

    const modal = document.createElement("div");
    modal.className = "gc-modal";
    modal.id = "gc-validation-modal";
    modal.setAttribute("aria-hidden", "true");
    modal.setAttribute("role", "dialog");
    modal.setAttribute("aria-modal", "true");
    modal.setAttribute("aria-labelledby", "gc-validation-modal-title");
    modal.innerHTML =
      '<div class="gc-modal__panel">' +
        '<p class="section-label">Form validation</p>' +
        '<h3 class="gc-modal__title" id="gc-validation-modal-title">Check the highlighted fields</h3>' +
        '<p class="section-copy" data-validation-intro>Fix these fields before saving.</p>' +
        '<ul class="gc-validation-list" data-validation-list></ul>' +
        '<div class="gc-actions">' +
          '<button type="button" class="btn gc-btn-primary" data-validation-close>Review form</button>' +
        '</div>' +
      '</div>';

    modal.addEventListener("click", function (event) {
      if (event.target === modal) {
        modal.setAttribute("aria-hidden", "true");
      }
    });

    modal.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        modal.setAttribute("aria-hidden", "true");
      }
    });

    document.body.appendChild(modal);
    return modal;
  }

  function collectJQueryValidationErrors(form, validator) {
    const seen = new Set();
    const errors = [];

    validator.errorList.forEach(function (error) {
      const message = normalizeMessage(error.message);
      const key = error.element.name + message;
      if (!message || seen.has(key)) {
        return;
      }

      seen.add(key);
      errors.push({
        label: getFieldLabel(form, error.element),
        message,
        element: error.element
      });
    });

    if (errors.length > 0) {
      return errors;
    }

    form.querySelectorAll(".field-validation-error, .invalid-feedback").forEach(function (messageEl) {
      const message = normalizeMessage(messageEl.textContent);
      if (!message || seen.has(message)) {
        return;
      }

      seen.add(message);
      errors.push({ label: "", message });
    });

    return errors.length > 0
      ? errors
      : [{ label: "", message: "Some fields are not valid. Review the highlighted inputs." }];
  }

  function hasServerValidationErrors(form) {
    return Array.from(form.querySelectorAll(".field-validation-error, .invalid-feedback"))
      .some(function (messageEl) {
        return normalizeMessage(messageEl.textContent).length > 0;
      });
  }

  function collectFrontendErrors(form) {
    const errors = [];
    const seen = new Set();

    form.querySelectorAll("input, select, textarea").forEach(function (element) {
      const error = validateFrontendField(form, element);
      if (!error) {
        clearManualValidationMessage(element);
        setManualValidationState(element, false);
        return;
      }

      const key = element.name + error;
      if (seen.has(key)) {
        return;
      }

      seen.add(key);
      setManualValidationMessage(element, error);
      setManualValidationState(element, true);
      errors.push({
        label: getFieldLabel(form, element),
        message: error,
        element
      });
    });

    return errors;
  }

  function validateFrontendField(form, element) {
    if (!element.name || element.disabled || element.type === "submit" || element.type === "button") {
      return "";
    }

    if (element.closest("#new-guitar-section") && !form.querySelector("#add-guitar-toggle")?.checked) {
      return "";
    }

    const value = (element.value || "").trim();
    const requiredMessage = requiredMessageFor(form, element);
    if (requiredMessage && !value) {
      return requiredMessage;
    }

    if (!value) {
      return "";
    }

    const maxLength = Number(element.getAttribute("data-val-length-max") || element.getAttribute("maxlength") || 0);
    if (maxLength > 0 && value.length > maxLength) {
      return element.getAttribute("data-val-length") || "Value is too long.";
    }

    const pattern = element.getAttribute("data-val-regex-pattern");
    if (pattern && !(new RegExp(pattern).test(value))) {
      return element.getAttribute("data-val-regex") || "Value is not valid.";
    }

    if (element.getAttribute("data-val-email") && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
      return element.getAttribute("data-val-email") || "Enter a valid email address.";
    }

    if (element.getAttribute("data-val-phone")) {
      const digits = value.replace(/\D/g, "");
      if (digits.length < 6 || !/^[+]?[\d\s().-]+$/.test(value)) {
        return element.getAttribute("data-val-phone") || "Enter a valid phone number.";
      }
    }

    const rangeMin = element.getAttribute("data-val-range-min");
    const rangeMax = element.getAttribute("data-val-range-max");
    if ((rangeMin || rangeMax) && !isInRange(value, rangeMin, rangeMax)) {
      return element.getAttribute("data-val-range") || "Value is out of range.";
    }

    const maxFutureDays = element.getAttribute("data-val-gcmaxfuturedays-days");
    if (maxFutureDays) {
      const date = parseDateTime(value);
      const maxDate = new Date();
      maxDate.setDate(maxDate.getDate() + Number(maxFutureDays));
      if (!date || date > maxDate) {
        return element.getAttribute("data-val-gcmaxfuturedays") || "Date is too far in the future.";
      }
    }

    const notBeforeSelector = element.getAttribute("data-val-gcafterorequalto-other");
    if (notBeforeSelector) {
      const date = parseDateTime(value);
      const other = document.querySelector(notBeforeSelector);
      const otherDate = parseDateTime(other ? other.value : "");
      if (date && otherDate && date < otherDate) {
        return element.getAttribute("data-val-gcafterorequalto") || "Date cannot be before the related date.";
      }
    }

    return "";
  }

  function requiredMessageFor(form, element) {
    const explicit = element.getAttribute("data-val-required");
    if (explicit) {
      return explicit;
    }

    const name = element.name;
    if (name === "MarkaId" || name === "KupacId" || name === "StrankaId" || name === "GitaraId" || name === "VrstaPopravkaId") {
      return requiredLookupMessage(name);
    }

    if (name === "StatusNalogaId" || name === "TipGitareId" || name === "GuitarTipGitareId") {
      return requiredLookupMessage(name);
    }

    if (name === "GuitarMarkaId" && form.querySelector("#add-guitar-toggle")?.checked) {
      return "Brand is required when adding a guitar.";
    }

    if (name === "GuitarSerijskiBroj" && form.querySelector("#add-guitar-toggle")?.checked) {
      return "Serial number is required when adding a guitar.";
    }

    if (name === "GuitarBrojZica" && form.querySelector("#add-guitar-toggle")?.checked) {
      return "Number of strings is required when adding a guitar.";
    }

    if (name === "GuitarTipGitareId" && form.querySelector("#add-guitar-toggle")?.checked) {
      return "Guitar type is required when adding a guitar.";
    }

    if (name === "GuitarDatumZaprimanja" && form.querySelector("#add-guitar-toggle")?.checked) {
      return "Received date is required when adding a guitar.";
    }

    return "";
  }

  function requiredLookupMessage(name) {
    const messages = {
      MarkaId: "Brand is required.",
      KupacId: "Owner is required.",
      StrankaId: "Customer is required.",
      GitaraId: "Guitar is required.",
      VrstaPopravkaId: "Repair type is required.",
      StatusNalogaId: "Status is required.",
      TipGitareId: "Guitar type is required.",
      GuitarTipGitareId: "Guitar type is required when adding a guitar."
    };

    return messages[name] || "This field is required.";
  }

  function isInRange(value, min, max) {
    const number = Number(value);
    if (Number.isNaN(number)) {
      return false;
    }

    if (min !== null && min !== "" && number < Number(min)) {
      return false;
    }

    if (max !== null && max !== "" && number > Number(max)) {
      return false;
    }

    return true;
  }

  function setManualValidationMessage(element, message) {
    const messageEl = findValidationMessage(element);
    if (!messageEl) {
      return;
    }

    messageEl.textContent = message;
    messageEl.classList.remove("field-validation-valid");
    messageEl.classList.add("field-validation-error");
  }

  function clearManualValidationMessage(element) {
    const messageEl = findValidationMessage(element);
    if (!messageEl || !messageEl.textContent.trim()) {
      return;
    }

    messageEl.textContent = "";
    messageEl.classList.remove("field-validation-error");
    messageEl.classList.add("field-validation-valid");
  }

  function findValidationMessage(element) {
    return element.name
      ? document.querySelector("[data-valmsg-for='" + cssEscape(element.name) + "']")
      : null;
  }

  function setManualValidationState(element, isInvalid) {
    if (window.jQuery) {
      setValidationState(element, isInvalid);
      return;
    }

    element.classList.toggle("is-invalid", isInvalid);
    element.classList.toggle("is-valid", !isInvalid);
  }

  function normalizeMessage(message) {
    return (message || "").replace(/\s+/g, " ").trim();
  }

  function getFieldLabel(form, element) {
    const datePicker = element.closest("[data-gc-datetime]");
    if (datePicker) {
      const label = datePicker.closest(".gc-form-group")?.querySelector(".gc-label");
      if (label) {
        return normalizeMessage(label.textContent);
      }
    }

    const labelByFor = element.id ? form.querySelector("label[for='" + cssEscape(element.id) + "']") : null;
    if (labelByFor) {
      return normalizeMessage(labelByFor.textContent);
    }

    const groupLabel = element.closest(".gc-form-group")?.querySelector(".gc-label");
    if (groupLabel) {
      return normalizeMessage(groupLabel.textContent);
    }

    return element.name || "";
  }

  function focusFirstInvalidField(form, validator, fallbackElement) {
    const firstInvalid = validator?.errorList[0]?.element || fallbackElement;
    if (!firstInvalid) {
      return;
    }

    const datePicker = firstInvalid.closest("[data-gc-datetime]");
    const customSelect = firstInvalid.closest(".gc-select");
    const autocomplete = firstInvalid.closest(".gc-autocomplete");
    const focusTarget =
      datePicker?.querySelector("[data-gc-datetime-display]") ||
      customSelect?.querySelector(".gc-select__toggle") ||
      autocomplete?.querySelector(".gc-autocomplete__input") ||
      firstInvalid;

    if (focusTarget && typeof focusTarget.focus === "function") {
      focusTarget.focus({ preventScroll: true });
      focusTarget.scrollIntoView({ behavior: "smooth", block: "center" });
    }
  }

  function parseDateTime(value) {
    if (!value || !value.trim()) {
      return null;
    }

    const trimmed = value.trim();
    const iso = trimmed.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})(?::(\d{2}))?/);
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

  function cssEscape(value) {
    if (window.CSS && typeof window.CSS.escape === "function") {
      return window.CSS.escape(value);
    }

    return value.replace(/["\\]/g, "\\$&");
  }
})();
