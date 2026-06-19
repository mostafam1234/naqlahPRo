export function isRtlPage(): boolean {
  return document.documentElement.dir === 'rtl'
    || getComputedStyle(document.documentElement).direction === 'rtl';
}

export function attachFormalDropdownPanelToBody(panel: HTMLElement): void {
  if (panel.parentElement !== document.body) {
    document.body.appendChild(panel);
  }
}

export function detachFormalDropdownPanelFromBody(panel: HTMLElement | null | undefined): void {
  if (panel?.parentElement === document.body) {
    panel.remove();
  }
}

export function positionFormalDropdownPanel(
  trigger: HTMLElement,
  panel: HTMLElement,
  minWidth = 384
): void {
  const rect = trigger.getBoundingClientRect();
  const width = Math.max(rect.width, minWidth);
  const rtl = isRtlPage();

  let left = rtl ? rect.right - width : rect.left;

  if (left < 8) left = 8;
  if (left + width > window.innerWidth - 8) {
    left = Math.max(8, window.innerWidth - width - 8);
  }

  const gap = 6;
  const spaceBelow = window.innerHeight - rect.bottom - gap - 8;
  const spaceAbove = rect.top - gap - 8;
  const desiredHeight = Math.min(window.innerHeight * 0.7, 420);
  const maxHeight = Math.max(180, Math.min(desiredHeight, Math.max(spaceBelow, spaceAbove)));

  const panelHeight = Math.min(panel.scrollHeight || maxHeight, maxHeight);
  const openAbove = spaceBelow < Math.min(220, panelHeight) && spaceAbove > spaceBelow;
  const top = openAbove
    ? Math.max(8, rect.top - gap - panelHeight)
    : rect.bottom + gap;

  panel.style.position = 'fixed';
  panel.style.top = `${top}px`;
  panel.style.left = `${left}px`;
  panel.style.right = 'auto';
  panel.style.bottom = 'auto';
  panel.style.width = `${width}px`;
  panel.style.maxHeight = `${maxHeight}px`;
  panel.style.zIndex = '10000';
}

export function resetFormalDropdownPanel(panel: HTMLElement): void {
  panel.style.position = '';
  panel.style.top = '';
  panel.style.left = '';
  panel.style.right = '';
  panel.style.bottom = '';
  panel.style.width = '';
  panel.style.maxHeight = '';
  panel.style.zIndex = '';
}
