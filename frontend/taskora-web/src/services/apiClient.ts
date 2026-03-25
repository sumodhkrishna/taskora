import axios, {
  AxiosError,
  type InternalAxiosRequestConfig,
} from "axios";
import {
  clearAuthSession,
  getAccessToken,
  getRefreshToken,
  updateTokens,
} from "../features/auth/services/authStorage";

type RetriableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean;
};

function isAuthEndpoint(url?: string): boolean {
  return Boolean(url && url.startsWith("/api/Auth/"));
}

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

const refreshClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

let refreshPromise: Promise<string | null> | null = null;

async function performTokenRefresh(): Promise<string | null> {
  const refreshToken = getRefreshToken();

  if (!refreshToken) {
    clearAuthSession();
    window.location.href = "/";
    return null;
  }

  try {
    const response = await refreshClient.post("/api/Auth/refresh", {
      refreshToken,
    });

    const data = response.data as {
      accessToken: string;
      refreshToken: string;
    };

    if (!data?.accessToken || !data?.refreshToken) {
      clearAuthSession();
      window.location.href = "/";
      return null;
    }

    updateTokens(data.accessToken, data.refreshToken);
    return data.accessToken;
  } catch {
    clearAuthSession();
    window.location.href = "/";
    return null;
  }
}

apiClient.interceptors.request.use((config) => {
  const token = getAccessToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined;

    if (!originalRequest) {
      return Promise.reject(error);
    }

    const status = error.response?.status;

    if (status !== 401 || originalRequest._retry || isAuthEndpoint(originalRequest.url)) {
      return Promise.reject(error);
    }

    if (!getRefreshToken()) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    if (!refreshPromise) {
      refreshPromise = performTokenRefresh().finally(() => {
        refreshPromise = null;
      });
    }

    const newAccessToken = await refreshPromise;

    if (!newAccessToken) {
      return Promise.reject(error);
    }

    originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
    return apiClient(originalRequest);
  }
);
