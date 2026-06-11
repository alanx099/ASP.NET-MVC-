(function () {
  "use strict";

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }

  function init() {
    document.addEventListener("click", handleLoadMoreClick);
  }

  async function handleLoadMoreClick(event) {
    const link = event.target.closest("[data-gc-load-more]");
    if (!link || link.dataset.loading === "true") {
      return;
    }

    const grid = findListGrid(link);
    if (!grid) {
      return;
    }

    event.preventDefault();

    const previousScrollY = window.scrollY;
    const oldCount = grid.children.length;
    setLoading(link, true);

    try {
      const response = await fetch(link.href, {
        headers: {
          "X-Requested-With": "XMLHttpRequest"
        }
      });

      if (!response.ok) {
        throw new Error("Load more request failed.");
      }

      const html = await response.text();
      const nextDocument = new DOMParser().parseFromString(html, "text/html");
      const nextLoadMoreLink = nextDocument.querySelector("[data-gc-load-more]");
      const nextGrid = nextLoadMoreLink
        ? findListGrid(nextLoadMoreLink)
        : nextDocument.querySelector(".entity-grid, .card-grid");
      const nextCards = nextGrid ? Array.from(nextGrid.children).slice(oldCount) : [];

      if (nextCards.length === 0) {
        window.location.assign(link.href);
        return;
      }

      const fragment = document.createDocumentFragment();
      nextCards.forEach(function (card) {
        fragment.appendChild(document.importNode(card, true));
      });
      grid.appendChild(fragment);

      replaceControls(nextDocument);
      replaceLoadMore(nextDocument, link);
      window.history.replaceState({}, "", link.href);
      window.scrollTo(window.scrollX, previousScrollY);
      document.dispatchEvent(new CustomEvent("gc:content-added", { detail: { root: grid } }));
    } catch (error) {
      window.location.assign(link.href);
    } finally {
      setLoading(link, false);
    }
  }

  function findListGrid(fromElement) {
    if (!fromElement) {
      return document.querySelector(".entity-grid, .card-grid");
    }

    let sibling = fromElement.closest(".gc-load-more")?.previousElementSibling;
    while (sibling) {
      if (sibling.matches(".entity-grid, .card-grid")) {
        return sibling;
      }

      sibling = sibling.previousElementSibling;
    }

    return null;
  }

  function replaceControls(nextDocument) {
    const currentCount = document.querySelector(".gc-list-controls__count");
    const nextCount = nextDocument.querySelector(".gc-list-controls__count");
    if (currentCount && nextCount) {
      currentCount.replaceWith(document.importNode(nextCount, true));
    }
  }

  function replaceLoadMore(nextDocument, currentLink) {
    const currentLoadMore = currentLink.closest(".gc-load-more");
    const nextLoadMore = nextDocument.querySelector(".gc-load-more");
    if (!currentLoadMore) {
      return;
    }

    if (nextLoadMore) {
      currentLoadMore.replaceWith(document.importNode(nextLoadMore, true));
    } else {
      currentLoadMore.remove();
    }
  }

  function setLoading(link, isLoading) {
    link.dataset.loading = isLoading ? "true" : "false";
    link.setAttribute("aria-busy", isLoading.toString());
    link.textContent = isLoading ? "Loading..." : "+ Load more";
  }
})();
