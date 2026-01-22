# Optimization Proposals - Charting Library Replacement

## 1. 🔍 Analysis & Context
*   **Problem:** `recharts` adds significant weight to the bundle (~500kB minified).
*   **Goal:** Replace `recharts` to optimize bundle size and remove lazy loading overhead for a better UX.
*   **Affected Files:** `DashboardPage.tsx`, `TeamMoodTrendWidget.tsx`, `package.json`.

---

## 2. 📋 Proposition 1: Manual SVG Chart (Hand-crafted)
*   **Concept:** Build a custom React component that generates native `<svg>` tags.
*   **Technical details:**
    *   Map time to X-axis and mood values (0-2/3) to Y-axis.
    *   Use `<path>` for the trend line and `<linearGradient>` for aesthetics.
    *   Simple hover logic for tooltips.
*   **Bundle Impact:** ~2kB (Minimal).
*   **Pros:** Zero dependencies, maximum performance, ultimate control.
*   **Cons:** Higher initial development and maintenance effort for the geometry logic.

## 3. 📋 Proposition 2: @mui/x-charts (MUI Ecosystem)
*   **Concept:** Use the official Material UI charting library.
*   **Technical details:**
    *   Install `@mui/x-charts`.
    *   Replace `recharts` components with MUI equivalents.
*   **Bundle Impact:** ~130kB.
*   **Pros:** Consistent with MUI theme, easier to implement than manual SVG.
*   **Cons:** Still a significant library weight (though lighter than Recharts).

---

## 4. 📊 Comparison

| Metric | Prop 1 (Manual SVG) | Prop 2 (MUI X-Charts) | Recharts (Current) |
| :--- | :--- | :--- | :--- |
| **Size (Minified)** | **~2 kB** | ~130 kB | ~500 kB |
| **Dependencies** | **0** | 1 | 1 |
| **Maintenance** | Custom | Third-party | Third-party |

---

## 5. ✅ Decision
We are proceeding with **Proposition 1** for maximum performance.
