import { apiClient } from "../../../services/apiClient";
import type {
  AuthResponseDto,
  EmailActionResponseDto,
  LoginRequest,
  LogoutRequest,
  MessageResponseDto,
  RefreshTokenRequest,
  RegisterRequest,
  RequestPasswordResetRequest,
  ResendVerificationEmailRequest,
  ResetPasswordRequest,
  UserDto,
  VerifyEmailRequest,
} from "../types/auth";

export async function login(request: LoginRequest): Promise<AuthResponseDto> {
  const response = await apiClient.post<AuthResponseDto>("/api/Auth/login", request);
  return response.data;
}

export async function register(request: RegisterRequest): Promise<EmailActionResponseDto> {
  const response = await apiClient.post<EmailActionResponseDto>("/api/Auth/register", request);
  return response.data;
}

export async function requestPasswordReset(
  request: RequestPasswordResetRequest
): Promise<EmailActionResponseDto> {
  const response = await apiClient.post<EmailActionResponseDto>("/api/Auth/password-reset/request", request);
  return response.data;
}

export async function confirmPasswordReset(
  request: ResetPasswordRequest
): Promise<MessageResponseDto> {
  const response = await apiClient.post<MessageResponseDto>("/api/Auth/password-reset/confirm", request);
  return response.data;
}

export async function verifyEmail(
  request: VerifyEmailRequest
): Promise<MessageResponseDto> {
  const response = await apiClient.post<MessageResponseDto>("/api/Auth/verify-email", request);
  return response.data;
}

export async function resendVerificationEmail(
  request: ResendVerificationEmailRequest
): Promise<EmailActionResponseDto> {
  const response = await apiClient.post<EmailActionResponseDto>("/api/Auth/verify-email/resend", request);
  return response.data;
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
