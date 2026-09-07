import type { ChipProps } from "@mui/material/Chip";
import { ORDER_STATUS } from "../api/types";
import type { OrderStatus } from "../api/types";

export function statusColor(status: OrderStatus): ChipProps["color"] {
  switch (status) {
    case ORDER_STATUS.Pending:
      return "warning";
    case ORDER_STATUS.Processing:
      return "info";
    case ORDER_STATUS.Completed:
      return "success";
    case ORDER_STATUS.Failed:
      return "error";
    case ORDER_STATUS.Cancelled:
    default:
      return "default";
  }
}

const VALID_TRANSITIONS: Record<OrderStatus, OrderStatus[]> = {
  [ORDER_STATUS.Pending]: [ORDER_STATUS.Processing, ORDER_STATUS.Cancelled],
  [ORDER_STATUS.Processing]: [ORDER_STATUS.Completed, ORDER_STATUS.Failed, ORDER_STATUS.Cancelled],
  [ORDER_STATUS.Completed]: [],
  [ORDER_STATUS.Failed]: [],
  [ORDER_STATUS.Cancelled]: [],
};

export function validNextStatuses(status: OrderStatus): OrderStatus[] {
  return VALID_TRANSITIONS[status];
}
