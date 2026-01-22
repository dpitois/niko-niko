# Implementation Plan - Debounce Mood Entries

## 1. 🔍 Analysis & Context
*   **Objective:** Implement click debouncing on the Mood Grid to prevent multiple API calls and notifications when a user cycles through mood options rapidly.
*   **Affected Files:** `app/frontend/src/components/sprints/SprintMoodGrid.tsx`
*   **Key Dependencies:** React (`useState`, `useRef`, `useEffect`).
*   **Risks/Unknowns:** Ensuring the optimistic UI stays in sync with the eventual SWR revalidation. Handling component unmounts while a save is pending.

## 2. 📋 Checklist
- [ ] Step 1: Create local state for optimistic updates and refs for timers.
- [ ] Step 2: Implement the `handleDebouncedClick` logic replacing the direct API call.
- [ ] Step 3: Update the render logic to prioritize optimistic state over SWR data.
- [ ] Verification.

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Setup Local State and Refs
*   **Goal:** Prepare the component to handle temporary states and timers.
*   **Action:**
    *   Modify `app/frontend/src/components/sprints/SprintMoodGrid.tsx`:
        *   Add `pendingMoods` state: `const [pendingMoods, setPendingMoods] = useState<Record<string, MoodType>>({});` (Key format: `${userId}_${date.toISOString()}`).
        *   Add `timeoutsRef`: `const timeoutsRef = useRef<Record<string, NodeJS.Timeout>>({});`.
        *   Import `MoodType` and `MoodValues` correctly if not already done.

### Step 2: Implement Debounced Click Logic
*   **Goal:** Delay the API call while updating the UI immediately.
*   **Action:**
    *   Modify `handleMoodClick` in `app/frontend/src/components/sprints/SprintMoodGrid.tsx`:
        *   Construct a unique key for the cell: `const cellKey = `${user.sub}_${date.toISOString()}`;`.
        *   Determine the `currentMood` (check `pendingMoods[cellKey]` first, then fallback to `moods` from SWR).
        *   Calculate `nextMood` (cycle logic).
        *   **Optimistic Update:** `setPendingMoods(prev => ({ ...prev, [cellKey]: nextMood }));`.
        *   **Clear Timeout:** `if (timeoutsRef.current[cellKey]) clearTimeout(timeoutsRef.current[cellKey]);`.
        *   **Set Timeout:** `timeoutsRef.current[cellKey] = setTimeout(async () => { ...API Call Logic... }, 1000);`.
        *   **Inside Timeout:**
            *   Perform `createMoodEntry` or `updateMoodEntry`.
            *   On Success: `mutateMoods()` (SWR) and `setPendingMoods(prev => { const n = {...prev}; delete n[cellKey]; return n; })`.
            *   On Error: Show Snackbar and remove from `pendingMoods` to revert UI.

### Step 3: Update Render Logic
*   **Goal:** Display the pending mood instantly to the user.
*   **Action:**
    *   In the rendering loop (inside `sprintDates.map`), modify how `moodEntry` is determined or how the color is calculated.
    *   Check `pendingMoods[cellKey]` first.
    *   If a pending mood exists, use it for the color/icon.
    *   If not, use the data from `moods` (SWR).

## 4. 🧪 Testing Strategy
*   **Manual Verification:**
    *   Click a mood cell 3 times in quick succession (e.g., Happy -> Neutral -> Sad).
    *   Observe the UI changing immediately (Green -> Orange -> Red).
    *   Check the Network tab in DevTools: **Only 1** request should be sent after a ~1s delay.
    *   Check persistence: Reload page after the request finishes to ensure data is saved.

## 5. ✅ Success Criteria
*   Rapid clicking triggers only 1 API call per sequence.
*   UI remains responsive and feels instant (no lag waiting for debounce).
*   Notifications are reduced to 1 per interaction sequence.
