---
name: validation
description: Use when implementing or reviewing form validation in this ASP.NET MVC app, especially client-side blur/focusout validation, server-side validation, unobtrusive jQuery validation, validation message styling, and keeping custom controls compatible with model binding.
---

# Validation Patterns

Use this skill when adding or changing validation behavior.

## Core Rules

- Always keep server-side validation as the source of truth.
- Add client-side validation only as fast feedback; never rely on it for correctness.
- Trigger client-side validation when a field loses focus, and also on change for selects, date pickers, autocomplete hidden inputs, and custom controls.
- Keep validation messages visually integrated with `.invalid-feedback`, `field-validation-error`, and the surrounding form control state.
- Preserve ASP.NET model binding names. Custom UI may be visible, but the posted value must come from a named input/select that matches the model property.

## ASP.NET MVC Form Pattern

- Use `asp-validation-for` beside every validated field.
- Use `aria-describedby="<Field>-error"` and keep the validation span near the input.
- For nested models, use generated names such as `Customer.Email` and ids such as `Customer_Email-error`.
- For optional sections, validate related fields only when the section is enabled.
- Before saving, re-check foreign keys with EF, for example brand, guitar type, owner, customer, status, repair type, and technician.

## Client-Side Pattern

- Use jQuery Validation unobtrusive parsing after page load.
- Set `onfocusout` to `this.element(element)`.
- Bind delegated `blur`, `focusout`, and `change` handlers to `input[data-val='true']`, `select[data-val='true']`, and `textarea[data-val='true']`.
- For custom controls, validate the hidden/native field, not only the visible display input.
- For custom date controls, when the display field blurs or changes, call `.valid()` on the hidden date value input.
- For autocomplete, dispatch both `gc:autocomplete-selected` and `change` from the hidden value input.

## Server-Side Pattern

- Normalize dates before saving to PostgreSQL `timestamp with time zone`; never save `DateTimeKind.Unspecified`.
- Convert user-entered local/unspecified date values to UTC before EF save.
- Add `ModelState.AddModelError` against the exact property name that owns the UI message.
- When the form posts nested view models, pass the correct prefix to validation helpers.
- For business rules, validate both existence and compatibility, for example guitar belongs to selected customer or technician has the required repair skill.

## UX Rules

- Mark invalid custom wrappers with `.is-invalid` as well as the underlying input.
- Clear custom validation messages only if they were produced by the same custom rule.
- Avoid browser default controls when the app already uses custom controls.
- Use short, direct validation messages.
