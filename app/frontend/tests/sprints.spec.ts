import { test, expect } from '@playwright/test';

test.describe('Sprint Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.request.post('/api/testing/reset');
  });

  async function loginAsAdmin(page) {
    const loginResponse = await page.request.post('/api/testing/login', {
      data: { email: 'admin@test.com' }
    });
    const { token } = await loginResponse.json();
    await page.goto('/login');
    await page.evaluate((jwt) => {
      localStorage.setItem('jwt_token', jwt);
    }, token);
    return token;
  }

  test('should create a new sprint successfully', async ({ page }) => {
    const token = await loginAsAdmin(page);

    // 1. Create a team first via API
    const teamName = 'Sprint Test Team';
    const teamResponse = await page.request.post('/api/teams', {
      data: { name: teamName, defaultSprintDuration: 14 },
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const team = await teamResponse.json();

    // 2. Navigate to Admin Sprints page
    await page.goto('/admin/sprints');

    // 3. Fill the sprint creation form
    await page.getByLabel('Team').click();
    await page.getByRole('option', { name: teamName }).click();

    const sprintName = 'Sprint 1';
    await page.getByLabel('Sprint Name').fill(sprintName);
    
    // Dates are automatically filled based on template/defaults usually, but let's be explicit
    // We'll use values that work
    await page.getByLabel('Start Date').fill('2026-01-01');
    await page.getByLabel('End Date').fill('2026-01-14');

    // 4. Click create
    await page.getByRole('button', { name: 'Create' }).click();

    // 5. Verify the sprint appears in the list
    await expect(page.locator('text=' + sprintName)).toBeVisible();
    await expect(page.locator('text=' + teamName).first()).toBeVisible();
  });

  test('should delete a sprint successfully', async ({ page }) => {
    const token = await loginAsAdmin(page);

    // 1. Create team and sprint via API
    const teamResponse = await page.request.post('/api/teams', {
      data: { name: 'Delete Sprint Team' },
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const team = await teamResponse.json();

    const sprintResponse = await page.request.post('/api/sprints', {
      data: { 
        name: 'Sprint to Delete', 
        startDate: '2026-02-01', 
        endDate: '2026-02-14',
        teamId: team.id 
      },
      headers: { 'Authorization': `Bearer ${token}` }
    });

    // 2. Navigate to Admin Sprints page
    await page.goto('/admin/sprints');

    // 3. Click delete on the sprint card to open dialog
    const sprintCard = page.locator('div', { hasText: 'Sprint to Delete' }).first();
    await sprintCard.getByRole('button', { name: 'Delete' }).click();

    // 4. Click delete inside the confirmation dialog
    const dialog = page.getByRole('dialog');
    await dialog.getByRole('button', { name: 'Delete' }).click();

    // Wait for dialog to disappear
    await expect(dialog).not.toBeVisible();

    // 5. Verify it's gone from the list
    await expect(page.locator('div', { hasText: 'Sprint to Delete' }).first()).not.toBeVisible();
  });
});
