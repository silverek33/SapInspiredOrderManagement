# SAP-Inspired Order Management System

A SAP-inspired business application designed to simulate an order lifecycle process from order creation to closure. The project demonstrates workflow logic, status management, business rules, user roles, and database-backed transactional operations in an ERP-like environment.

## Business Context

The application follows a simplified order-to-cash flow:

Customer -> Sales Order -> Order Items -> Approval -> Fulfillment -> Delivery -> Invoice -> Closure

It is built as a portfolio project for roles such as Junior SAP Consultant, SAP Support, Key User, Business Analyst, Implementation Support, Process Analyst, and IT/Data roles with a business process focus.

## Features

- Cookie-based login with three business roles: Admin, Sales Operator, Manager
- Customer master data management
- Product master data management with active/inactive control and stock visibility
- Sales order creation with order items
- Automatic line total and order total calculation
- ERP-like document lifecycle statuses
- Role-aware status transition rules
- Manager approval queue
- Status history tracking for every order
- Filtering by status, customer, priority and order date
- Operational dashboard with counts, open order value and high-priority orders

## Tech Stack

- C#
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server LocalDB
- Razor Views
- Bootstrap

## Demo Users

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@sapdemo.local | Admin123! |
| Sales Operator | sales@sapdemo.local | Sales123! |
| Manager | manager@sapdemo.local | Manager123! |

## Database Structure

Core entities:

- Users
- Customers
- Products
- SalesOrders
- SalesOrderItems
- StatusHistories

Key relationships:

- Customer 1..N SalesOrders
- SalesOrder 1..N SalesOrderItems
- Product 1..N SalesOrderItems
- User 1..N SalesOrders
- User 1..N StatusHistories

## Workflow

Supported status lifecycle:

Draft -> Submitted -> Approved -> In Fulfillment -> Delivered -> Invoiced -> Closed

Cancellation is allowed before invoice/closure according to the configured role rules. Closed and Cancelled orders block further status changes.

Role rules:

- Sales Operator can create Draft orders, edit Draft orders and submit them.
- Manager can approve Submitted orders and move approved orders through fulfillment, delivery, invoice and closure.
- Admin can manage master data and perform all workflow transitions.

## Business Rules

- Orders can only be created for active customers.
- New order items can only use active products.
- Orders must contain at least one item before submission.
- Quantity must be greater than zero.
- Unit prices are copied from product master data.
- Total amount is calculated from order items.
- Invalid status jumps are blocked.
- Every status change writes a StatusHistory record.
- Draft orders can be edited; approved, closed and cancelled documents are protected from normal editing.

## Run Locally

```powershell
dotnet restore
dotnet run --urls http://localhost:5127
```

The app uses SQL Server LocalDB by default:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SapInspiredOrderManagement;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

The database schema is applied through EF Core migrations, and demo data is created automatically on first run.
