export interface TeamInvitation {
  id: string;
  teamId: string;
  teamName: string;
  creatorUserId: string;
  creatorUserName: string;
  expirationDate: string; // ISO string
  token: string;
  status: string; // e.g., "Pending", "Accepted", "Expired"
}
