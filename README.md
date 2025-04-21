# SmartCowork

SmartCowork is a comprehensive coworking space management platform built on a microservices architecture. It facilitates the booking of workspaces, meeting rooms, and shared resources while leveraging AI for optimized space utilization and personalized recommendations.

## Architecture

This project implements a microservices architecture with the following components:

- **API Gateway**: Routes client requests to appropriate services
- **User Service**: Handles user authentication, registration, and profile management
- **Space Service**: Manages coworking spaces, their availability, and features
- **Booking Service**: Handles workspace reservations and scheduling
- **Billing Service**: Manages invoicing and payment processing
- **AI Service**: Provides recommendations and predictive analytics

## Technologies

- **.NET 8**: Core framework for all microservices
- **Entity Framework Core**: ORM for data access
- **SQL Server**: Database for persistent storage
- **RabbitMQ**: Message broker for inter-service communication
- **JWT**: For secure authentication
- **Ocelot**: API Gateway implementation
- **Swagger/OpenAPI**: API documentation
- **Angular**: Frontend framework

## Features

- **User Management**: Registration, authentication, and profile management
- **Space Management**: Creation, updating, and searching for available spaces
- **Booking System**: Real-time availability checking and reservation management
- **Automated Billing**: Invoice generation and payment processing
- **AI Recommendations**: Space suggestions based on user preferences and usage patterns
- **Analytics Dashboard**: Usage statistics and occupancy reports

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server
- RabbitMQ
- Docker (optional for containerization)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/your-username/SmartCowork.git
cd SmartCowork