# Implementation Plan - Update Frontend DTO Model

## 1. 🔍 Analysis & Context
*   **Objective:** Update the frontend TypeScript interface `CreateMood` to reflect the new field `TimezoneOffset` added to the backend DTO, improving type safety and documentation.
*   **Affected Files:**
    *   `app/frontend/src/models/CreateMood.ts`
*   **Key Dependencies:** None.
*   **Risks/Unknowns:** None.

## 2. 📋 Checklist
- [ ] Step 1: Update `CreateMood` interface.
- [ ] Verification: Lint and then Build frontend.

## 3. 📝 Step-by-Step Implementation Details

### Step 1: Update Interface
*   **Goal:** Align Frontend Model with Backend DTO.
*   **Action:**
    *   Modify `app/frontend/src/models/CreateMood.ts`.
    *   Add `timezoneOffset?: number;` (Optional).
    *   *Note:* Making it optional (`?`) is crucial because `MoodEntryForm` creates this object without the offset (which is injected by the service later). If we made it required, we would break the UI component.
    *   Code:
        ```typescript
        export interface CreateMood {
          userId: string;
          sprintId: string;
          mood: MoodType;
          date?: string;
          timezoneOffset?: number; // Added to match backend DTO
        }
        ```

## 4. 🧪 Testing Strategy
*   **Verification:**
    *   Run `npm run lint` in `app/frontend` (ensure code style/quality passes).
    *   Run `npm run build` in `app/frontend` (ensure compilation passes).

## 5. ✅ Success Criteria
*   The `CreateMood` interface explicitly includes `timezoneOffset`.