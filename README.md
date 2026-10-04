# Awesome Pizza

A small portal to manage the orders of a pizzeria. The customer can order without registration and receives a code to follow the order. The pizza maker sees the queue, takes one order at a time and sets it as ready.

The project follows the two iterations of the assignment: first the .NET API with the tests, then an Angular frontend that uses them. Both are in this repository.

## Tech stack

Backend:

- .NET 10, ASP.NET Core Web API with controllers
- Entity Framework Core 10 with SQL Server 2022 (in Docker)
- SignalR for the live updates
- OpenAPI document and Swagger UI
- xUnit v3 on Microsoft.Testing.Platform, `Microsoft.AspNetCore.Mvc.Testing` for the integration tests

Frontend:

- Angular 22.2 (standalone components, signals)
- Angular Material 22.2
- `@microsoft/signalr` 10
- TypeScript 6, Prettier

## Prerequisites

- .NET 10 SDK
- Docker
- Node.js 24.15 or later (22.22.3+ also works), required by Angular 22

## Getting started

The database runs in Docker. From the root of the repo:

```bash
docker compose up -d
```

SQL Server 2022 runs on port 14330 (on my pc the 1433 was already used) and it must be running for the API and also for the tests. In Development the API applies the migrations and the seed of the menu at startup, so there is nothing else to do: you just start the API and the frontend.

The `sa` password (`AwesomePizza!Dev123`) is in `docker-compose.yml` and in `appsettings.Development.json`. I know that this is not a good practice, but it's only the password of a local container for development, and in this way the project starts without creating secrets or env files. In a real environment I would put it in a secret store.

## Useful URLs

- Frontend: http://localhost:4200
- Kitchen page: http://localhost:4200/kitchen (there is no link to it in the toolbar)
- Swagger UI: http://localhost:5046/swagger (Development only)

## Tests

Also the integration tests need the database container running. They use the same SQL Server but with a different database, `AwesomePizza_Tests`, that is created at the start and deleted at the end, so the development database is never touched.

## Project structure

I used a monorepo: backend and frontend are in the same repository.

```
backend/
  src/AwesomePizza.Api/       the API
  tests/AwesomePizza.Tests/   unit and integration tests
frontend/                     Angular app
docker-compose.yml            SQL Server for local development
```

The API is one project divided in folders (models, data, DTOs, services, controllers, SignalR hub, background job). In the frontend there is one folder for each page under `src/app/pages`, and the HTTP and SignalR services are in `src/app/services`.

## API

Customer:

- `GET /api/Pizza` the menu
- `POST /api/Order` creates an order, returns 201 with the order and its code
- `GET /api/Order/{code}` the order with its status (the code is case-insensitive)

Kitchen:

- `GET /api/Kitchen/queue` the orders that are not ready yet, oldest first
- `POST /api/Kitchen/queue/next` takes the next order
- `POST /api/Kitchen/orders/{code}/complete` marks the order as ready

SignalR hub (`/hubs/notification`):

- `SubscribeToOrder(code)` joins the group of one order and receives `OrderStatusChanged`
- `JoinKitchen()` joins the kitchen group and receives `QueueChanged` every time an order is created, taken or completed

## How it works

The customer opens the home page, chooses the pizzas with + and −, sees the total and can insert a name (optional). After the confirm they're redirected to the tracking page (`/orders/{code}`) with code, status, a progress bar and the pizzas. The page updates by itself and a snackbar tells when the order is ready. If the page is closed, the code can be inserted in the "Segui il tuo ordine" box in the home page.

The pizza maker uses `/kitchen`: on top there is the order in progress, under it the queue. The "Prendi il prossimo" button takes the oldest order and "Ordine pronto" completes it. Both show a confirmation snackbar, and also the API errors are shown in a snackbar.

## Design choices

### A monolith

It's one pizzeria that only has to manage orders. With microservices I would have more services to deploy, communication between them and data on different databases, for a domain of three entities. I didn't see a real advantage, so I chose a monolith on purpose: for this problem it's the right choice, and it's simple to run and to read. The solution has only two projects, the API and the tests.

### One pizza maker, FIFO queue

I assumed there is only one pizza maker, like the assignment describes. More pizza makers would need authentication to work well, and that isn't part of the assignment. So there can be maximum one order in progress in all the system, and the pizza maker doesn't choose the order: the server always gives the oldest pending one.

This is checked in `KitchenService`, that returns 409 if there is already an order in progress, and in the database with a unique filtered index on `Status`, that blocks it also when two requests arrive at the same time.

### The Order model

`Order` has private setters and the status can change only with `Start()` and `Complete()`, so an order can't skip a step or go back. Every line saves the price of the pizza at the moment of the order, so if the menu changes the old orders are not touched.

### No login, and the order code

The assignment doesn't ask for registration or authentication, so there is no login: the pizza maker opens the kitchen page from the URL and the kitchen endpoints are open. In a real project I would add authentication with roles or claims for that part.

The customer receives a random code of 6 characters, different from the internal id and case-insensitive. It works like a credential for the tracking: who has the code can follow the order.

### Live updates with SignalR

I chose SignalR because I know it well and for live notifications from backend to frontend it works very well. I use it to tell to the pages that something changed: to the kitchen when an order is created, taken or completed, and to the customer when the status of the order changes. For example, with `/kitchen` open in two tabs, if you take or complete an order in one, the other one updates without reload. This avoid incostintent states.

### Error handling

A `GlobalExceptionHandler` catches the exceptions of the services and transforms them in a `ProblemDetails` with the correct status code (404, 400 or 409).

### Menu seed

The menu changes rarely and the menu management is not in the assignment, so I load it with a seed instead of making a CRUD. The pizzas have fixed ids, and at startup (in Development) the seeder adds the missing ones and updates name and price of the others.

### Order cleanup job

A `BackgroundService` runs at startup and then every 24 hours, and deletes the `Ready` orders created more than 7 days ago (both values are in `appsettings.json`).

To be honest for a single pizzeria it's not really necessary: also with 100 orders per day it would take years before the table becomes heavy. But in a real project there is always a retention policy, and it's how I'm used to work, so I wanted show that I know it.

### Tests

The assignment asks for unit tests, and mine cover the domain without database: order transitions and total, the price snapshot and the format of the order code.

They don't cover services and controllers, that use the `DbContext` directly and have most of their logic in the queries and in the database constraints. For this reason, also if it wasn't required, I added integration tests with `WebApplicationFactory` on a real SQL Server database. They test order creation and tracking, the FIFO queue, the 409 errors, concurrent requests and the cleanup job.

### Angular and Angular Material

The assignment allowed React or Angular. Angular with Angular Material is what I use every day at work. In few days I couldn't learn React good enough to write it like I want, and I wanted to develop this project by myself, without using AI tools to cover what I don't know. So I chose the stack where I feel confident.

The pages use standalone components and signals for the state (quantities, the total with a `computed`, the kitchen queue).

I kept it minimal the UI design, I preferred to spend the time on the application part (state, live updates, error handling) and not on the graphic.

## What I'd do in a real project

- Authentication for the kitchen, with roles or claims on the kitchen endpoints and on the hub.
- More pizza makers: the rule of "one order in progress" must change and probably I would add a concurrency token.
- A small admin area to manage the menu, instead of the seed.
- An orders history for the pizzeria. In this case the cleanup would archive the orders instead of deleting them, based on the completion date.
- A configuration for each environment: secrets out of the repository, migrations and seed in the deploy pipeline, and a retry for the (very rare) case of a duplicated order code, that today gives a 500.
