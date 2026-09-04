const STORAGE_KEY = "ops.token";

export const getToken = (): string | null => localStorage.getItem(STORAGE_KEY);

export const setToken = (token: string | null): void => {
  if (token) {
    localStorage.setItem(STORAGE_KEY, token);
  } else {
    localStorage.removeItem(STORAGE_KEY);
  }
};
