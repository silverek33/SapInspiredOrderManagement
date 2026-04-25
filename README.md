# SAP-Inspired Order Management System

This project is a simplified order management system inspired by SAP ERP workflows.  
It demonstrates how business processes such as order creation, status tracking and role-based operations can be implemented in a structured application.

The goal of the project is to connect software development with real business logic and process thinking.

---

## Business context

In typical ERP systems, order management involves:

- creating and managing orders
- tracking order status
- handling role-based actions (e.g. admin vs user)
- maintaining data consistency across related entities

This project models a simplified version of that workflow.

---

## Features

- Order creation and management
- Order status tracking (e.g. Created → In Progress → Completed)
- Role-based access (admin vs user)
- Structured data model for business entities
- CRUD operations with validation
- Separation of business logic from controllers

---

## Tech stack

- ASP.NET Core MVC (.NET 8)
- C#
- Entity Framework Core
- SQL Server
- Razor Views

---

## Domain model

Main entities:

- Order – represents a business transaction
- User – system user with role
- Status – represents order lifecycle stage

Relationships:

- one user → many orders  
- one order → one status  

This reflects real ERP-style data modeling.

---

## Architecture

The project separates responsibilities into:

- Controllers – request handling
- Services – business logic
- Data layer – persistence with EF Core
- Views – UI layer

This structure makes the application easier to extend and maintain.

---

## Technical decisions

- MVC architecture to clearly separate UI and backend logic
- Entity Framework Core for relational data management
- Explicit modeling of order status workflow
- Role-based access to simulate real business environments

---

## Limitations

This is a simplified ERP-style system and does not include:

- advanced reporting
- integration with external systems
- complex authorization rules
- workflow automation

---

## Possible improvements

- add workflow engine for order status transitions
- add reporting dashboard
- implement API layer
- add audit logging
- integrate with external systems

---
