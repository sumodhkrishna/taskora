import { apiClient } from "../../../services/apiClient";
import type {
  AuthResponseDto,
  LoginRequest,
  LogoutRequest,
  RefreshTokenRequest,
  RegisterRequest,
  RequestPasswordResetRequest,
  ResetPasswordRequest,
  UserDto,
} from "../types/auth";

export async function login(request: LoginRequest): Promise<AuthResponseDto> {
  const response = await apiClient.post<AuthResponseDto>("/api/Auth/login", request);
  return response.data;
}

export async function register(request: RegisterRequest): Promise<UserDto> {
  const response = await apiClient.post<UserDto>("/api/Auth/register", request);
  return response.data;
}

export async function requestPasswordReset(
  request: RequestPasswordResetRequest
): Promise<void> {
  await apiClient.post("/api/Auth/password-reset/request", request);
}

export async function confirmPasswordReset(
  request: ResetPasswordRequest
): Promise<void> {
  await apiClient.post("/api/Auth/password-reset/confirm", request);
}

export async function refreshAccessToken(
  request: RefreshTokenRequest
): Promise<AuthResponseDto> {
  const response = await apiClient.post<AuthResponseDto>("/api/Auth/refresh", request);
  return response.data;
}

export async function logout(request: LogoutRequest): Promise<void> {
  await apiClient.post("/api/Auth/logout", request);
}

export async function getCurrentUser(): Promise<UserDto> {
  const response = await apiClient.get<UserDto>("/api/Users/me");
  return response.data;
}