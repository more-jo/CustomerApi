# CustomerApi

ASP.NET Core Minimal API for managing customers and orders.

This project was created as a practice project to improve my understanding of clean code, TDD and API design. It uses Entity Framework Core with an in-memory database. The project also contains unit and integration tests using NUnit. All tests are currently passing.

## Prerequisites

- .NET SDK installed
- You can check the installed version with `dotnet --version`

## Build and Test

```bash
dotnet build
dotnet test
```

## Endpoints

| Endpoint | Behaviour |
|---|---|
| `GET /customers` | Returns all customers, including soft-deleted ones |
| `GET /customers/{id}` | Returns the customer with its `IsDeleted` status; returns 404 if the customer never existed |
| `POST /customers` | Creates a customer and returns 201 with a location header; returns 422 if the name is missing or empty |
| `PUT /customers/{id}` | Updates a customer; returns 204 on success and 404 if the customer does not exist; returns 422 if the name is missing or empty |
| `DELETE /customers/{id}` | Soft-deletes a customer by setting `IsDeleted = true`; returns 404 if the customer does not exist |
| `GET /orders?customerId={id}` | Returns the orders for a customer, including soft-deleted orders |
| `GET /orders/{id}` | Returns an order or 404 if it does not exist. Returns 200 also for soft-deleted. |
| `POST /orders` | Creates an order and returns 201 with a location header; returns 404 if the referenced customer does not exist |
| `DELETE /orders/{id}` | Soft-deletes an order by setting `IsDeleted = true`; returns 404 if the order does not exist |

## Design decisions

| Decision | Rationale |
|---|---|
| Flat route `/orders?customerId=` instead of `/customers/{id}/orders` | I chose this because it keeps the order endpoints independent from the customer endpoints. It also makes it easier to add other ways of querying orders later. |
| Separate endpoints for `/orders/{id}` and `/orders?customerId=` | Query parameters are not part of ASP.NET Core routing. Using the same route for both cases would therefore cause a conflict. |
| Soft delete instead of permanently deleting customers | A deleted customer is considered inactive rather than completely gone. This means the customer can potentially be reactivated later. |
| `GET /customers/{id}` still returns deleted customers | The API should be able to distinguish between a customer that was deleted and a customer that never existed. |
| `IsDeleted` is part of the API response | Clients may need to know whether a customer or order is inactive. Hiding this information would make the soft-delete behaviour difficult to use from the outside. |
| Deleting a customer does not delete its orders | The order itself is still valid and should remain available even if the customer is inactive. This behaviour is also covered by a test. |
| Orders are soft-deleted as well | A cancelled order should still be traceable. There is a difference between an order that was created and later cancelled and an order that never existed. |
| Missing `CustomerId` on `POST /orders` returns 404 | The request itself is valid, but the referenced customer does not exist. Therefore, 404 is more appropriate than 422. |
| Cross-entity validation is done in the handler | The repository is responsible for data access. Checking whether a customer exists before creating an order is part of the application logic. |
| `IOrderRepository` has its own interface | Keeping the interfaces separate avoids creating one large repository interface as the application grows. |
| Missing `CustomerId` on `PATCH /customers` returns 404 | The request itself is valid, but the referenced customer does not exist. |
| `PATCH /customers` changes leave other parts untouched | Customer might be restored in the future. |

## Other decisions

| Decision | Rationale |
|---|---|
| Removed `Microsoft.EntityFrameworkCore.Sqlite` instead of updating it | `dotnet list package --vulnerable --include-transitive` showed two high-severity vulnerabilities in transitive dependencies. Since SQLite was not actually needed for this project, removing the dependency was the simpler solution; Removed package cannot be vulnerable. |
| Generic `ProblemDetails.Detail` for 5xx errors with a `traceId` in `Extensions` | Returning `ex.Message` could expose internal information such as connection strings, file paths or table names. Instead, the exception is logged together with the request's trace ID. The client only receives the trace ID, which still makes it possible to find the corresponding error in the logs. For 4xx errors, a more specific message is returned because these errors are caused by the client's request. |
| `LogError` is used for 5xx errors and `LogWarning` for 4xx errors | Client errors should not fill the logs with error-level entries. Otherwise, a large number of invalid requests could make actual server-side problems harder to spot. |
| `/throw` is only registered in development | The endpoint is only there to trigger the exception middleware during testing. There is no reason to expose an endpoint whose only purpose is to throw an exception in production. |

## In Progress

- **Response DTOs** — The handlers currently return the entities directly. This means that adding a new public property to an entity could unintentionally change the API response.

- **No authentication** — Authentication is intentionally out of scope for this iteration.

## AI Assistance / Transparency

AI was used during the development of this project.
The main design and implementation decisions were made by me. I created the test cases and implented the source code. AI suggestions were reviewed, considered and adapted where appropriate rather than being used without review.
