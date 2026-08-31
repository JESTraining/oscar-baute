## Duration: 4 Evenings (approx. 2-3 hours per evening, ~10 hours total)

## Stack
- Backend: .NET 8 (C#)
- Frontend: Angular 17+ or React 18+ (your choice)
- Database: MS SQL Server
- ORM: Entity Framework Core
- Messaging: RabbitMQ
- Containerization: Docker & Kubernetes
- Additional: JWT Authentication, Logging

---

## Exercise: Order Processing System

### Business Context

You are building a simplified order management system for an e-commerce company. The system handles order intake, processing, inventory validation, and status tracking.

---

### Requirements

#### Evening 1: Setup & Foundation (2-3 hours)

**1. Project Setup**
- Create a solution with:
  - `API` project (ASP.NET Core Web API)
  - `Core` project (Domain models, Interfaces)
  - `Infrastructure` project (Data, Repositories, Services)
  - `Tests` project (Unit tests - optional but encouraged)
- Set up Docker Compose with:
  - SQL Server container
  - RabbitMQ container
  - API container (later)

**2. Database & EF Core**
- Design a clean schema for:
  - `Orders`: Id, OrderNumber (unique), CustomerName, CustomerEmail, OrderDate, Status (Pending/Processing/Completed/Failed/Cancelled), TotalAmount
  - `OrderItems`: Id, OrderId, ProductName, Quantity, UnitPrice
  - `InventoryChecks`: Id, OrderId, CheckDate, IsAvailable
- Create EF Core migrations
- Implement DbContext with proper relationships

**3. Models & DTOs**
- Domain entities
- Request/Response DTOs for all endpoints

---

#### Evening 2: Backend APIs & Logic (2-3 hours)

**4. REST APIs**
- `POST /api/orders` — Create new order
  - Validate: at least one item, quantity > 0, customer name required
  - Generate OrderNumber (e.g., ORD-001)
  - Calculate TotalAmount automatically
  - Status starts as Pending

- `GET /api/orders` — List all orders
  - Show: OrderNumber, CustomerName, Date, Status, TotalAmount
  - Filter by Status
  - Filter by Date Range

- `GET /api/orders/{id}` — Get order details with items

- `PUT /api/orders/{id}/status` — Update status (Admin only)
  - Add simple validation for status flow

**5. Business Logic**
- Implement OrderService with validation rules
- Repository pattern or direct DbContext usage
- Proper async/await throughout

---

#### Evening 3: Messaging & Frontend Start (2-3 hours)

**6. RabbitMQ Integration**
- Install RabbitMQ.Client NuGet package
- Publish `OrderCreatedEvent` when order is created
- Create a consumer (background service) that:
  - Listens for OrderCreatedEvent
  - Simulates inventory check (random pass/fail)
  - Updates order status to Processing (if pass) or Failed (if fail)
  - Logs the check result

**7. Frontend Foundation**
- Set up Angular or React project
- Create basic UI structure:
  - Navigation bar
  - Orders list page
  - Create order form
  - Order detail page

**8. API Integration**
- Create service to call backend APIs
- Display orders list with status
- Show status updates (manual refresh or simple polling)

---

#### Evening 4: Frontend Completion & Docker/K8s (2-3 hours)

**9. Frontend Polish**
- Complete the create order form
- Implement order detail view with status history
- Add polling every 10 seconds to refresh order list
- Basic styling (use a CSS framework or keep it clean)

**10. Containerization**
- Create `Dockerfile` for backend API
- Create `docker-compose.yml` with all services:
  - API
  - SQL Server
  - RabbitMQ
- Create basic Kubernetes deployment YAML:
  - Deployment with 2 replicas
  - Service (ClusterIP)
  - ConfigMap for environment variables

### Important Notes

- **No AI tools allowed** (ChatGPT, Copilot, Claude, etc.)
- You may use official documentation (MSDN, Angular/React docs, etc.)
- You may search for syntax and references
- Focus on delivering a **working** core solution over perfection
- It's okay to leave nice-to-have features incomplete
- Quality over quantity

---

### Optional Bonus Points (if time allows)

- Unit tests for business logic
- FluentValidation for request validation
- Swagger/OpenAPI documentation
- Better UI/UX with loading states and error handling
- SignalR for real-time order status updates instead of polling
- Health checks endpoint
- API versioning

---

### Suggested Time Management

| Evening | Focus | Must Complete |
| :--- | :--- | :--- |
| 1 | Setup & Database | DbContext, migrations, models |
| 2 | Backend APIs | POST, GET list, GET detail, status update |
| 3 | RabbitMQ + Frontend start | Queue publish/subscribe, list page, create form |
| 4 | Frontend complete + Docker/K8s | Detail page, polling, Docker Compose, K8s YAML |

---

Good luck! Focus on delivering a solid, working solution.
