// GC Custom Select Enhancer
// Upgrades native <select data-gc-select="true"> with accessible custom UI

(function() {
  'use strict';

  // Initialize on DOM ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initGcSelects);
  } else {
    initGcSelects();
  }

  function initGcSelects() {
    const selects = document.querySelectorAll('select[data-gc-select="true"]');
    selects.forEach(nativeSelect => {
      enhanceSelect(nativeSelect);
    });
  }

  function enhanceSelect(nativeSelect) {
    // Wrap in gc-select container if not already wrapped
    let wrapper = nativeSelect.parentElement;
    if (!wrapper || !wrapper.classList.contains('gc-select')) {
      wrapper = document.createElement('div');
      wrapper.className = 'gc-select';
      nativeSelect.parentNode.insertBefore(wrapper, nativeSelect);
      wrapper.appendChild(nativeSelect);
    }

    // Add native select class
    nativeSelect.classList.add('gc-select__native');

    // Get current value
    const selectedIndex = nativeSelect.selectedIndex;
    const selectedOption = nativeSelect.options[selectedIndex];
    const selectedText = selectedOption ? selectedOption.textContent : 'Select...';
    const selectedValue = nativeSelect.value;

    // Create toggle button
    const toggle = document.createElement('button');
    toggle.type = 'button';
    toggle.className = 'gc-select__toggle';
    toggle.setAttribute('aria-haspopup', 'listbox');
    toggle.setAttribute('aria-expanded', 'false');
    toggle.setAttribute('id', 'gc-select-toggle-' + generateId());

    const valueSpan = document.createElement('span');
    valueSpan.className = 'gc-select__value';
    valueSpan.textContent = selectedText;

    const chevron = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    chevron.setAttribute('class', 'gc-select__chevron');
    chevron.setAttribute('viewBox', '0 0 24 24');
    chevron.setAttribute('aria-hidden', 'true');
    const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
    path.setAttribute('d', 'M7 10l5 5 5-5z');
    chevron.appendChild(path);

    toggle.appendChild(valueSpan);
    toggle.appendChild(chevron);

    // Create custom list
    const listId = 'gc-select-list-' + generateId();
    const list = document.createElement('ul');
    list.className = 'gc-select__list';
    list.setAttribute('role', 'listbox');
    list.setAttribute('aria-hidden', 'true');
    list.setAttribute('id', listId);
    list.setAttribute('tabindex', '-1');

    // Populate options
    Array.from(nativeSelect.options).forEach((option, index) => {
      const li = document.createElement('li');
      const optionId = 'gc-option-' + generateId();
      li.className = 'gc-select__option';
      li.setAttribute('role', 'option');
      li.setAttribute('id', optionId);
      li.setAttribute('data-value', option.value);
      li.setAttribute('aria-selected', index === selectedIndex ? 'true' : 'false');
      li.textContent = option.textContent;

      li.addEventListener('click', () => selectOption(option.value, li, toggle, list, nativeSelect, valueSpan));
      li.addEventListener('keydown', e => handleOptionKeydown(e, li, list, toggle, nativeSelect, valueSpan));

      list.appendChild(li);
    });

    toggle.setAttribute('aria-controls', listId);

    // Insert into wrapper
    wrapper.insertBefore(toggle, nativeSelect);
    wrapper.insertBefore(list, nativeSelect);

    // Toggle open/close
    toggle.addEventListener('click', e => {
      e.preventDefault();
      const isOpen = toggle.getAttribute('aria-expanded') === 'true';
      if (isOpen) {
        closeList(toggle, list);
      } else {
        openList(toggle, list);
      }
    });

    // Close on outside click
    document.addEventListener('click', e => {
      if (!wrapper.contains(e.target)) {
        closeList(toggle, list);
      }
    });

    // Keyboard on toggle
    toggle.addEventListener('keydown', e => {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        openList(toggle, list);
      } else if (e.key === 'ArrowDown') {
        e.preventDefault();
        openList(toggle, list);
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        openList(toggle, list);
      }
    });

    // Sync native select changes (for programmatic updates)
    const observer = new MutationObserver(() => {
      const newIdx = nativeSelect.selectedIndex;
      const newText = nativeSelect.options[newIdx] ? nativeSelect.options[newIdx].textContent : 'Select...';
      valueSpan.textContent = newText;
      updateSelectedStates(list, nativeSelect.value);
    });
    observer.observe(nativeSelect, { attributes: true, attributeFilter: ['value'] });
  }

  function openList(toggle, list) {
    toggle.setAttribute('aria-expanded', 'true');
    list.setAttribute('aria-hidden', 'false');
    const selectedOption = list.querySelector('[aria-selected="true"]');
    const focusTarget = selectedOption || list.querySelector('[role="option"]');
    if (focusTarget) {
      focusTarget.focus();
      list.setAttribute('aria-activedescendant', focusTarget.id);
    }
  }

function closeList(toggle, list, shouldReturnFocus = false) {
  toggle.setAttribute('aria-expanded', 'false');
  list.setAttribute('aria-hidden', 'true');

  if (shouldReturnFocus) {
    toggle.focus();
  }
}

  function selectOption(value, optionEl, toggle, list, nativeSelect, valueSpan) {
    // Update native select
    nativeSelect.value = value;
    nativeSelect.dispatchEvent(new Event('change', { bubbles: true }));

    // Update UI
    valueSpan.textContent = optionEl.textContent;
    updateSelectedStates(list, value);

    // Close list
    closeList(toggle, list);
  }

  function updateSelectedStates(list, value) {
    list.querySelectorAll('[role="option"]').forEach(opt => {
      opt.setAttribute('aria-selected', opt.getAttribute('data-value') === value ? 'true' : 'false');
    });
  }

  function handleOptionKeydown(e, li, list, toggle, nativeSelect, valueSpan) {
    const options = Array.from(list.querySelectorAll('[role="option"]'));
    const currentIndex = options.indexOf(li);

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        const nextIndex = (currentIndex + 1) % options.length;
        const nextOption = options[nextIndex];
        nextOption.focus();
        list.setAttribute('aria-activedescendant', nextOption.id);
        break;
      case 'ArrowUp':
        e.preventDefault();
        const prevIndex = currentIndex === 0 ? options.length - 1 : currentIndex - 1;
        const prevOption = options[prevIndex];
        prevOption.focus();
        list.setAttribute('aria-activedescendant', prevOption.id);
        break;
      case 'Enter':
      case ' ':
        e.preventDefault();
        selectOption(li.getAttribute('data-value'), li, toggle, list, nativeSelect, valueSpan);
        break;
      case 'Escape':
        e.preventDefault();
        closeList(toggle, list);
        break;
      default:
        break;
    }
  }

  function generateId() {
    return Math.random().toString(36).substr(2, 9);
  }
})();
