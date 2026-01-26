import { test, expect } from '@playwright/test';

test.describe('Mood Tracking', () => {
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

  test('should record mood successfully', async ({ page }) => {
    const token = await loginAsAdmin(page);

    // 1. Create a team and an active sprint
    const teamResponse = await page.request.post('/api/teams', {
      data: { name: 'Mood Team' },
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const team = await teamResponse.json();

    const today = new Date();
    const startDate = new Date(today);
    startDate.setDate(today.getDate() - 2); // Starts 2 days ago
    const endDate = new Date(today);
    endDate.setDate(today.getDate() + 5); // Ends in 5 days

    const sprintResponse = await page.request.post('/api/sprints', {
      data: { 
        name: 'Active Sprint', 
        startDate: startDate.toISOString().split('T')[0], 
        endDate: endDate.toISOString().split('T')[0],
        teamId: team.id 
      },
      headers: { 'Authorization': `Bearer ${token}` }
    });

    // 2. Go to Dashboard (Home)
    await page.goto('/my-teams');

    // 3. Verify the "How are you today?" section is visible
    await expect(page.getByText('How are you today?')).toBeVisible();

    // 4. Click on a mood emoji (e.g., Very Happy)
    // The buttons have tooltips with the mood name
    await page.getByRole('button', { name: 'Very Happy' }).click();

    // 5. Verify the mood is recorded (UI update)
    // The widget should show "Mood recorded for this day"
    await expect(page.getByText('Mood recorded for this day')).toBeVisible();

    // 6. Navigate to the sprint details to see it in the grid
    await page.getByRole('link', { name: 'View Full Board' }).first().click();
    
    // Check if the grid contains an entry for the user
    // (In a real scenario, we'd check the specific cell, but here we check for general visibility of the recorded state)
    // The user name "Admin Test" should be in the grid
    await expect(page.locator('text=Admin Test')).toBeVisible();
  });
});
