import { HttpError, ProblemDetails } from "./models/problem-details";
import { correlationInterceptor } from "./interceptors/correlation.interceptor";
import { tenantInterceptor } from "./interceptors/tenant.interceptor";
import { idempotencyInterceptor } from "./interceptors/idempotency.interceptor";
import {
  executeWithRetry,
  RetryConfig,
} from "./interceptors/retry.interceptor";

export interface RequestOptions extends RequestInit {
  tenantId?: string | null;
  authToken?: string | null;
  retryConfig?: RetryConfig;
  params?: Record<string, string | number | boolean | undefined | null>;
}

export class ApiClient {
  private readonly baseUrl: string;

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl ?? process.env["NEXT_PUBLIC_API_URL"] ?? "/api";
  }

  private buildUrl(
    endpoint: string,
    params?: Record<string, string | number | boolean | undefined | null>
  ): string {
    const cleanEndpoint = endpoint.startsWith("/") ? endpoint : `/${endpoint}`;
    const url = new URL(`${this.baseUrl}${cleanEndpoint}`, "http://localhost");

    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          url.searchParams.append(key, String(value));
        }
      });
    }

    // If running in browser or relative, return relative path with query
    if (this.baseUrl.startsWith("/")) {
      return `${url.pathname}${url.search}`;
    }

    return url.toString();
  }

  async request<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
    const {
      tenantId,
      authToken,
      retryConfig,
      params,
      headers: customHeaders,
      ...fetchOptions
    } = options;

    const method = (fetchOptions.method ?? "GET").toUpperCase();
    const headers = new Headers(customHeaders);

    // Apply Interceptor Chain
    correlationInterceptor(headers);
    tenantInterceptor(headers, tenantId);
    idempotencyInterceptor(method, headers);

    if (authToken && !headers.has("Authorization")) {
      headers.set("Authorization", `Bearer ${authToken}`);
    }

    if (!headers.has("Content-Type") && fetchOptions.body) {
      headers.set("Content-Type", "application/json");
    }

    const targetUrl = this.buildUrl(endpoint, params);

    return executeWithRetry(
      async () => {
        const response = await fetch(targetUrl, {
          ...fetchOptions,
          method,
          headers,
        });

        if (!response.ok) {
          let problem: ProblemDetails;
          try {
            problem = (await response.json()) as ProblemDetails;
          } catch {
            problem = {
              title: response.statusText || "HTTP Error",
              status: response.status,
              detail: `Request failed with status ${response.status}`,
            };
          }
          throw new HttpError(problem);
        }

        if (response.status === 204) {
          return undefined as T;
        }

        return (await response.json()) as T;
      },
      method,
      retryConfig
    );
  }

  get<T>(endpoint: string, options?: RequestOptions): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: "GET" });
  }

  post<T>(
    endpoint: string,
    body?: unknown,
    options?: RequestOptions
  ): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: "POST",
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  }

  put<T>(
    endpoint: string,
    body?: unknown,
    options?: RequestOptions
  ): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: "PUT",
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  }

  patch<T>(
    endpoint: string,
    body?: unknown,
    options?: RequestOptions
  ): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: "PATCH",
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  }

  delete<T>(endpoint: string, options?: RequestOptions): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: "DELETE" });
  }
}

export const apiClient = new ApiClient();
