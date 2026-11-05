export interface UserInfo {
  id: number;
  username: string;
}

export interface AuthResponse {
  token: string;
  userId: number;
  username: string;
}
