export interface DecodedToken {
  sub: string; // Subject (user id)
  name: string;
  email: string;
  avatar_url?: string;
  is_super_admin?: string; // This claim might be optional
  is_onboarded?: string; // "true" or "false"
}

export interface TeamRole {
  isAdmin: boolean;
  isMember: boolean;
}
