/**
 * Audit feature constants.
 * Change type keys for i18n (ADMIN.AUDIT.INSERT, etc.)
 */
export const AUDIT_CHANGE_TYPE_KEYS: Record<number, string> = {
  1: 'ADMIN.AUDIT.INSERT',
  2: 'ADMIN.AUDIT.UPDATE',
  3: 'ADMIN.AUDIT.DELETE',
};

export const AUDIT_PAGE_SIZES = [10, 15, 25, 50] as const;
