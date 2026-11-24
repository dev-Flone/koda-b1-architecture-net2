const API_BASE_URL = import.meta.env.VITE_BASE_URL;

export class ApiError extends Error {
  constructor(status, message) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

export async function apiRequest(
  path,
  { method = "GET", credentials = "omit", headers, body } = {},
) {
  if (!API_BASE_URL) throw new ApiError(0, "VITE_BASE_URL not yet configured");

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    credentials,
    headers: { Accept: "application/json", ...headers },
    body,
  }).catch(() => {
    throw new ApiError(
      0,
      "Cannot called API. Check the server, Network, and CORS",
    );
  });

  const isJson = response.headers.get("content-type")?.includes("json");
  const data = isJson ? await response.json().catch(() => null) : null;

  if (!response.ok) {
    const firstValidationErr = Object.values(data?.errors ?? {}).flat()[0];
    const message =
      response.status === 429
        ? "Too many request. Try again later"
        : response.status >= 500
          ? "Server broken"
          : (firstValidationErr ??
            data?.detail ??
            data?.title ??
            "Request failed.");

    throw new ApiError(response.status, message);
  }
  return data;
}

const validateSession = (data) => {
  if (
    !data?.accessToken ||
    !data?.expiresAtUtc ||
    !data?.user?.id ||
    !data?.user?.email
  ) {
    throw new Error("Authentication format response was not match");
  }
  return data;
};

export const loginUser = async ({ email, password }) =>
  validateSession(
    await apiRequest("/api/v1/auth/login", {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email: email.trim(), password }),
    }),
  );

let refreshInFlight = null;
export const refreshSession = () =>
  (refreshInFlight ??= apiRequest("/api/v1/auth/refresh", {
    method: "POST",
    credentials: "include",
  })
    .then(validateSession)
    .finally(() => (refreshInFlight = null)));

export const getCurrentUser = async (accessToken) => {
  if (!accessToken) throw new ApiError(401, "AccessToken not available");

  return apiRequest("/api/v1/auth/me", {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
};

export const logoutUser = async () => {
  await apiRequest("/api/v1/auth/logout", {
    method: "POST",
    credentials: "include",
  });
};
