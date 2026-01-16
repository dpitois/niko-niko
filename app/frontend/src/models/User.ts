export interface User {
  id: string;
  email?: string; // Optional now
  name: string;
  avatarUrl?: string;
  createdAt?: string; // ISO string
}
