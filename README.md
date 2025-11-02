# 🚗 AutoEdge - Comprehensive Vehicle Dealership Management System

AutoEdge is a full-featured automotive dealership management platform built with ASP.NET Core MVC 9.0. The system provides end-to-end solutions for vehicle sales, service management, customer relationships, recruitment, and employee onboarding.

**Live Application:** [https://autoedgedealership.azurewebsites.net/](https://autoedgedealership.azurewebsites.net/)

---

## 📋 Table of Contents

- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Requirements](#-requirements)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Database Setup](#-database-setup)
- [Running the Application](#-running-the-application)
- [User Roles](#-user-roles)
- [Module Overview](#-module-overview)
- [Services & Integrations](#-services--integrations)
- [Testing](#-testing)
- [Project Structure](#-project-structure)
- [Key Features Highlights](#-key-features-highlights)
- [Contributing](#-contributing)
- [License](#-license)
- [Support](#-support)

---

## ✨ Features

### 🛒 Vehicle Sales & Inventory
- Browse and search vehicles with filters
- Vehicle details with images and specifications
- Contract generation and e-signatures
- Stripe payment integration
- PDF document generation (contracts, invoices)
- QR code generation for vehicles

### 🔧 Service Management
- Service booking system with scheduling
- Vehicle pickup and drop-off logistics
- Real-time service status updates
- Service checklists for mechanics
- Automated invoice generation
- Payment tracking and notifications
- Driver assignment for vehicle transportation

### 👥 Customer Portal
- Customer dashboard with service history
- Booking management
- Invoice and payment history
- Real-time status notifications
- Document downloads

### 🎯 Recruitment System
- Job posting management
- Application submission and tracking
- Resume parsing with OCR
- AI-powered assessments
- Interview scheduling
- Video meeting integration (Jitsi/Zoom)
- Automated email notifications

### 👔 Employee Onboarding
- Offer generation and acceptance
- Documentation collection
- Digital signatures
- Background check tracking
- Admin review workflow

### 📊 Admin Dashboard
- User and role management
- Vehicle inventory management
- Analytics and reports
- System configuration
- Purchase and delivery tracking

### 🤖 AI Assistant
- Real-time chat support
- Natural language processing
- Context-aware responses
- Integration with OpenRouter API

---

## 🛠 Technology Stack

### Backend
- **Framework:** ASP.NET Core 9.0 MVC
- **Database:** SQL Server (Azure SQL)
- **ORM:** Entity Framework Core 9.0.8
- **Authentication:** ASP.NET Core Identity
- **PDF Generation:** iText 7
- **OCR:** Tesseract 5.2.0
- **Background Jobs:** Hangfire 1.8.6
- **Email:** MailKit 4.3.0
- **QR Codes:** QRCoder 1.6.0

### Frontend
- **UI Framework:** Bootstrap 5
- **JavaScript:** Vanilla JS with modern ES6+
- **Icons:** Font Awesome 6
- **Charts:** Chart.js
- **Signature Pad:** signature_pad.js

### Payment & Integrations
- **Payment Gateway:** Stripe
- **E-Signatures:** Canvas-based digital signatures
- **Video Meetings:** Jitsi Meet / Zoom
- **AI Services:** OpenRouter API (Llama 3.2)
- **Maps:** Google Maps API

---

## 📦 Requirements

- **.NET SDK 9.0** or later
- **SQL Server** (SQL Server 2019+ or Azure SQL Database)
- **Visual Studio 2022** or **VS Code** or **JetBrains Rider**
- **SMTP Server** (Gmail SMTP configured)
- **Stripe Account** (for payment processing)
- **OpenRouter API Key** (for AI features)

---

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/AutoEdge.git
cd AutoEdge
```

### 2. Restore Packages

```bash
dotnet restore
```

### 3. Configure Database Connection

Update `appsettings.json` with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AutoEdgeDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

### 4. Run Migrations

```bash
dotnet ef database update
```

---

## ⚙️ Configuration

### Email Settings

Configure SMTP in `appsettings.json`:

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  }
}
```

### Stripe Configuration

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_..."
  }
}
```

### AI Assistant Settings

```json
{
  "AI": {
    "BaseUrl": "https://openrouter.ai/api/v1/chat/completions",
    "ApiKey": "sk-or-v1-...",
    "Model": "meta-llama/llama-3.2-3b-instruct:free",
    "Temperature": 0.6,
    "MaxTokens": 1000
  }
}
```

### Application URL

Update base URL for production:

```json
{
  "Assessment": {
    "BaseUrl": "https://your-domain.com",
    "AssessmentPath": "/RecruitmentApplicant/Assessment"
  },
  "BaseUrl": "https://your-domain.com"
}
```

---

## 🗄️ Database Setup

The application uses Entity Framework Core migrations. The database is automatically created and seeded on first run.

### Seed Data Includes:
- Default admin user
- Sample vehicles and images
- Document types
- Job postings
- Sample applications
- Assessment questions
- User roles and permissions

### Default Admin Credentials

**Email:** admin@autoedge.com  
**Password:** Admin@123

⚠️ **Important:** Change the default admin password after first login in production!

---

## 🏃 Running the Application

### Development

```bash
dotnet run
```

The application will be available at:
- HTTPS: `https://localhost:7213`
- HTTP: `http://localhost:5071`

### Production Deployment

1. Publish the application:

```bash
dotnet publish -c Release -o ./publish
```

2. Deploy to Azure App Service (configured via Publish Profiles)

---

## 👤 User Roles

| Role | Description | Access Level |
|------|-------------|--------------|
| **Administrator** | Full system access | All modules, user management, reports |
| **Customer** | Vehicle purchasers | Browse, purchase, service bookings, invoices |
| **SalesRepresentative** | Sales team | Vehicle sales, customer management |
| **SupportStaff** | Customer support | Service management, inquiries |
| **Mechanic** | Service technicians | Service board, checklists, work orders |
| **Driver** | Delivery drivers | Pickup/delivery assignments, QR scanning |
| **Recruiter** | HR recruitment | Job postings, applications, interviews |
| **Applicant** | Job applicants | Application submission, assessment |
| **Technician** | Service scheduling | Maintenance scheduling, vehicle check-ins |

---

## 📦 Module Overview

### Core Modules

1. **Vehicle Management** (`VehicleController`, `VehicleBrowseController`)
   - Inventory management
   - Search and filtering
   - Image uploads
   - Details and specifications

2. **Sales & Purchase** (`PurchaseController`, `ContractController`)
   - Purchase initiation
   - Contract generation
   - E-signature workflow
   - Payment processing

3. **Service Management** (`BookingsController`, `ServiceBookingController`)
   - Service bookings
   - Status tracking
   - Invoice generation
   - Driver assignment

4. **Customer Portal** (`CustomerController`, `CustomerServicePortalController`)
   - Service history
   - Payment tracking
   - Document access

5. **Recruitment** (`RecruitmentRecruiterController`, `RecruitmentApplicantController`)
   - Job management
   - Application tracking
   - AI assessments
   - Interview scheduling

6. **Employee Onboarding** (`EmployeeOnboardingController`, `EmploymentOfferController`)
   - Offer management
   - Documentation workflow
   - Digital signatures

7. **Admin** (`AdminController`)
   - User management
   - Role assignment
   - System analytics

8. **Delivery & Logistics** (`DeliveryController`, `PickupDropoffController`)
   - QR code scanning
   - Driver assignments
   - Real-time tracking

---

## 🔌 Services & Integrations

### Application Services

- **EmailService:** Automated email notifications
- **PaymentService:** Stripe payment processing
- **ContractService:** PDF contract generation
- **ESignatureService:** Digital signature workflow
- **QRCodeService:** QR code generation
- **BookingService:** Service booking management
- **RecruitmentEmailService:** Recruitment notifications
- **AIAssistantService:** AI chat assistant
- **VideoMeetingService:** Jitsi/Zoom integration
- **OcrService:** Resume and document parsing
- **ResumeParserService:** Resume extraction
- **AssessmentService:** AI-powered assessments

### External Integrations

- **Stripe:** Payment processing
- **OpenRouter:** AI services
- **Jitsi Meet:** Video conferencing
- **Zoom:** Interview scheduling
- **Azure SQL:** Database hosting
- **SMTP:** Email delivery

---

## 🧪 Testing

The application includes Playwright tests for critical workflows:

```bash
dotnet test
```

Test coverage includes:
- Service booking workflow
- QR code scanning
- Payment processing
- User authentication

---

## 📁 Project Structure

```
AutoEdge/
├── Areas/                    # ASP.NET Core Areas
│   └── Identity/            # Identity pages
├── Controllers/             # MVC Controllers
├── Data/                    # DbContext & seed data
├── Models/                  # Entity models and ViewModels
├── Services/                # Business logic services
├── Repositories/            # Data access layer
├── Views/                   # Razor views
├── wwwroot/                 # Static files
├── Migrations/              # EF Core migrations
├── Tests/                   # Test projects
├── Scripts/                 # Deployment scripts
└── Templates/               # Email templates
```

---

## 🌟 Key Features Highlights

### Real-time Notifications
- Email notifications for bookings, payments, and status updates
- Automated reminders and confirmations
- Service completion alerts

### Document Management
- PDF generation for contracts and invoices
- Secure document storage
- Digital signatures
- OCR document processing

### Payment Processing
- Secure Stripe integration
- Multiple payment methods
- Invoice management
- Payment history tracking

### AI-Powered Features
- Intelligent chat assistant
- Automated resume parsing
- Assessment grading
- Context-aware responses

### Mobile-Responsive
- Bootstrap 5 responsive design
- Mobile-friendly interfaces
- QR code scanning
- Touch-optimized controls

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Authors

- **AutoEdge Development Team**

---

## 🙏 Acknowledgments

- ASP.NET Core team
- Bootstrap contributors
- Stripe for payment services
- OpenRouter for AI services
- All open-source libraries used in this project

---

## 📞 Support

For support, email support@autoedge.com or open an issue in the repository.

---

## 🔄 Changelog

### Version 1.0.0
- Initial release
- Core dealership management features
- Vehicle sales and service management
- Recruitment and onboarding workflows
- AI assistant integration
- Payment processing
- Document management

---

**Built with ❤️ using ASP.NET Core**
