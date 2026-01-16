# Implementation Plan - Manual SVG Chart Replacement

## 1. 🔍 Analysis & Context
*   **Objective:** Replace `recharts` with a hand-crafted SVG implementation to eliminate library overhead.
*   **Affected Files:**
    *   `app/frontend/package.json`
    *   `app/frontend/src/pages/DashboardPage.tsx`
    *   `app/frontend/src/components/dashboard/TeamMoodTrendWidget.tsx`
*   **Key Dependencies:** None (removes `recharts`).
*   **Risks/Unknowns:** Correctly scaling dates and mood values into SVG coordinate space. Handling responsive resizing.

## 2. 📋 Checklist
- [ ] Step 1: Cleanup & Dependencies
- [ ] Step 2: Remove Lazy Loading in Dashboard
- [ ] Step 3: Implement SVG Logic in Widget
- [ ] Step 4: Add Tooltip Interaction
- [ ] Verification

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Cleanup & Dependencies
*   **Goal:** Remove the heavy library.
*   **Action:**
    *   Run `npm uninstall recharts` in `app/frontend`.
*   **Verification:** `package.json` no longer contains `recharts`.

### Step 2: Remove Lazy Loading in Dashboard
*   **Goal:** Revert to standard import since the component is now lightweight.
*   **Action:**
    *   Modify `app/frontend/src/pages/DashboardPage.tsx`:
        *   Remove `React.lazy` for `TeamMoodTrendWidget`.
        *   Remove `<Suspense>` wrapper.
        *   Add standard `import TeamMoodTrendWidget from ...`.

### Step 3: Implement SVG Logic in Widget
*   **Goal:** Replace Recharts components with SVG primitives.
*   **Action:**
    *   Modify `app/frontend/src/components/dashboard/TeamMoodTrendWidget.tsx`.
    *   **Logic:**
        *   Calculate X/Y coordinates: `x = (index / (data.length - 1)) * width`, `y = height - (moodScore / maxScore) * height`.
        *   Create `<svg viewBox="0 0 width height">`.
        *   Render `<path d="..." />` for the line.
        *   Add `<linearGradient>` for the area fill.
        *   Add simple `<text>` or `Box` for axes/labels.
*   **Verification:** Chart visualizes data correctly.

### Step 4: Add Tooltip Interaction
*   **Goal:** Restore hover functionality.
*   **Action:**
    *   Overlay the SVG with transparent "hit areas" (rects) or use a single mouse move listener.
    *   Display a floating `Paper` or `Box` with the mood details on hover.

## 4. 🧪 Testing Strategy
*   **Functional:** Ensure the trend line matches the mood data.
*   **Responsive:** Verify the SVG scales correctly in its container (using `viewBox`).
*   **Theme:** Ensure colors work in both Light and Dark modes.

## 5. ✅ Success Criteria
*   `recharts` is removed from bundle.
*   Bundle size is significantly reduced.
*   The chart looks as "sexy" as before (smoothed lines, gradients).
*   Interaction (Tooltip) works.
