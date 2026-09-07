import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Link from "@mui/material/Link";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { useEffect, useState } from "react";
import { Link as RouterLink } from "react-router-dom";
import { extractErrorMessage, getOrders } from "../api/api";
import type { OrderListItem } from "../api/types";
import { statusColor } from "../utils/statusColor";

const POLL_INTERVAL_MS = 10000;

function OrdersListPage() {
  const [orders, setOrders] = useState<OrderListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);

  useEffect(() => {
    let cancelled = false;
    let isFirstLoad = true;

    const fetchOrders = async () => {

      if (isFirstLoad) {
        setLoading(true);
      }

      try {
        const data = await getOrders();
        if (cancelled) {
          return;
        }
        
        setOrders(data);
        setLastUpdated(new Date());
        setError(null);
      } catch (err) {
        if (!cancelled) {
          setError(extractErrorMessage(err, "Could not load orders."));
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
        isFirstLoad = false;
      }
    };

    fetchOrders();
    const interval = setInterval(fetchOrders, POLL_INTERVAL_MS);

    return () => {
      cancelled = true;
      clearInterval(interval);
    };
  }, []);

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Orders
      </Typography>

      {lastUpdated && (
        <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 1 }}>
          Last updated {lastUpdated.toLocaleTimeString()} — refreshes every 10s
        </Typography>
      )}

      {loading && <CircularProgress />}
      {error && <Alert severity="error">{error}</Alert>}

      {!loading && !error && orders.length === 0 && (
        <Typography>
          No orders yet.{" "}
          <Link component={RouterLink} to="/orders/new">
            Create one
          </Link>
          .
        </Typography>
      )}

      {!loading && !error && orders.length > 0 && (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Order #</TableCell>
                <TableCell>Customer</TableCell>
                <TableCell>Date</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Total</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {orders.map((order) => (
                <TableRow key={order.id} hover>
                  <TableCell>
                    <Link component={RouterLink} to={`/orders/${order.id}`}>
                      {order.orderNumber}
                    </Link>
                  </TableCell>
                  <TableCell>{order.customerName}</TableCell>
                  <TableCell>{new Date(order.orderDate).toLocaleString()}</TableCell>
                  <TableCell>
                    <Chip label={order.status} color={statusColor(order.status)} size="small" />
                  </TableCell>
                  <TableCell align="right">${order.totalAmount.toFixed(2)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}

export default OrdersListPage;
