export interface SelectOption {
  value: string;
  label: string;
  hint?: string;
  /** نص إضافي للبحث المحلي (الحالة، الهاتف، إلخ) */
  meta?: string;
  activeLabel?: string;
  activeBadgeClass?: string;
  stateLabel?: string;
  stateBadgeClass?: string;
}