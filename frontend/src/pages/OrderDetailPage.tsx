import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { extractErrorMessage, getOrder, updateOrderStatus } from "../api/api";
import { ORDER_STATUS } from "../api/types";
import type { OrderDetail, OrderStatus } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { statusColor, validNextStatuses } from "../utils/statusColor";

const PENDING_POLL_INTERVAL_MS = 3000;

function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { isAdmin } = useAuth();

  const [order, setOrder] = useState<OrderDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [updating, setUpdating] = useState(false);
  const [updateError, setUpdateError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      return;
    }
    let cancelled = false;

    setLoading(true);
    getOrder(Number(id))
      .then((data) => {
        if (!cancelled) {
          setOrder(data);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(extractErrorMessage(err, "Could not load this order."));
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [id]);

  useEffect(() => {
    if (!id || order?.status !== ORDER_STATUS.Pending) {
      return;
    }
    let cancelled = false;

    const interval = setInterval(async () => {
      try {
        const refreshed = await getOrder(Number(id));
        if (!cancelled) {
          setOrder(refreshed);
        }
      } catch (ex) {
        console.error("Error refreshing order:", ex);
      }
    }, PENDING_POLL_INTERVAL_MS);

    return () => {
      cancelled = true;
      clearInterval(interval);
    };
  }, [id, order?.status]);

  const handleAdvanceStatus = async (nextStatus: OrderStatus) => {
    if (!order) {
      return;
    }

    setUpdating(true);
    setUpdateError(null);

    try {
      await updateOrderStatus(order.id, nextStatus);
      const refreshed = await getOrder(order.id);
      setOrder(refreshed);
    } catch (err) {
      setUpdateError(extractErrorMessage(err, "Could not update the status."));
    } finally {
      setUpdating(false);
    }
  };

  if (loading) {
    return <CircularProgress />;
  }
  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }
  if (!order) {
    return <Typography>Order not found.</Typography>;
  }

  const timeline: { date: string; status: OrderStatus }[] = [
    { date: order.orderDate, status: ORDER_STATUS.Pending },
    ...order.inventoryChecks
      .slice()
      .sort((a, b) => new Date(a.checkDate).getTime() - new Date(b.checkDate).getTime())
      .map((check) => ({
        date: check.checkDate,
        status: check.isAvailable ? ORDER_STATUS.Processing : ORDER_STATUS.Failed,
      })),
  ];

  const nextStatuses = validNextStatuses(order.status);

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        {order.orderNumber}
      </Typography>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Stack spacing={1}>
          <Typography>
            <strong>Customer:</strong> {order.customerName}
            {order.customerEmail ? ` (${order.customerEmail})` : ""}
          </Typography>
          <Typography>
            <strong>Date:</strong> {new Date(order.orderDate).toLocaleString()}
          </Typography>
          <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
            <Typography component="span">
              <strong>Status:</strong>
            </Typography>
            <Chip label={order.status} color={statusColor(order.status)} size="small" />
            {order.status === ORDER_STATUS.Pending && (
              <>
                <CircularProgress size={14} />
                <Typography variant="caption" color="text.secondary">
                  Waiting for inventory check…
                </Typography>
              </>
            )}
          </Stack>
          <Typography>
            <strong>Total:</strong> ${order.totalAmount.toFixed(2)}
          </Typography>
        </Stack>

        {isAdmin && nextStatuses.length > 0 && (
          <Box sx={{ mt: 2 }}>
            <Typography variant="subtitle2" gutterBottom>
              Update status
            </Typography>
            <Stack direction="row" spacing={1}>
              {nextStatuses.map((next) => (
                <Button key={next} variant="outlined" disabled={updating} onClick={() => handleAdvanceStatus(next)}>
                  Mark {next}
                </Button>
              ))}
            </Stack>
            {updateError && (
              <Alert severity="error" sx={{ mt: 1 }}>
                {updateError}
              </Alert>
            )}
          </Box>
        )}
      </Paper>

      <Typography variant="h6" component="h2" gutterBottom>
        Status history
      </Typography>
      <Paper sx={{ mb: 3 }}>
        <List dense>
          {timeline.map((entry, index) => (
            <ListItem key={index}>
              <ListItemText
                primary={<Chip label={entry.status} color={statusColor(entry.status)} size="small" />}
                secondary={new Date(entry.date).toLocaleString()}
              />
            </ListItem>
          ))}
        </List>
      </Paper>

      <Typography variant="h6" component="h2" gutterBottom>
        Items
      </Typography>
      <TableContainer component={Paper} sx={{ mb: 3 }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Product</TableCell>
              <TableCell align="right">Qty</TableCell>
              <TableCell align="right">Unit price</TableCell>
              <TableCell align="right">Subtotal</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {order.items.map((item) => (
              <TableRow key={item.id}>
                <TableCell>{item.productName}</TableCell>
                <TableCell align="right">{item.quantity}</TableCell>
                <TableCell align="right">${item.unitPrice.toFixed(2)}</TableCell>
                <TableCell align="right">${(item.quantity * item.unitPrice).toFixed(2)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Typography variant="h6" component="h2" gutterBottom>
        Inventory checks
      </Typography>
      {order.inventoryChecks.length === 0 ? (
        <Typography>No inventory check has run yet — this page refreshes automatically while the order is Pending.</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Available</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {order.inventoryChecks.map((check) => (
                <TableRow key={check.id}>
                  <TableCell>{new Date(check.checkDate).toLocaleString()}</TableCell>
                  <TableCell>{check.isAvailable ? "Yes" : "No"}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}

export default OrderDetailPage;
