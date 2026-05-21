(function () {
  "use strict";

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initRepairAlerts);
  } else {
    initRepairAlerts();
  }

  function initRepairAlerts() {
    if (!("animate" in Element.prototype)) {
      return;
    }

    document.querySelectorAll("[data-repair-alert='impossible']").forEach(function (card, index) {
      const delay = index * 120;
      card.animate([
        { transform: "translateX(0) scale(1)", boxShadow: "0 0 0 rgba(255,107,129,0)" },
        { transform: "translateX(-2px) scale(1.006)", boxShadow: "0 0 34px rgba(255,107,129,0.34)" },
        { transform: "translateX(2px) scale(1.012)", boxShadow: "0 0 54px rgba(255,107,129,0.52)" },
        { transform: "translateX(0) scale(1)", boxShadow: "0 0 0 rgba(255,107,129,0)" }
      ], {
        duration: 1550,
        delay,
        iterations: Infinity,
        easing: "cubic-bezier(.2,.8,.2,1)"
      });

      sweepAlarm(card, delay);
    });

    document.querySelectorAll("[data-repair-alert='needs-tech'] .repair-edit-action").forEach(function (button, index) {
      button.animate([
        { transform: "translateX(0)", filter: "brightness(1)" },
        { transform: "translateX(5px)", filter: "brightness(1.35)" },
        { transform: "translateX(-3px)", filter: "brightness(1.2)" },
        { transform: "translateX(0)", filter: "brightness(1)" }
      ], {
        duration: 1150,
        delay: index * 90,
        iterations: Infinity,
        easing: "ease-in-out"
      });
    });
  }

  function sweepAlarm(card, delay) {
    const sweep = document.createElement("span");
    sweep.className = "repair-alarm-sweep";
    sweep.setAttribute("aria-hidden", "true");
    card.appendChild(sweep);

    sweep.animate([
      { transform: "translateX(-120%) skewX(-18deg)", opacity: 0 },
      { transform: "translateX(-20%) skewX(-18deg)", opacity: 0.65 },
      { transform: "translateX(120%) skewX(-18deg)", opacity: 0 }
    ], {
      duration: 2200,
      delay,
      iterations: Infinity,
      easing: "cubic-bezier(.16,1,.3,1)"
    });
  }
})();
