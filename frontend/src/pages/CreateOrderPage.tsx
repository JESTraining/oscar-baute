import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { createOrder, extractErrorMessage } from "../api/api";
import type { OrderItemRequest } from "../api/types";

const emptyItem = (): OrderItemRequest => ({ productName: "", quantity: 1, unitPrice: 0 });

function CreateOrderPage() {
  const navigate = useNavigate();

  const [customerName, setCustomerName] = useState("");
  const [customerEmail, setCustomerEmail] = useState("");
  const [items, setItems] = useState<OrderItemRequest[]>([emptyItem()]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const updateItem = (index: number, changes: Partial<OrderItemRequest>) => {
    setItems((current) => current.map((item, i) => (i === index ? { ...item, ...changes } : item)));
  };

  const addItem = () => setItems((current) => [...current, emptyItem()]);

  const removeItem = (index: number) => setItems((current) => current.filter((_, i) => i !== index));

  const total = items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0);

  const validate = (): string | null => {
    if (customerName.trim() === "") {
      return "Customer name is required.";
    }
    if (items.length === 0) {
      return "At least one order item is required.";
    }
    if (items.some((i) => i.quantity <= 0)) {
      return "Every item quantity must be greater than zero.";
    }
    if (items.some((i) => i.unitPrice < 0)) {
      return "Item unit price cannot be negative.";
    }
    return null;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }

    setError(null);
    setSubmitting(true);

    try {
      const order = await createOrder({
        customerName,
        customerEmail: customerEmail.trim() === "" ? undefined : customerEmail,
        items,
      });
      navigate(`/orders/${order.id}`);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not create the order."));
      setSubmitting(false);
    }
  };

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        New Order
      </Typography>

      <Paper sx={{ p: 3, maxWidth: 900 }}>
        <Box component="form" onSubmit={handleSubmit}>
          <Stack spacing={2}>
            <TextField
              label="Customer name"
              value={customerName}
              onChange={(e) => setCustomerName(e.target.value)}
              required
              fullWidth
            />
            <TextField
              label="Customer email"
              type="email"
              value={customerEmail}
              onChange={(e) => setCustomerEmail(e.target.value)}
              fullWidth
            />

            <Typography variant="h6" component="h2">
              Items
            </Typography>

            {items.map((item, index) => (
              <Stack direction="row" spacing={1} sx={{ alignItems: "flex-start" }} key={index}>
                <TextField
                  label="Product name"
                  value={item.productName}
                  onChange={(e) => updateItem(index, { productName: e.target.value })}
                  required
                  fullWidth
                />
                <TextField
                  label="Qty"
                  type="number"
                  value={item.quantity}
                  onChange={(e) => updateItem(index, { quantity: Number(e.target.value) })}
                  required
                  slotProps={{ htmlInput: { min: 1 } }}
                  sx={{ width: 100 }}
                />
                <TextField
                  label="Unit price"
                  type="number"
                  value={item.unitPrice}
                  onChange={(e) => updateItem(index, { unitPrice: Number(e.target.value) })}
                  required
                  slotProps={{ htmlInput: { min: 0, step: 0.1 } }}
                  sx={{ width: 180 }}
                />
                <Button onClick={() => removeItem(index)} disabled={items.length === 1} sx={{ mt: 1 }}>
                  Remove
                </Button>
              </Stack>
            ))}

            <Button onClick={addItem} variant="outlined" sx={{ alignSelf: "flex-start" }}>
              + Add item
            </Button>

            <Typography variant="subtitle1">
              Total: ${total.toFixed(2)}
            </Typography>

            {error && <Alert severity="error">{error}</Alert>}

            <Box>
              <Button type="submit" variant="contained" disabled={submitting}>
                {submitting ? "Creating…" : "Create order"}
              </Button>
            </Box>
          </Stack>
        </Box>
      </Paper>
    </Box>
  );
}

export default CreateOrderPage;
