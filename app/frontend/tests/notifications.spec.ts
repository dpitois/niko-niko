import { test, expect, Page } from '@playwright/test';

test.describe('Real-time Notifications (SignalR)', () => {
  test.beforeEach(async ({ page }) => {
    await page.request.post('/api/testing/reset');
  });

  async function loginAs(page: Page, email: string) {
    const loginResponse = await page.request.post('/api/testing/login', {
      data: { email }
    });
    const { token } = await loginResponse.json();
    await page.goto('/login');
    await page.evaluate((jwt) => {
      localStorage.setItem('jwt_token', jwt);
    }, token);
    return token;
  }

  test.skip('should receive a notification when a team is renamed', async ({ browser }) => {
    // 1. Create two contexts
    const adminContext = await browser.newContext();
    const memberContext = await browser.newContext();

    const adminPage = await adminContext.newPage();
    const memberPage = await memberContext.newPage();

    // 2. Admin setup
    const adminToken = await loginAs(adminPage, 'admin@test.com');
    
    // Create Team
    const teamName = 'SignalR Team';
    const teamResponse = await adminPage.request.post('/api/teams', {
      data: { name: teamName },
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const team = await teamResponse.json();

    // Create Invitation
    const inviteResponse = await adminPage.request.post(`/api/teams/${team.id}/invitations`, {
      data: { teamId: team.id, expirationDate: new Date(Date.now() + 86400000).toISOString() },
      headers: { 'Authorization': `Bearer ${adminToken}` }
    });
    const invite = await inviteResponse.json();
    const inviteLink = `/accept-invitation?token=${invite.token}`;

    // 3. Member setup & Join Team
    const memberToken = await loginAs(memberPage, 'member@test.com');
    
    // Accept Invitation via API
    const acceptResponse = await memberPage.request.post(`/api/teaminvitations/${invite.token}/accept`, {
      headers: { 'Authorization': `Bearer ${memberToken}` }
    });
    expect(acceptResponse.ok()).toBeTruthy();
    
    // Go to dashboard and wait for SignalR
    await memberPage.goto('/my-teams');
    // Ensure the team is visible on dashboard (confirms data loaded)
    await expect(memberPage.getByText(teamName)).toBeVisible();
    // Wait a bit for SignalR group registration to propagate
    await memberPage.waitForTimeout(2000);

    // 4. Admin renames the team
    await adminPage.goto('/admin/teams');
    const teamCard = adminPage.locator('div', { hasText: teamName }).first();
    await teamCard.getByRole('button', { name: 'Edit team name' }).click();

    const newName = 'Renamed for Member';
    const dialog = adminPage.getByRole('dialog');
    await dialog.getByLabel('Team Name').fill(newName);
    await dialog.getByRole('button', { name: 'Save' }).click();

    // 5. Verify Member receives notification
    await expect(memberPage.locator(`text=A team has been renamed to "${newName}"`)).toBeVisible({ timeout: 15000 });

    await adminContext.close();
    await memberContext.close();
  });
});
