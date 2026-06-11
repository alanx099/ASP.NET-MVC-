---
name: filters
description: Use when implementing or reviewing list filtering, sorting, pagination, load-more behavior, and server-side list controls in this ASP.NET MVC app, including custom select styling, autocomplete filters, asc/desc controls, and avoiding rendering all entities at once.
---

# Filter And List Patterns

Use this skill when changing model list pages such as customers, guitars, repairs, or technicians.

## Core Rules

- Do filtering, sorting, and limiting server-side. Do not render all records and hide them in the browser.
- Default to showing 10 records.
- Offer fixed amount choices: `10`, `50`, `100`, and `All`.
- Keep sort as a dropdown using the app custom select styling.
- Keep direction as an immediate `Asc / Desc` segmented control.
- Preserve all current query parameters when changing amount, direction, filters, or using `+ Load more`.

## Controller Pattern

- Accept query parameters like:
  - `sort`
  - `direction`
  - optional filter ids, for example `customerId`
  - `pageSize`
  - `take`
- Normalize `pageSize` to `10`, `50`, `100`, or `-1` for All.
- Normalize `direction` to `asc` or `desc`.
- Count after filters are applied and before `Take`.
- For `All`, set `take = totalCount`.
- For normal batches, clamp safely:
  - if `totalCount <= pageSize`, return `totalCount`
  - otherwise return between `pageSize` and `totalCount`
- Use `AsNoTracking()` for read-only list queries.
- Use `Include` only for navigation data rendered by the card.
- Declare included queries as `IQueryable<T>` before dynamic sort switches.

## View Model Pattern

Use a small list-state model with:

- `Sort`
- `Direction`
- `PageSize`
- `Take`
- `TotalCount`
- optional filter ids and display text
- optional autocomplete endpoint
- `HasMore`, `VisibleCount`, and `NextTake`

## View Pattern

- Render shared list controls above the card grid.
- Render `+ Load more` below the card grid only when `HasMore` is true.
- Remove manual Apply buttons; filters should apply immediately.
- Use `onchange="this.form.submit()"` for sort dropdowns.
- For autocomplete filters, submit the form on `gc:autocomplete-selected`.
- Add a clear link when a filter is active.
- Ensure dropdown/autocomplete panels appear above cards:
  - list controls need a higher `z-index`
  - dropdown containers need `overflow: visible`
  - card grids should stay below controls in stacking order

## Query Parameter Preservation

When generating links for amount, direction, clear, and load more:

- Keep `sort`
- Keep `direction`
- Keep active filter ids
- Keep `pageSize`
- Set `take` back to the chosen batch size when changing amount or direction
- Increment `take` only for `+ Load more`

## UX Rules

- Use compact controls; filters are work UI, not a landing section.
- Keep count text visible, for example `Showing 10 of 523`.
- Do not use infinite scroll for this app; explicit `+ Load more` is clearer.
