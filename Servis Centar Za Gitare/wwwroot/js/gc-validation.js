(function () {
  "use strict";

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

  $.validator.setDefaults({
    ignore: ":hidden:not([data-gc-datetime-value]):not([data-gc-autocomplete-value]):not(#knowledge-hidden-select)",
    errorElement: "span",
    errorClass: "invalid-feedback",
    validClass: "is-valid",
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

    $(document).on("blur focusout change", "[data-gc-datetime-display]", function () {
      const hiddenInput = this.closest("[data-gc-datetime]")?.querySelector("[data-gc-datetime-value]");
      if (hiddenInput) {
        $(hiddenInput).valid();
      }
    });

    $(document).on("blur focusout change", "input[data-val='true'], select[data-val='true'], textarea[data-val='true']", function () {
      validateField(this);
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
})();
