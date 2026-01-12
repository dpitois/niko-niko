export interface TeamDto {
  id: string; // GUID is a string in TS/JS
  name: string;
  adminId: string;
  createdAt: string; // Dates are strings over HTTP
}
