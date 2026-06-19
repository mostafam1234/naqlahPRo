/** @type {import('tailwindcss').Config} */
const colors = require('./src/theme/colors.json');

const chartColors = Object.fromEntries(
  colors.chart.map((hex, index) => [index, hex])
);

module.exports = {
  darkMode: 'class',
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['"Plus Jakarta Sans"', '"IBM Plex Sans Arabic"', 'system-ui', 'sans-serif'],
      },
      colors: {
        primary: colors.primary,
        accent: colors.accent,
        brand: colors.brand,
        chart: chartColors,
        neutral: colors.brand,
        input: {
          50: colors.brand[50],
          100: colors.brand[100],
          200: colors.brand[200],
          300: colors.brand[300],
          400: colors.brand[400],
          500: colors.brand[500],
          600: colors.brand[600],
        },
        success: {
          50: '#ecfdf5',
          100: '#d1fae5',
          200: '#a7f3d0',
          300: '#6ee7b7',
          400: '#34d399',
          ...colors.success,
          700: '#047857',
        },
        error: {
          50: '#fef2f2',
          100: '#fee2e2',
          200: '#fecaca',
          300: '#fca5a5',
          400: '#f87171',
          ...colors.error,
          700: '#b91c1c',
        },
        warning: {
          50: '#fffbeb',
          100: '#fef3c7',
          200: '#fde68a',
          300: '#fcd34d',
          400: '#fbbf24',
          ...colors.warning,
          700: '#b45309',
        },
        info: colors.accent,
        surface: {
          50: '#ffffff',
          100: colors.brand[50],
          200: colors.brand[100],
          300: colors.brand[200],
        },
        background: {
          primary: '#ffffff',
          secondary: colors.brand[50],
          tertiary: colors.brand[100],
        },
      },
      boxShadow: {
        sidebar: '4px 0 32px rgba(0, 0, 0, 0.35)',
        glow: `0 0 24px ${colors.primary[500]}73`,
        'glow-accent': `0 0 20px ${colors.accent[400]}59`,
        header: '0 1px 0 rgba(0,0,0,0.06), 0 4px 24px rgba(0,0,0,0.04)',
      },
      backgroundImage: {
        'naqlah-gradient': `linear-gradient(135deg, ${colors.primary[500]} 0%, ${colors.primary[600]} 100%)`,
        'naqlah-gradient-hover': `linear-gradient(135deg, ${colors.primary[600]} 0%, ${colors.primary[700]} 100%)`,
        'naqlah-sidebar': `linear-gradient(180deg, ${colors.brand[50]} 0%, ${colors.brand[100]} 100%)`,
        'naqlah-sidebar-dark': `linear-gradient(180deg, ${colors.brand[800]} 0%, ${colors.brand[900]} 100%)`,
        'naqlah-surface': `linear-gradient(135deg, ${colors.primary[50]} 0%, ${colors.accent[50]} 100%)`,
      },
      animation: {
        'fade-in': 'fadeIn 0.3s ease-in-out',
        'slide-in-right': 'slideInRight 0.3s ease-out',
        'pulse-glow': 'pulseGlow 3s ease-in-out infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideInRight: {
          '0%': { transform: 'translateX(16px)', opacity: '0' },
          '100%': { transform: 'translateX(0)', opacity: '1' },
        },
        pulseGlow: {
          '0%, 100%': { boxShadow: `0 0 16px ${colors.primary[500]}59` },
          '50%': { boxShadow: `0 0 28px ${colors.accent[400]}73` },
        },
      },
    },
  },
  plugins: [],
};
