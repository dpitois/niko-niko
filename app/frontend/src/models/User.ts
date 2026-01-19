export interface User {
  id: string;
  email?: string; // Optional now
  name: string;
  avatarUrl?: string;
  provider?: string;
  createdAt?: string; // ISO string
  isOnboarded?: boolean;
}
