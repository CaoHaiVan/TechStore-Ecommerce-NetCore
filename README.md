🚀 TechStore – Modern Ecommerce Platform (ASP.NET Core MVC)

TechStore is a modern, high-performance eCommerce web application built on ASP.NET Core MVC, providing a seamless shopping experience with secure online payments, real-time order processing, and a powerful admin management system.

This project was developed as a graduation thesis, focusing on building a fully functional, production-ready eCommerce solution with clean architecture, optimized data flow, and real business workflows.

⭐ Key Highlights

Full-featured ecommerce platform with real-world business logic

Integrated online payment gateways (MoMo & VNPAY)

High-performance backend with ASP.NET Core MVC

Centralized admin dashboard to manage products, categories, orders & customers

Scalable database architecture using SQL Server + Entity Framework Core

Friendly, responsive UI built with Bootstrap

Clean architecture for easy maintenance and extensibility

🛒 Core Features
Customer Features

Browse products by category, brand, or search

View product details with images & pricing

Add/remove/update items in shopping cart

Apply discount codes (percentage-based coupons)

Checkout with:

Cash on Delivery (COD)

MoMo online payment

VNPAY online payment

Track order history and status

Responsive experience across devices

Admin Features

Secure admin login & session management

Product Management (CRUD, hide/unhide, image upload)

Category & Brand Management

Order Management (view, update status, cancel, delete)

Slider Management (homepage banners)

Shipping Fee Management by location

Coupon Management (discount %, expiry, usage limit)

User Management

Dashboard with system overview (products, orders, revenue trends…)

🧱 Architecture Overview

TechStore follows a clean and maintainable architecture:

ASP.NET Core MVC for frontend + backend

Entity Framework Core for ORM

SQL Server as the relational database

Layered structure:

Controllers

Services

Repositories

Views (Razor)

Models (Domain & ViewModels)

Authentication using Cookies / Custom Auth

Clean routing & modular structure

🗄️ Database Design

Main tables include:

Users

Products

Categories

Brands

Orders

OrderDetails

Coupons

ShippingFees

Sliders

Designed with referential integrity, foreign keys, and optimized indexing to ensure high performance under real workloads.

🛠️ Tech Stack
Technology	Purpose
ASP.NET Core MVC	Main backend + frontend framework
Entity Framework Core	ORM and database communication
SQL Server	Primary relational database
Bootstrap 5	UI/UX frontend framework
jQuery + AJAX	Dynamic interactions & async operations
MoMo Payment	Online payment gateway
VNPAY Payment	Secure VN payment gateway
LINQ	Data querying
Identity / Cookies	Authentication
⚙️ Installation & Setup
Requirements

Visual Studio 2022

.NET 7 / .NET 8 SDK

SQL Server + SSMS

Git

Setup Steps

Clone the repository:

git clone https://github.com/CaoHaiVan/TechStore-Ecommerce-NetCore.git


Open the solution file:

TechStore-Ecommerce-NetCore.sln


Update your database connection in:

appsettings.Development.json


Run migrations (if applicable)

Press F5 to run on IIS Express or Kestrel

💳 Payment Gateways
MoMo

Integrated via sandbox API

Secure encrypted payment URL

Callback handled through:

/Checkout/PaymentCallback

VNPAY

Uses signed & encoded payment requests
Callback:
/Checkout/PaymentCallbackVnpay
Both gateways emulate real-world ecommerce transaction flows.
🖥️ User Interface Preview

(You can add images here later, e.g. Home page, Product list, Admin Dashboard)

🚀 Future Enhancements

AI-powered product recommendations

Customer reviews & ratings

SEO optimization for product ranking

Multi-language support

Real-time order notifications using SignalR

Mobile application integration

Microservices architecture (Product API / Order API / Auth API)

👨‍💻 Author

 Cao Hai Van
Graduation Thesis — 2025
Tech major: Software Engineering / Information Technology
