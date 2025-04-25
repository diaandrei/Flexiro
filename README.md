# Flexiro Backend
A robust and scalable e-commerce backend solution developed using ASP.NET, designed to support small businesses with a cost-effective platform for managing products, orders, payments, and user authentication.

## Architecture Overview
The backend follows a layered architecture pattern to ensure maintainability, scalability, and testability:

### Layers
- **API Layer**: Exposes RESTful endpoints consumed by the front-end client
- **Application Layer**: Manages complex operations and transforms domain objects into DTOs
- **Service Layer**: Implements core business logic and operations
- **Contracts Layer**: Defines interfaces and DTOs used across the system
- **Identity Layer**: Handles authentication and authorization via JWT tokens

## Features
- **Secure User Authentication**: JWT-based authentication with role-based access control
- **Product Management**: CRUD operations for products with image support
- **Order Processing**: Comprehensive workflow from cart checkout to delivery
- **Real-time Notifications**: SignalR integration for instant updates
- **Payment Processing**: Integration with trusted payment gateways
- **Logging & Audit Trail**: Comprehensive logging for troubleshooting and audit purposes

## Technologies Used
- **Framework**: ASP.NET Core
- **ORM**: Entity Framework Core
- **Database**: Azure SQL Database
- **Storage**: Azure Blob Storage for media files
- **Authentication**: JWT Token-based security
- **Real-time Communication**: SignalR
- **Testing**: Unit testing with mocking frameworks
- **CI/CD**: Automated build, test, and deployment pipeline
- **Cloud Hosting**: Azure App Services

## Live Demo

### Deployed Application
- **Frontend Application**: [Live platform](https://flexiroweb-h3g0fvfkdbhcdvgk.uksouth-01.azurewebsites.net)
- **API Endpoints**: [API Endpoints](https://flexiroapi-d7akfuaug8d7esdg.uksouth-01.azurewebsites.net/swagger/index.html)

### Using the Platform
1. Visit the frontend application link
2. Browse products and navigate the platform without an account
3. To complete the checkout process, you'll need to register for an account
4. For seller registration, please note that admin approval is required before your shop will be displayed on the main screen

### Testing Payments
For testing payment functionality, you can use any of the following test cards:

- **Visa**: 4111 1111 1111 1111
- **Mastercard**: 5555 5555 5555 4444
- **Maestro**: 5018 0000 0009

Use any future expiration date successful test payments.

For more test card options, please refer to the [Braintree Testing Documentation](https://developer.paypal.com/braintree/docs/guides/credit-cards/testing-go-live/php)

## Getting Started
### Prerequisites
- Visual Studio 2022
- .NET 8 SDK or later
- SQL Server (local or Azure)
- Azure account (for cloud deployment)

### Installation
1. Clone the repository:
   ```bash
   git clone
   cd Flexiro
   ```

2. Update the connection strings in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=yourserver;Database=ECommerceDB;Trusted_Connection=True;MultipleActiveResultSets=true",
       "BlobStorage": "Your Azure Blob Storage Connection String"
     }
   }
   ```

3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

## Security
- JWT tokens for authentication with claims-based authorization
- Token expiration and refresh mechanisms
- HTTPS protocol enforcement
- Input validation and sanitization
- Cross-Origin Resource Sharing (CORS) configuration

## Testing
The project includes comprehensive unit tests covering business logic across all layers:
```bash
dotnet test
```

## Deployment
### Azure Deployment
1. Create required Azure resources:
   - Azure App Service
   - Azure SQL Database
   - Azure Blob Storage

2. Configure CI/CD pipeline in Azure DevOps or GitHub Actions

3. Update connection strings and configuration for production

4. Deploy using the pipeline or manual deployment:
   ```bash
   dotnet publish -c Release
   ```

## API Documentation
Our API is fully documented using Swagger. When running the application locally, you can access the documentation at:
```
https://localhost:5001/swagger
```

For the deployed version, visit:
```
https://flexiroapi-d7akfuaug8d7esdg.uksouth-01.azurewebsites.net/swagger/index.html
```

## Roadmap
- **Microservices Migration**: Plan to break down the monolithic application into microservices
- **Advanced Analytics**: Integration with BI tools for business insights
- **Enhanced Security**: Multi-factor authentication and additional security features
- **Mobile App Support**: API enhancements to support native mobile applications
- **Advanced Real-time Features**: Expand real-time communication capabilities

## Contributing
1. Fork the repository
2. Create a feature branch: `git checkout -b feature-name`
3. Commit your changes: `git commit -m 'Add feature'`
4. Push to the branch: `git push origin feature-name`
5. Submit a pull request
