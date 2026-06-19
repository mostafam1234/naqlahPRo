/** Returns today's date formatted for HTML date inputs (YYYY-MM-DD) in local timezone. */
export function getTodayDateInputValue(): string {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/** Default from/to date range for search filters (both set to today). */
export function getDefaultSearchDateRange(): { fromDate: string; toDate: string } {
  const today = getTodayDateInputValue();
  return { fromDate: today, toDate: today };
}
