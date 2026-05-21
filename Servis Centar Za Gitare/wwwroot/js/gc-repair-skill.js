(function () {
  "use strict";

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }

  function init() {
    const form = document.querySelector("[data-repair-form]");
    const dataEl = document.getElementById("repair-skill-data");
    const modal = document.getElementById("repair-skill-modal");

    if (!form || !dataEl || !modal) {
      return;
    }

    const data = JSON.parse(dataEl.textContent || "{}");
    const customerSelect = form.querySelector("[name='StrankaId']");
    const guitarSelect = form.querySelector("[name='GitaraId']");
    const technicianSelect = form.querySelector("[name='TehnicarId']");
    const repairTypeSelect = form.querySelector("[name='VrstaPopravkaId']");
    const statusSelect = form.querySelector("[name='StatusNalogaId']");

    if (!customerSelect || !guitarSelect || !technicianSelect || !repairTypeSelect || !statusSelect) {
      return;
    }

    let isSyncing = false;

    customerSelect.addEventListener("change", function () {
      if (isSyncing) {
        return;
      }

      filterGuitarsByCustomer(data, guitarSelect, customerSelect.value);

      if (guitarSelect.value && !guitarBelongsToCustomer(data, guitarSelect.value, customerSelect.value)) {
        setSelectValue(guitarSelect, "", false);
      }

      applySkillRules(data, modal, guitarSelect, technicianSelect, repairTypeSelect, statusSelect, true);
    });

    guitarSelect.addEventListener("change", function () {
      const guitar = getGuitar(data, guitarSelect.value);
      if (guitar && customerSelect.value !== guitar.ownerId.toString()) {
        isSyncing = true;
        setSelectValue(customerSelect, guitar.ownerId.toString(), false, guitar.ownerText);
        filterGuitarsByCustomer(data, guitarSelect, customerSelect.value);
        isSyncing = false;
      }

      applySkillRules(data, modal, guitarSelect, technicianSelect, repairTypeSelect, statusSelect, true);
    });

    repairTypeSelect.addEventListener("change", function () {
      applySkillRules(data, modal, guitarSelect, technicianSelect, repairTypeSelect, statusSelect, true);
    });

    technicianSelect.addEventListener("change", function () {
      if (!technicianSelect.value) {
        clearMissingSkill(technicianSelect, repairTypeSelect);
        return;
      }

      if (!hasRequiredSkill(data, guitarSelect.value, technicianSelect.value, repairTypeSelect.value)) {
        setSelectValue(technicianSelect, "", false);
        filterTechnicians(technicianSelect, getQualifiedTechnicians(data, guitarSelect.value, repairTypeSelect.value));
        markMissingSkill(technicianSelect, repairTypeSelect);
        openModal(modal, "Missing required skill", "The selected technician does not have the skill required for this repair. Choose one of the available technicians in the filtered dropdown.");
      } else {
        clearMissingSkill(technicianSelect, repairTypeSelect);
      }
    });

    form.addEventListener("submit", function (event) {
      const result = applySkillRules(data, modal, guitarSelect, technicianSelect, repairTypeSelect, statusSelect, false);

      if (result === "invalid-technician") {
        event.preventDefault();
        event.stopPropagation();
        openModal(modal, "Missing required skill", "The selected technician does not have the skill required for this repair. The technician was cleared and the dropdown now shows only technicians who can fulfill the selected repair.");
      }
    }, true);

    modal.querySelectorAll("[data-repair-skill-close]").forEach(function (button) {
      button.addEventListener("click", function () {
        closeModal(modal);
      });
    });

    modal.addEventListener("click", function (event) {
      if (event.target === modal) {
        closeModal(modal);
      }
    });

    modal.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        closeModal(modal);
      }
    });

    filterGuitarsByCustomer(data, guitarSelect, customerSelect.value);
    applySkillRules(data, modal, guitarSelect, technicianSelect, repairTypeSelect, statusSelect, false);
  }

  function applySkillRules(data, modal, guitarSelect, technicianSelect, repairTypeSelect, statusSelect, shouldWarn) {
    const qualifiedTechnicians = getQualifiedTechnicians(data, guitarSelect.value, repairTypeSelect.value);
    filterTechnicians(technicianSelect, qualifiedTechnicians);

    if (!guitarSelect.value || !repairTypeSelect.value) {
      resetStatusOptions(statusSelect);
      clearMissingSkill(technicianSelect, repairTypeSelect);
      return "incomplete";
    }

    if (qualifiedTechnicians.length === 0) {
      setSelectValue(technicianSelect, "", false);
      setReceivedOnlyStatus(data, statusSelect);
      clearMissingSkill(technicianSelect, repairTypeSelect);

      if (shouldWarn) {
        openModal(modal, "No technician available", "There is no technician with the required skill for this guitar type and repair type. Add a technician or edit an existing technician with this skill. This repair can only be saved with status Zaprimljen for now.");
      }

      return "no-technician";
    }

    resetStatusOptions(statusSelect);

    if (technicianSelect.value && !hasRequiredSkill(data, guitarSelect.value, technicianSelect.value, repairTypeSelect.value)) {
      setSelectValue(technicianSelect, "", false);
      markMissingSkill(technicianSelect, repairTypeSelect);

      if (shouldWarn) {
        openModal(modal, "Missing required skill", "The selected technician does not have the skill required for this repair. The technician was cleared and the dropdown now shows only technicians who can fulfill the selected repair.");
      }

      return "invalid-technician";
    }

    clearMissingSkill(technicianSelect, repairTypeSelect);
    return "ok";
  }

  function filterGuitarsByCustomer(data, guitarSelect, customerId) {
    const allowed = new Set((data.guitars || [])
      .filter(function (guitar) {
        return !customerId || guitar.ownerId.toString() === customerId.toString();
      })
      .map(function (guitar) {
        return guitar.id.toString();
      }));

    setSelectAvailability(guitarSelect, function (option) {
      return allowed.has(option.value.toString());
    });
  }

  function filterTechnicians(select, qualifiedTechnicians) {
    const allowed = new Set((qualifiedTechnicians || []).map(function (item) {
      return item.id.toString();
    }));

    setSelectAvailability(select, function (option) {
      return allowed.has(option.value.toString());
    });
  }

  function setReceivedOnlyStatus(data, statusSelect) {
    const receivedStatusId = (data.receivedStatusId || 1).toString();
    setSelectValue(statusSelect, receivedStatusId, false);
    setSelectAvailability(statusSelect, function (option) {
      return option.value === receivedStatusId;
    });
  }

  function resetStatusOptions(statusSelect) {
    setSelectAvailability(statusSelect, function () {
      return true;
    });
  }

  function setSelectAvailability(select, predicate) {
    if (!select.options) {
      return;
    }

    Array.from(select.options).forEach(function (option) {
      const isAllowed = option.value === "" || predicate(option);
      option.hidden = !isAllowed;
      option.disabled = !isAllowed;
    });

    const wrapper = select.closest(".gc-select");
    if (!wrapper) {
      return;
    }

    wrapper.querySelectorAll(".gc-select__option").forEach(function (optionEl, index) {
      const nativeOption = select.options[index];
      if (!nativeOption) {
        return;
      }

      optionEl.style.display = nativeOption.hidden ? "none" : "";
      optionEl.setAttribute("aria-disabled", nativeOption.disabled ? "true" : "false");
    });
  }

  function hasRequiredSkill(data, guitarId, technicianId, repairTypeId) {
    if (!guitarId || !technicianId || !repairTypeId) {
      return true;
    }

    const guitar = getGuitar(data, guitarId);
    const technician = (data.technicianSkills || []).find(function (item) {
      return item.id.toString() === technicianId.toString();
    });

    if (!guitar || !technician) {
      return true;
    }

    return (technician.skills || []).some(function (skill) {
      return skill.typeId.toString() === guitar.typeId.toString() &&
        skill.repairTypeId.toString() === repairTypeId.toString();
    });
  }

  function getQualifiedTechnicians(data, guitarId, repairTypeId) {
    if (!guitarId || !repairTypeId) {
      return data.technicianSkills || [];
    }

    const guitar = getGuitar(data, guitarId);
    if (!guitar) {
      return data.technicianSkills || [];
    }

    return (data.technicianSkills || []).filter(function (technician) {
      return (technician.skills || []).some(function (skill) {
        return skill.typeId.toString() === guitar.typeId.toString() &&
          skill.repairTypeId.toString() === repairTypeId.toString();
      });
    });
  }

  function getGuitar(data, guitarId) {
    return (data.guitars || []).find(function (item) {
      return item.id.toString() === guitarId.toString();
    });
  }

  function guitarBelongsToCustomer(data, guitarId, customerId) {
    if (!guitarId || !customerId) {
      return true;
    }

    const guitar = getGuitar(data, guitarId);
    return !guitar || guitar.ownerId.toString() === customerId.toString();
  }

  function setSelectValue(select, value, shouldDispatch, text) {
    select.value = value;

    const wrapper = select.closest(".gc-select");
    if (wrapper) {
      const selectedOption = Array.from(select.options).find(function (option) {
        return option.value === value;
      }) || select.options[select.selectedIndex];

      const valueEl = wrapper.querySelector(".gc-select__value");
      if (valueEl && selectedOption) {
        valueEl.textContent = selectedOption.textContent;
      }

      wrapper.querySelectorAll(".gc-select__option").forEach(function (optionEl) {
        optionEl.setAttribute("aria-selected", optionEl.getAttribute("data-value") === value ? "true" : "false");
      });
    }

    const autocomplete = select.closest(".gc-autocomplete");
    if (autocomplete) {
      if (value && text) {
        select.dispatchEvent(new CustomEvent("gc:set-display", { detail: { text } }));
      } else if (!value) {
        select.dispatchEvent(new Event("gc:clear-display"));
      }
    }

    if (shouldDispatch) {
      select.dispatchEvent(new Event("change", { bubbles: true }));
    }
  }

  function markMissingSkill(technicianSelect, repairTypeSelect) {
    [technicianSelect, repairTypeSelect].forEach(function (select) {
      select.classList.add("is-invalid");
      const wrapper = select.closest(".gc-select, .gc-autocomplete");
      if (wrapper) {
        wrapper.classList.add("is-invalid");
      }
    });

    setValidationMessage("TehnicarId", "Missing required skill");
    setValidationMessage("VrstaPopravkaId", "Missing required skill");
  }

  function clearMissingSkill(technicianSelect, repairTypeSelect) {
    [technicianSelect, repairTypeSelect].forEach(function (select) {
      select.classList.remove("is-invalid");
      const wrapper = select.closest(".gc-select, .gc-autocomplete");
      if (wrapper) {
        wrapper.classList.remove("is-invalid");
      }
    });

    clearValidationMessage("TehnicarId");
    clearValidationMessage("VrstaPopravkaId");
  }

  function setValidationMessage(fieldName, message) {
    const messageEl = document.querySelector("[data-valmsg-for='" + fieldName + "']");
    if (!messageEl) {
      return;
    }

    messageEl.textContent = message;
    messageEl.classList.remove("field-validation-valid");
    messageEl.classList.add("field-validation-error");
  }

  function clearValidationMessage(fieldName) {
    const messageEl = document.querySelector("[data-valmsg-for='" + fieldName + "']");
    if (!messageEl || messageEl.textContent !== "Missing required skill") {
      return;
    }

    messageEl.textContent = "";
    messageEl.classList.remove("field-validation-error");
    messageEl.classList.add("field-validation-valid");
  }

  function openModal(modal, title, message) {
    const titleEl = modal.querySelector("[data-repair-skill-title]");
    const messageEl = modal.querySelector("[data-repair-skill-message]");
    if (titleEl && title) {
      titleEl.textContent = title;
    }
    if (messageEl && message) {
      messageEl.textContent = message;
    }

    modal.setAttribute("aria-hidden", "false");
    modal.querySelector("[data-repair-skill-close]")?.focus();
  }

  function closeModal(modal) {
    modal.setAttribute("aria-hidden", "true");
  }
})();
