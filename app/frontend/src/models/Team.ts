export interface TeamDto {
  id: string; // GUID is a string in TS/JS
  name: string;
  adminId: string;
  adminName: string;
  createdAt: string; // Dates are strings over HTTP
  defaultSprintDuration?: number;
}
