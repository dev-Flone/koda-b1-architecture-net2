const API_BASE_URL = import.meta.env.VITE_BASE_URL;

export async function loginUser({ email, password }) {
  if (!API_BASE_URL) {
    throw new Error("VITE_BASE_URL not yet configured");
  }

  let response;

  try {
    response = await fetch(`${API_BASE_URL}/api/v1/auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify({
        email: email.trim(),
        password,
      }),
    });
  } catch {
    throw new Error(
      "Can't fetch API. Check the backend and configure the CORS",
    );
  }

  const contentType = response.headers.get("content-type") ?? "";
  const isJson = contentType.includes("json");
  const data = isJson ? await response.json().catch(() => null) : null;

  if (!response.ok) {
    if (response.status == 401) throw new Error("Incorrect Email or Password.");

    if (response.status == 400) {
      const errors = Object.values(data?.errors ?? {}).flat();
      throw new Error(errors[0] ?? "Login Credential is not valid.");
    }

    if (response.status === 429)
      throw new Error("Too much request. Try again later.");

    throw new Error(
      response.status >= 500
        ? "Server broken."
        : "Failed to Login. Try again later.",
    );
  }

  if (!data?.user?.id || !data?.user?.email)
    throw new Error("Format Response Api was not matched.");

  return data;
}

export async function registerUser({ email, password, id, displayName }) {
  if (!API_BASE_URL) {
    throw new Error("API_BASE_URL is not configured.");
  }

  let response;

  try {
    response = await fetch(`${API_BASE_URL}/api/v1/auth/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify({
        email: email.trim(),
        password,
        id,
        displayName: displayName.trim(),
      }),
    });
  } catch {
    throw new Error(
      "Unable to reach the server. Check your connection or backend CORS settings.",
    );
  }

  const contentType = response.headers.get("content-type") ?? "";
  const isJson = contentType.includes("json");
  const data = isJson ? await response.json().catch(() => null) : null;

  if (!response.ok) {
    if (response.status === 409) {
      throw new Error("An account with this email already exists.");
    }

    if (response.status === 400 || response.status === 422) {
      const validationErrors = Object.values(data?.errors ?? {}).flat();
      throw new Error(validationErrors[0] ?? "Invalid registration details.");
    }

    if (response.status === 429) {
      throw new Error("Too many requests. Please try again later.");
    }

    throw new Error(
      response.status >= 500
        ? "Server error. Please try again later."
        : "Registration failed. Please try again.",
    );
  }

  if (!data?.id || !data?.email) {
    throw new Error("Unexpected API response format.");
  }

  return data;
}
