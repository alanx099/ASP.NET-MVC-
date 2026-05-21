(function () {
  "use strict";

  const debounceMs = 180;

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }

  function init() {
    document.querySelectorAll("[data-gc-autocomplete]").forEach(enhanceAutocomplete);
  }

  function enhanceAutocomplete(root) {
    const hidden = root.querySelector("[data-gc-autocomplete-value]");
    const input = root.querySelector("[data-gc-autocomplete-input]");
    const list = root.querySelector("[data-gc-autocomplete-list]");
    const endpoint = root.dataset.endpoint;
    let activeIndex = -1;
    let items = [];
    let timer = 0;

    if (!hidden || !input || !list || !endpoint) {
      return;
    }

    input.addEventListener("input", function () {
      hidden.value = "";
      hidden.dispatchEvent(new Event("change", { bubbles: true }));
      scheduleFetch();
    });

    input.addEventListener("focus", function () {
      fetchResults();
    });

    input.addEventListener("keydown", function (event) {
      if (event.key === "ArrowDown") {
        event.preventDefault();
        moveActive(1);
      } else if (event.key === "ArrowUp") {
        event.preventDefault();
        moveActive(-1);
      } else if (event.key === "Enter" && activeIndex >= 0) {
        event.preventDefault();
        selectItem(items[activeIndex]);
      } else if (event.key === "Escape") {
        closeList();
      }
    });

    document.addEventListener("click", function (event) {
      if (!root.contains(event.target)) {
        closeList();
      }
    });

    hidden.addEventListener("gc:set-display", function (event) {
      input.value = event.detail?.text || "";
    });

    hidden.addEventListener("gc:clear-display", function () {
      input.value = "";
    });

    function scheduleFetch() {
      window.clearTimeout(timer);
      timer = window.setTimeout(fetchResults, debounceMs);
    }

    async function fetchResults() {
      const url = new URL(endpoint, window.location.origin);
      url.searchParams.set("term", input.value || "");
      appendDependency(url, "customerId", root.dataset.customerField);
      appendDependency(url, "guitarId", root.dataset.guitarField);
      appendDependency(url, "repairTypeId", root.dataset.repairTypeField);

      const response = await fetch(url.toString(), {
        headers: { "X-Requested-With": "XMLHttpRequest" }
      });

      if (!response.ok) {
        return;
      }

      items = await response.json();
      activeIndex = -1;
      renderList();
    }

    function renderList() {
      list.innerHTML = "";

      if (!items.length) {
        const empty = document.createElement("div");
        empty.className = "gc-autocomplete__empty";
        empty.textContent = "No results";
        list.appendChild(empty);
        openList();
        return;
      }

      items.forEach(function (item, index) {
        const option = document.createElement("button");
        option.type = "button";
        option.className = "gc-autocomplete__option";
        option.setAttribute("role", "option");
        option.textContent = item.text;
        option.addEventListener("click", function () {
          selectItem(item);
        });
        option.addEventListener("mouseenter", function () {
          setActive(index);
        });
        list.appendChild(option);
      });

      openList();
    }

    function selectItem(item) {
      hidden.value = item.id;
      input.value = item.text;
      hidden.dispatchEvent(new CustomEvent("gc:autocomplete-selected", { bubbles: true, detail: item }));
      hidden.dispatchEvent(new Event("change", { bubbles: true }));
      closeList();
    }

    function moveActive(delta) {
      if (!items.length) {
        return;
      }

      const nextIndex = activeIndex + delta;
      setActive((nextIndex + items.length) % items.length);
    }

    function setActive(index) {
      activeIndex = index;
      list.querySelectorAll(".gc-autocomplete__option").forEach(function (option, optionIndex) {
        option.setAttribute("aria-selected", optionIndex === activeIndex ? "true" : "false");
      });
    }

    function openList() {
      input.setAttribute("aria-expanded", "true");
      list.setAttribute("aria-hidden", "false");
    }

    function closeList() {
      input.setAttribute("aria-expanded", "false");
      list.setAttribute("aria-hidden", "true");
    }
  }

  function appendDependency(url, name, selector) {
    if (!selector) {
      return;
    }

    const element = document.querySelector(selector);
    if (element && element.value) {
      url.searchParams.set(name, element.value);
    }
  }
})();
