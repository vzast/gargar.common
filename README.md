# Gargar.Common Library

## Overview

Gargar.Common is a comprehensive .NET library that provides reusable components for building robust and scalable applications. This library includes implementations for common infrastructure concerns such as email services, database access, file storage, and payment processing.

## Features

- **Repository Pattern**: Generic repository implementation for data access operations
- **Email Service**: SMTP-based email sending with support for attachments
- **Identity Management**: Configurable identity options for user authentication and authorization
- **Payment Processing**: Stripe integration for payment processing
- **File Storage**: S3-compatible object storage integration

## Project Structure

```
Gargar.Common/
├── src/
│   ├── Gargar.Common.API/
│   ├── Gargar.Common.Application/
│   │   └── Interfaces/
│   ├── Gargar.Common.Domain/
│   │   ├── Helpers/
│   │   └── Repository/
│   ├── Gargar.Common.Infrastructure/
│   │   ├── Email/
│   │   └── Storage/
│   └── Gargar.Common.Persistance/
│       ├── Database/
│       └── Repository/
└── tests/
    └── Gargar.Common.Tests/
```

## Getting Started

### Prerequisites

- .NET 7.0 or later
- SQL Server instance
- SMTP server for email functionality
- S3-compatible storage (like MinIO) for file storage
- Stripe account for payment processing

### Configuration

The application requires an `appsecrets.json` file with the following structure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=yourserver;Database=YourDatabase;User Id=username;Password=password;TrustServerCertificate=True;"
  },

  "IdentityOptions": {
    "Password": {
      "RequireDigit": true,
      "RequiredLength": 8,
      "RequireNonAlphanumeric": false,
      "RequireUppercase": true,
      "RequireLowercase": true
    },
    "SignIn": {
      "RequireConfirmedEmail": false,
      "RequireConfirmedPhoneNumber": false,
      "RequireConfirmedAccount": false
    },
    "Tokens": {
      "EmailConfirmationTokenProvider": "email",
      "AuthenticatorTokenProvider": "authenticator"
    },
    "User": {
      "RequireUniqueEmail": true,
      "AllowedUserNameCharacters": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"
    },
    "Lockout": {
      "MaxFailedAccessAttempts": 5
    }
  },

  "StripeOptions": {
    "SecretKey": "your_stripe_secret_key",
    "PublishableKey": "your_stripe_publishable_key",
    "FrontendUrl": "https://your-frontend-url.com",
    "WebhookKey": "your_webhook_key"
  },

  "EmailOptions": {
    "Host": "your.smtp.server",
    "Port": 587,
    "Username": "your_email@example.com",
    "Password": "your_email_password",
    "EnableSsl": true,
    "From": "your_email@example.com"
  },

  "S3Options": {
    "AccessKey": "your_access_key",
    "SecretKey": "your_secret_key",
    "BucketName": "your_bucket_name",
    "ServiceURL": "your_service_url"
  }
}
```

> **IMPORTANT:** Never commit the actual `appsecrets.json` file to version control. Add it to your `.gitignore` file to prevent accidental exposure of sensitive information.

## Usage Examples

### Using the Email Service

```csharp
// Configure the email service
var emailOptions = new EmailOptions
{
    Host = "your.smtp.server",
    Port = 587,
    Username = "your_email@example.com",
    Password = "your_password",
    EnableSsl = true,
    From = "your_email@example.com"
};

var emailService = new EmailService(emailOptions);

// Send a simple email
await emailService.SendEmailAsync(
    "recipient@example.com",
    "Test Subject",
    "<p>This is a test email</p>",
    true);

// Send an email with attachment
await emailService.SendEmailWithAttachmentAsync(
    "recipient@example.com",
    "Test Subject with Attachment",
    "<p>This email has an attachment</p>",
    "path/to/attachment.pdf",
    true);
```

### Using the Repository Pattern

```csharp
// Assuming you have a DbContext and an entity defined
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Create a repository instance
var repository = new BaseRepository<Product, int>(dbContext);

// Query for products
var expensiveProducts = await repository.GetAllAsync(p => p.Price > 100);

// Get a paged list
var pagedProducts = await repository.GetPagedAsync(
    predicate: p => p.Price > 0,
    orderBy: q => q.OrderByDescending(p => p.Price),
    pageNumber: 1,
    pageSize: 10);

// Add a new product
var newProduct = new Product { Name = "New Product", Price = 49.99m };
await repository.AddAsync(newProduct);
await repository.SaveAsync();
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
