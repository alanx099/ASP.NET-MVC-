---
name: custom-form-elements
description: Use when building or reviewing custom form elements in this ASP.NET MVC app, including autocomplete comboboxes, date/time pickers, custom selects, hidden model-binding inputs, AJAX lookup endpoints, validation integration, and accessible UI behavior.
---

# Custom Form Elements

Use this skill when replacing native browser controls or adding reusable form controls.

## Core Rules

- Keep the model-bound value in a named hidden or native input that matches the ASP.NET model property.
- Let the visible custom UI handle search/display only.
- Do not use browser default datepicker controls for date/time; use the app custom partial.
- Reuse shared partials for custom controls.
- Every custom control must support validation, keyboard use, and server-side fallback behavior.

## Autocomplete Combobox Pattern

- Render a wrapper with `data-gc-autocomplete`.
- Render a hidden input:
  - `name="<ModelProperty>"`
  - `id="<ModelPropertyId>"`
  - `data-gc-autocomplete-value`
- Render a visible text input:
  - `autocomplete="off"`
  - `aria-autocomplete="list"`
  - `aria-expanded`
  - `data-gc-autocomplete-input`
- Render a listbox container with `data-gc-autocomplete-list`.
- On input, clear the hidden value and dispatch `change`.
- On item selection, set hidden value, set display text, dispatch `gc:autocomplete-selected`, then dispatch `change`.
- Support dependencies with wrapper dataset values, for example `customerId`, `guitarId`, or `repairTypeId`.
- Fetch results with AJAX and `X-Requested-With: XMLHttpRequest`.

## AJAX Endpoint Pattern

- Return small JSON arrays shaped like `{ id, text }`.
- Limit results, usually `Take(12)`.
- Use `AsNoTracking()`.
- Normalize `term` with trim/lowercase.
- Search across natural display fields, for example:
  - customers: first name, last name, email
  - guitars: brand, type, serial number
  - brands: brand name
  - technicians: first name, last name, email
- Include extra metadata only when client logic needs it, for example guitar owner id/text or type id.

## Date/Time Picker Pattern

- Render a hidden model-bound input for the actual value.
- Render a visible display input for localized text.
- Store values in invariant ISO-like format for posting.
- Convert to UTC before saving PostgreSQL `timestamp with time zone`.
- Validate hidden values when the display field blurs or changes.
- Keep hr/en browser locale display behavior in JavaScript, but keep server parsing robust.

## Custom Select Pattern

- Keep the native `select` in the DOM for model binding and validation.
- Enhance only selects marked with `data-gc-select="true"`.
- Dispatch native `change` when a custom option is selected.
- Keep the custom option list synchronized after programmatic changes.
- Use custom select styling for list sort dropdowns and form option sets.

## Validation Integration

- Put validation messages directly under the control with `asp-validation-for` or `data-valmsg-for`.
- Toggle `.is-invalid` and `.is-valid` on both the hidden/native input and the custom wrapper.
- For autocomplete fields, validate the hidden value.
- For custom date fields, validate the hidden date value.
- Trigger validation on blur/focusout/change.

## Layout And Layering

- Custom dropdown/listbox panels must render above cards and form panels.
- Give filter/control areas higher `z-index` and `overflow: visible`.
- Avoid nesting cards inside cards.
- Keep compact form fields aligned with existing `.gc-form-group`, `.gc-input`, `.gc-select`, and `.invalid-feedback` styles.

## Accessibility Rules

- Use labels tied to the visible input when users type into it.
- Use `role="listbox"` and `role="option"` for autocomplete results.
- Support Arrow Up, Arrow Down, Enter, and Escape.
- Close popovers on outside click.
- Keep status and error messages available through `aria-describedby` and `aria-live` where relevant.
