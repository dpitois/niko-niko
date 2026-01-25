# Implementation Plan - User Guide & Static Documentation

## 1. 🔍 Analysis & Context
*   **Objective:** Implement a professional static documentation site using Starlight (Astro) including user guides (Standard, Team Admin, Super Admin) and a static export of the Swagger API documentation.
*   **Affected Files:**
    *   `app/frontend/nginx.conf`: Routing for `/docs`.
    *   `docker/Dockerfile.frontend`: Multi-stage build for documentation.
    *   `app/frontend/src/components/Header.tsx` (or similar): Link to documentation.
    *   `New directory: docs/`: Documentation source files.
*   **Key Dependencies:**
    *   **Starlight**: Documentation framework.
    *   **Redoc-cli**: For static Swagger generation.
*   **Risks/Unknowns:**
    *   Synchronization between Backend API changes and static Swagger export.
    *   Ensuring smooth Nginx routing between React SPA and static docs.

## 2. 📋 Checklist
- [x] Step 1: Initialize Starlight project in `docs/`
- [x] Step 2: Configure Static Swagger generation
- [x] Step 3: Draft documentation content (Issue #36)
- [x] Step 4: Update Docker & Nginx configuration
- [x] Step 5: Integrate documentation link in Frontend UI
- [x] Step 6: Fix Nginx redirection and API doc availability
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details
...
### Step 6: Fix Nginx redirection and API doc availability [x]
*   **Goal:** Resolve the "page not responding" error and ensure API documentation is generated.
*   **Action:**
    *   Update `app/frontend/nginx.conf` to use a more robust routing for `/docs`.
    *   Enable Swagger JSON endpoint in backend `Program.cs`.
    *   Add Nginx proxy relay for `/swagger`.
    *   Use ReDoc standalone in `docs/public/api/index.html` pointing to the relayed JSON.
*   **Verification:** Rebuild docker image and verify `/docs/` access and dynamic API content.

