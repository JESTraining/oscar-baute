# Order Processing System

Simple overview of use/purpose.

## Description

A small order management system: order intake, status tracking, and async
inventory validation. Backend is .NET 8 (API + RabbitMQ consumer), frontend
is React/Vite, storage is SQL Server, messaging is RabbitMQ. Everything runs
through Docker Compose.

## Tech Stack

* **Frontend:** React 18, Vite, TypeScript, MUI, React Router, Axios
* **Backend:** .NET 8, ASP.NET Core Web API, Entity Framework Core, JWT auth
* **Database:** SQL Server 2022
* **Messaging:** RabbitMQ
* **Containerization:** Docker, Docker Compose

## Getting Started

### Dependencies

* Docker Desktop, with Compose v2 (ships with recent versions - check with
  docker compose version)
* Nothing else. .NET and Node are not required locally, the API and frontend
  are both built inside their own containers.

### Installing

* Clone this repository
* No config changes needed to get it running - docker-compose.yml and
  docker-compose.override.yml are already wired together. docker compose
  picks up both automatically as long as you run it from the repo root.
* Optional: if you want the frontend built against a different API URL than
  the default (http://localhost:8080/api), set VITE_API_BASE_URL in a
  .env file at the repo root before building.

### Executing program

* From the repo root:

```
docker compose up --build
```

  --build is only needed the first time, or after changing API/frontend
  code - later on, docker compose up is enough.

* This starts 4 containers:
  * sqlserver - SQL Server 2022
  * rabbitmq - RabbitMQ + management UI
  * ops.api - the backend API
  * frontend - the React app, served by nginx, on port 5174

* The API applies EF Core migrations itself on startup (Program.cs calls
  dbContext.Database.Migrate()), so there's no separate migration step.
  That same migration seeds 2 users, so you can log in immediately:

  | Email                 | Password  | Role         |
  |------------------------|-----------|--------------|
  | admin@test.com          | Test@123  | Admin        |
  | regularuser@test.com    | Test@123  | RegularUser  |

  (see Backend/OPS.Infrastructure/Data/AppDbContext.cs,
  OnModelCreating -> `builder.HasData(...))

* Once it's up:
  * Frontend - http://localhost:5174
  * API (Swagger) - http://localhost:8080/swagger
  * RabbitMQ management - http://localhost:15672 (user: admin / pass: Test@123)
  * SQL Server - localhost,1433 (user: sa / pass: Test@123)

* To stop it:

```
docker compose down
```

  Data is kept (SQL data lives in a named volume, sqldata). To wipe the
  database and start clean instead:

```
docker compose down -v
```

## Help

* The very first time you run `docker compose up --build`, the API can fail
  to start. `ops.api` depends on `sqlserver` via `depends_on`, but that only
  waits for the SQL Server container to *start*, not for SQL Server itself to
  actually be ready to accept connections - and on startup the API tries to
  connect right away to run its migration and create the database. If SQL
  Server is still initializing at that moment, the API can't reach it and
  crashes. SQL Server can take a little while to boot on a first run, so
  this is fairly common the first time.

  If this happens, just run it again once SQL Server has had time to finish
  starting up:

```
docker compose up
```

* The frontend's VITE_API_BASE_URL is baked into the built JS at image
  build time (Vite env vars aren't read at runtime), so changing the API's
  port means rebuilding the frontend image, not just restarting it:

```
docker compose build frontend
docker compose up
```

## Authors

oscarbaute
[oscar.baute@unosquare.com](mailto:oscar.baute@unosquare.com)

## Version History

* Current
    * Added event publisher, Orders controller, inventory check consumer
    * Added React/Vite frontend, wired into docker-compose
* 0.1
    * Initial API setup: JWT authentication, database context,
      docker-compose, repository pattern
* 0.0
    * Initial commit

See commit history for details.

## License

No license file yet - internal project.

## Acknowledgments

* This README follows the structure of
  [DomPizzie's README-Template.md gist](https://gist.github.com/DomPizzie/7a5ff55ffa9081f2de27c315f5018afc)
