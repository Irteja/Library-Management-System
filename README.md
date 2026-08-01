# Library Management System

A comprehensive, full-stack Library Management System built with **ASP.NET Core (Backend)** and **React (Frontend)**. This project provides role-based access control, book catalog management, lending and reservation capabilities, and member administration.

## 🏗️ Architecture & Technology Stack

This project was built with a strong focus on maintainability, scalability, and clean code principles. Here is an overview of the technologies used and *why* they were chosen.

### Backend Stack
* **Framework:** ASP.NET Core Web API
* **Architecture:** Clean Architecture
* **Pattern:** CQRS (Command Query Responsibility Segregation) with MediatR
* **ORM:** Entity Framework Core
* **Authentication:** JWT (JSON Web Tokens) with BCrypt hashing
* **Validation:** FluentValidation

#### Why these backend choices?
1. **Clean Architecture:** By separating the codebase into Domain, Application, Infrastructure, and API layers, we ensure that business logic is completely isolated from external concerns (like the database or UI). This makes the application highly testable and resilient to future changes (e.g., swapping the database provider or UI framework without rewriting business rules).
2. **CQRS & MediatR:** Separating read operations (Queries) from write operations (Commands) simplifies complex business logic. `MediatR` provides an elegant in-process messaging system, keeping controllers incredibly thin and mapping exact use cases to specific handlers.
3. **JWT Auth & BCrypt:** JWTs allow for stateless authentication, which is perfect for modern Single Page Applications (SPAs) like our React frontend. BCrypt is used for secure, salted password hashing to protect user credentials.
4. **FluentValidation:** Validating requests in the Application layer before they hit the business logic prevents bad data from entering the system. FluentValidation separates rules from the models themselves, keeping entities clean.

### Frontend Stack
* **Framework:** React (bootstrapped with Vite)
* **Routing:** React Router
* **State Management:** React Context API
* **Styling:** Vanilla CSS (Custom styling)
* **HTTP Client:** Axios

#### Why these frontend choices?
1. **React & Vite:** React provides a highly modular, component-based approach to building UIs. Vite was chosen over older tools (like Create React App) for its blazing-fast cold server start and hot module replacement (HMR), significantly speeding up development.
2. **Context API (instead of Redux):** For an application of this scope, Redux introduces unnecessary boilerplate. The built-in Context API is perfect for managing global state like user sessions (`AuthContext`) and theme preferences efficiently.
3. **Vanilla CSS:** To maintain complete control over the design and avoid the overhead or specific paradigms of large CSS frameworks (like Bootstrap or Tailwind). This ensures a bespoke, highly customized look and feel that perfectly matches the product requirements.
4. **Axios:** Provides automatic JSON transformation, global error handling (via interceptors), and request cancellation, making API communication more robust than the native `fetch` API.

---

## ✨ Key Features

* **Role-Based Access Control (RBAC):** Distinct workflows for `Admin`, `Librarian`, and `Member` users.
* **Member Self-Registration:** Public registration flow that automatically creates system user accounts and domain member profiles securely.
* **Book & Catalog Management:** Complete CRUD operations for books, managing stock levels across different library branches.
* **Borrowing & Returning:** Track active loans, calculate due dates, and manage returns.
* **Reservation System:** Allow members to reserve books that are currently checked out.
* **Member Management:** Admins and Librarians can manage member accounts, view borrowing history, and register new staff members.

---

## 🚀 Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (or later)
* [Node.js](https://nodejs.org/) (v18 or later)
* SQL Server / SQLite (Depending on active EF Core provider configuration)

### Backend Setup
1. Navigate to the backend directory:
   ```bash
   cd Back-End/src/API
   ```
2. Update the `appsettings.json` or `appsettings.Development.json` with your database connection string if necessary.
3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the API:
   ```bash
   dotnet run
   ```
   *The API will typically run on `https://localhost:7082` or similar.*

### Frontend Setup
1. Navigate to the frontend directory:
   ```bash
   cd Front-End
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Create a `.env` file (if required) and point `VITE_API_URL` to your backend instance.
4. Start the development server:
   ```bash
   npm run dev
   ```
   *The React app will typically run on `http://localhost:5173`.*

---

## 📁 Project Structure

```
Library Management System/
│
├── Back-End/
│   ├── src/
│   │   ├── API/             # Controllers, Middleware, Startup configuration
│   │   ├── Application/     # Use cases (CQRS), Interfaces, DTOs, Validators
│   │   ├── Domain/          # Entities, Enums, Exceptions (Core Business Logic)
│   │   └── Infrastructure/  # EF Core DbContext, Repositories, External Services
│   └── tests/               # Unit and Integration tests
│
└── Front-End/
    ├── src/
    │   ├── components/      # Reusable UI components (Layout, Protected Routes)
    │   ├── context/         # Global state (AuthContext)
    │   ├── pages/           # Route views (Login, Dashboard, Register, Books, etc.)
    │   ├── services/        # Axios API clients
    │   └── index.css        # Global styles
    └── package.json
```
