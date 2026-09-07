export type OrderStatus = "Pending" | "Processing" | "Completed" | "Failed" | "Cancelled";

export const ORDER_STATUS = {
  Pending: "Pending",
  Processing: "Processing",
  Completed: "Completed",
  Failed: "Failed",
  Cancelled: "Cancelled",
} as const satisfies Record<OrderStatus, OrderStatus>;

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiration: string;
}

export interface OrderItemRequest {
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateOrderRequest {
  customerName: string;
  customerEmail?: string;
  items: OrderItemRequest[];
}

// GET /api/orders row shape.
export interface OrderListItem {
  id: number;
  orderNumber: string;
  customerName: string;
  orderDate: string;
  status: OrderStatus;
  totalAmount: number;
}

export interface OrderItem {
  id: number;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface InventoryCheck {
  id: number;
  checkDate: string;
  isAvailable: boolean;
}

// GET /api/orders/{id} shape.
export interface OrderDetail {
  id: number;
  orderNumber: string;
  customerName: string;
  customerEmail: string | null;
  orderDate: string;
  status: OrderStatus;
  totalAmount: number;
  items: OrderItem[];
  inventoryChecks: InventoryCheck[];
}
