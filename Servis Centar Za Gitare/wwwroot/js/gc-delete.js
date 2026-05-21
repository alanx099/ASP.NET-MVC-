(function () {
  "use strict";

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }

  function init() {
    const modal = createDeleteModal();
    let pendingForm = null;

    document.querySelectorAll("[data-delete-form]").forEach(function (form) {
      form.addEventListener("submit", function (event) {
        const button = form.querySelector("button[type='submit']");
        if (button && button.disabled) {
          event.preventDefault();
          return;
        }

        const label = form.getAttribute("data-delete-label") || "this item";
        event.preventDefault();
        pendingForm = form;
        openDeleteModal(modal, label);
      });
    });

    modal.querySelector("[data-delete-cancel]").addEventListener("click", function () {
      pendingForm = null;
      closeDeleteModal(modal);
    });

    modal.querySelector("[data-delete-confirm]").addEventListener("click", function () {
      if (!pendingForm) {
        closeDeleteModal(modal);
        return;
      }

      const form = pendingForm;
      pendingForm = null;
      closeDeleteModal(modal);
      animateAndSubmit(form);
    });

    modal.addEventListener("click", function (event) {
      if (event.target === modal) {
        pendingForm = null;
        closeDeleteModal(modal);
      }
    });

    modal.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        pendingForm = null;
        closeDeleteModal(modal);
      }
    });
  }

  function animateAndSubmit(form) {
    const card = form.closest("[data-delete-card]");
    if (!card || card.classList.contains("is-shredding")) {
      form.submit();
      return;
    }

    card.classList.add("is-shredding");
    buildShreds(card);
    window.setTimeout(function () {
      form.submit();
    }, 720);
  }

  function createDeleteModal() {
    const existing = document.getElementById("gc-delete-modal");
    if (existing) {
      return existing;
    }

    const modal = document.createElement("div");
    modal.className = "gc-modal";
    modal.id = "gc-delete-modal";
    modal.setAttribute("aria-hidden", "true");
    modal.setAttribute("role", "dialog");
    modal.setAttribute("aria-modal", "true");
    modal.setAttribute("aria-labelledby", "gc-delete-modal-title");
    modal.innerHTML =
      '<div class="gc-modal__panel">' +
        '<p class="section-label">Delete confirmation</p>' +
        '<h3 class="gc-modal__title" id="gc-delete-modal-title">Delete item?</h3>' +
        '<p class="section-copy" data-delete-message>This action cannot be undone.</p>' +
        '<div class="gc-actions">' +
          '<button type="button" class="btn gc-btn-secondary" data-delete-cancel>Cancel</button>' +
          '<button type="button" class="btn gc-btn-primary gc-btn-danger" data-delete-confirm>Delete</button>' +
        '</div>' +
      '</div>';

    document.body.appendChild(modal);
    return modal;
  }

  function openDeleteModal(modal, label) {
    const title = modal.querySelector("#gc-delete-modal-title");
    const message = modal.querySelector("[data-delete-message]");
    if (title) {
      title.textContent = "Delete " + label + "?";
    }
    if (message) {
      message.textContent = "This will permanently remove " + label + " from the system.";
    }

    modal.setAttribute("aria-hidden", "false");
    modal.querySelector("[data-delete-cancel]")?.focus();
  }

  function closeDeleteModal(modal) {
    modal.setAttribute("aria-hidden", "true");
  }

  function buildShreds(card) {
    const shredLayer = document.createElement("div");
    shredLayer.className = "gc-shred-layer";
    shredLayer.setAttribute("aria-hidden", "true");

    for (let index = 0; index < 10; index += 1) {
      const shred = document.createElement("span");
      shred.className = "gc-shred-strip";
      shred.style.left = (index * 10) + "%";
      shred.style.animationDelay = (index * 28) + "ms";
      shredLayer.appendChild(shred);
    }

    card.appendChild(shredLayer);
  }
})();
