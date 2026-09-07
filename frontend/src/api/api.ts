import axios from "axios";
import { getToken } from "../auth/token";
import type { CreateOrderRequest, LoginRequest, LoginResponse, OrderDetail, OrderListItem } from "./types";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000/api",
});

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let onUnauthorized: (() => void) | null = null;

export const setUnauthorizedHandler = (handler: (() => void) | null): void => {
  onUnauthorized = handler;
};

api.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (
      axios.isAxiosError(error) &&
      error.response?.status === 401 &&
      !error.config?.url?.includes("/Security/authenticate")
    ) {
      onUnauthorized?.();
    }
    return Promise.reject(error);
  },
);

export const login = async (payload: LoginRequest): Promise<LoginResponse> => {
  const { data } = await api.post<LoginResponse>("/Security/authenticate", payload);
  return data;
};

export interface GetOrdersParams {
  status?: string;
  from?: string;
  to?: string;
}

export const getOrders = async (params?: GetOrdersParams): Promise<OrderListItem[]> => {
  const { data } = await api.get<OrderListItem[]>("/orders", { params });
  return data;
};

export const getOrder = async (id: number): Promise<OrderDetail> => {
  const { data } = await api.get<OrderDetail>(`/orders/${id}`);
  return data;
};

export const createOrder = async (payload: CreateOrderRequest): Promise<OrderDetail> => {
  const { data } = await api.post<OrderDetail>("/orders", payload);
  return data;
};

export const updateOrderStatus = async (id: number, status: string): Promise<void> => {
  await api.put(`/orders/${id}/status`, { status });
};

export function extractErrorMessage(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { error?: string } | undefined;
    if (data?.error) {
      return data.error;
    }
  }
  return fallback;
}

export default api;
