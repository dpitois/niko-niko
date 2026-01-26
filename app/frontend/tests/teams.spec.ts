import { test, expect } from '@playwright/test';

test.describe('Team Management', () => {
  test.beforeEach(async ({ page }) => {
    // Reset database before each test
    await page.request.post('/api/testing/reset');
  });

  test('should create a new team successfully', async ({ page }) => {
    // 1. Login as admin
    const loginResponse = await page.request.post('/api/testing/login', {
      data: { email: 'admin@test.com' }
    });
    expect(loginResponse.ok()).toBeTruthy();
    const { token } = await loginResponse.json();
    expect(token).toBeDefined();

    // Set token in localStorage and navigate
    // We go to a neutral page first to set the localStorage for the domain
    await page.goto('/login');
    await page.evaluate((jwt) => {
      localStorage.setItem('jwt_token', jwt);
    }, token);

    // 2. Navigate to Admin Teams page
    await page.goto('/admin/teams');
    await expect(page).toHaveURL(/\/admin\/teams/);

    // 3. Fill the team creation form
    const teamName = 'E2E Test Team';
    await page.getByLabel('New Team Name').fill(teamName);
    await page.getByLabel('Default Sprint Duration (days)').fill('14');

    // 4. Click create
    await page.getByRole('button', { name: 'Create Team' }).click();

    // 5. Verify the team appears in the list
    await expect(page.locator('text=' + teamName)).toBeVisible();
    
    // Check if the success snackbar or updated list is there
    // The team name should be rendered in an AdminTeamListItem
    const teamCard = page.locator('div', { hasText: teamName }).first();
    await expect(teamCard).toBeVisible();
  });

  test('should edit a team successfully', async ({ page }) => {
    // 1. Login as admin
    const loginResponse = await page.request.post('/api/testing/login', {
      data: { email: 'admin@test.com' }
    });
    const { token } = await loginResponse.json();

    await page.goto('/login');
    await page.evaluate((jwt) => {
      localStorage.setItem('jwt_token', jwt);
    }, token);

    // 2. Create a team via API to have something to edit
    await page.request.post('/api/teams', {
      data: { name: 'To Be Edited', defaultSprintDuration: 10 },
      headers: { 'Authorization': `Bearer ${token}` }
    });

    // 3. Navigate to Admin Teams page
    await page.goto('/admin/teams');
    
    // 4. Click edit button for our team
    // Find the card containing 'To Be Edited' and click the edit icon (Edit team name)
    const teamCard = page.locator('div', { hasText: 'To Be Edited' }).first();
    await teamCard.getByRole('button', { name: 'Edit team name' }).click();

    // 5. Change name and default duration
    const dialog = page.getByRole('dialog');
    const newName = 'Edited Team Name';
    await dialog.getByLabel('Team Name').fill(newName);
    await dialog.getByLabel('Default Sprint Duration (days)').fill('21');
    
    // 6. Save changes
    await dialog.getByRole('button', { name: 'Save' }).click();

    // 7. Verify changes
    await expect(page.locator('text=' + newName)).toBeVisible();
    await expect(page.locator('text=To Be Edited')).not.toBeVisible();
  });

  test('should delete a team successfully', async ({ page }) => {
    // 1. Login as admin
    const loginResponse = await page.request.post('/api/testing/login', {
      data: { email: 'admin@test.com' }
    });
    const { token } = await loginResponse.json();

    await page.goto('/login');
    await page.evaluate((jwt) => {
      localStorage.setItem('jwt_token', jwt);
    }, token);

    // 2. Create a team via API
    await page.request.post('/api/teams', {
      data: { name: 'To Be Deleted', defaultSprintDuration: 7 },
      headers: { 'Authorization': `Bearer ${token}` }
    });

    // 3. Navigate to Admin Teams page
    await page.goto('/admin/teams');
    
    // 4. Handle confirmation dialog (confirm delete)
    page.on('dialog', dialog => dialog.accept());

    // 5. Click delete button
    const teamCard = page.locator('div', { hasText: 'To Be Deleted' }).first();
    await teamCard.getByRole('button', { name: 'Delete' }).click();

    // 6. Verify it's gone
    await expect(page.locator('text=To Be Deleted')).not.toBeVisible();
  });
});
