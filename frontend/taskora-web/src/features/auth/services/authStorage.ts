const ACCESS_TOKEN_KEY = "taskora_access_token";
const REFRESH_TOKEN_KEY = "taskora_refresh_token";
const USER_ID_KEY = "taskora_user_id";
const USER_NAME_KEY = "taskora_user_name";
const USER_EMAIL_KEY = "taskora_user_email";

export function saveAuthSession(
  accessToken: string,
  refreshToken: string,
  userId: number | string,
  name: string,
  email: string
): void {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  localStorage.setItem(USER_ID_KEY, String(userId));
  localStorage.setItem(USER_NAME_KEY, name);
  localStorage.setItem(USER_EMAIL_KEY, email);
}

export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function getStoredUserName(): string {
  return localStorage.getItem(USER_NAME_KEY) ?? "";
}

export function updateTokens(accessToken: string, refreshToken: string): void {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
}

export function clearAuthSession(): void {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(USER_ID_KEY);
  localStorage.removeItem(USER_NAME_KEY);
  localStorage.removeItem(USER_EMAIL_KEY);
}