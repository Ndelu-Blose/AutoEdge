# AutoEdge Views Documentation
## Complete View Structure and Styling Requirements

This document provides a comprehensive overview of all views in the AutoEdge application, organized by controller and functionality. Use this to create consistent styling across all pages.

---

## 🏠 **Home Views**

### **Home/Index.cshtml**
- **Purpose**: Landing page and main entry point
- **Content**: Hero section, features showcase, statistics, call-to-action
- **Key Elements**:
  - Hero banner with gradient background
  - Feature cards with icons (Quality Assurance, Expert Service, Competitive Pricing)
  - Statistics section (500+ Vehicles Sold, 98% Customer Satisfaction, etc.)
  - Services grid (Vehicle Sales, Financing, Documentation, Support)
  - Success popup modal for purchase confirmations
- **Styling Notes**: Modern gradient hero, card-based layout, responsive design

### **Home/Privacy.cshtml**
- **Purpose**: Privacy policy page
- **Content**: Standard privacy policy text
- **Styling Notes**: Simple text layout, consistent with site theme

---

## 🚗 **Vehicle Management Views**

### **Vehicle/Index.cshtml**
- **Purpose**: Vehicle inventory management (Admin/Sales)
- **Content**: 
  - Vehicle listing table with pagination
  - Search and filter controls
  - Sort options (Make, Model, Year, Price, Status)
  - Action buttons (Create, Edit, Delete, Toggle Status)
- **Key Elements**:
  - Data table with vehicle details
  - Pagination controls
  - Search input field
  - Status badges
- **Styling Notes**: Admin-style table, action buttons, status indicators

### **Vehicle/Details.cshtml**
- **Purpose**: Detailed vehicle information view
- **Content**:
  - Vehicle specifications
  - Image gallery
  - Related inquiries list
  - Action buttons
- **Key Elements**:
  - Image carousel/gallery
  - Specification table
  - Inquiry history
- **Styling Notes**: Image-focused layout, specification cards

### **Vehicle/Create.cshtml**
- **Purpose**: Add new vehicle to inventory
- **Content**:
  - Vehicle form (VIN, Make, Model, Year, etc.)
  - Image upload section
  - Pricing information
  - Features and specifications
- **Key Elements**:
  - Multi-section form
  - File upload area
  - Price input fields
- **Styling Notes**: Form-based layout, file upload styling

### **Vehicle/Edit.cshtml**
- **Purpose**: Edit existing vehicle information
- **Content**: Similar to Create but pre-populated
- **Key Elements**: Same as Create view
- **Styling Notes**: Same as Create view

### **Vehicle/Delete.cshtml**
- **Purpose**: Confirmation for vehicle deletion
- **Content**: Confirmation dialog with vehicle details
- **Key Elements**: Warning message, confirmation buttons
- **Styling Notes**: Alert-style layout, prominent warning

---

## 🔍 **Vehicle Browsing Views (Customer)**

### **VehicleBrowse/Index.cshtml**
- **Purpose**: Customer vehicle browsing and search
- **Content**:
  - Vehicle grid/card layout
  - Search and filter sidebar
  - Pagination
  - Quick view options
- **Key Elements**:
  - Vehicle cards with images
  - Filter sidebar
  - Search functionality
  - "View Details" buttons
- **Styling Notes**: E-commerce style grid, filter sidebar, hover effects

### **VehicleBrowse/Details.cshtml**
- **Purpose**: Customer vehicle detail view
- **Content**:
  - Large image gallery
  - Vehicle specifications
  - Pricing information
  - "Purchase" or "Inquire" buttons
  - Related vehicles
- **Key Elements**:
  - Image gallery/slider
  - Price display
  - CTA buttons
  - Specification list
- **Styling Notes**: Product page style, prominent CTAs, image gallery

---

## 👤 **Customer Views**

### **Customer/Dashboard.cshtml**
- **Purpose**: Customer main dashboard
- **Content**:
  - Purchase history
  - Active inquiries
  - Service bookings
  - Quick actions
- **Key Elements**:
  - Dashboard cards/widgets
  - Recent activity list
  - Quick action buttons
- **Styling Notes**: Dashboard layout, card-based widgets

### **Customer/Profile.cshtml**
- **Purpose**: Customer profile management
- **Content**:
  - Personal information form
  - Contact details
  - Preferences
  - Account settings
- **Key Elements**:
  - Profile form
  - Avatar upload
  - Settings toggles
- **Styling Notes**: Profile page layout, form styling

### **Customer/Documents.cshtml**
- **Purpose**: Document management
- **Content**:
  - Document list
  - Upload functionality
  - Status indicators
  - Download links
- **Key Elements**:
  - Document table
  - Upload area
  - Status badges
- **Styling Notes**: Document management interface

### **Customer/Inquiries.cshtml**
- **Purpose**: Customer inquiry history
- **Content**:
  - Inquiry list
  - Status tracking
  - Response history
- **Key Elements**:
  - Inquiry cards
  - Status timeline
- **Styling Notes**: Timeline layout, status indicators

---

## 💰 **Purchase Views**

### **Purchase/PurchaseWorkflow.cshtml**
- **Purpose**: Multi-step purchase process
- **Content**:
  - Progress indicator
  - Current step form
  - Navigation buttons
  - Vehicle summary
- **Key Elements**:
  - Step progress bar
  - Form sections
  - Vehicle summary card
- **Styling Notes**: Multi-step form, progress indicators

### **Purchase/PurchaseStatus.cshtml**
- **Purpose**: Purchase status tracking
- **Content**:
  - Status timeline
  - Current status
  - Next steps
  - Contact information
- **Key Elements**:
  - Timeline component
  - Status badges
  - Action buttons
- **Styling Notes**: Timeline layout, status visualization

### **Purchase/MyPurchases.cshtml**
- **Purpose**: Customer purchase history
- **Content**:
  - Purchase list
  - Status indicators
  - Action buttons
- **Key Elements**:
  - Purchase cards
  - Status badges
- **Styling Notes**: Card-based layout, status indicators

### **Purchase/SelectPayment.cshtml**
- **Purpose**: Payment method selection
- **Content**:
  - Payment options
  - Form fields
  - Total calculation
- **Key Elements**:
  - Payment method cards
  - Form inputs
  - Total display
- **Styling Notes**: Payment form, method selection

### **Purchase/PaymentConfirmation.cshtml**
- **Purpose**: Payment confirmation
- **Content**:
  - Confirmation details
  - Receipt information
  - Next steps
- **Key Elements**:
  - Success message
  - Receipt details
  - Action buttons
- **Styling Notes**: Success page, confirmation layout

---

## 📋 **Contract Views**

### **Contract/Sign.cshtml**
- **Purpose**: Electronic signature interface
- **Content**:
  - Contract document
  - Signature canvas
  - Signing instructions
- **Key Elements**:
  - Document viewer
  - Signature pad
  - Instructions panel
- **Styling Notes**: Signature interface, document viewer

---

## 🚚 **Delivery Views**

### **Delivery/Schedule.cshtml**
- **Purpose**: Delivery scheduling (Admin)
- **Content**:
  - Delivery list
  - Driver assignment
  - Scheduling interface
- **Key Elements**:
  - Delivery table
  - Calendar interface
  - Assignment controls
- **Styling Notes**: Admin interface, calendar integration

### **Delivery/Track.cshtml**
- **Purpose**: Delivery tracking
- **Content**:
  - Delivery status
  - Location tracking
  - ETA information
- **Key Elements**:
  - Status display
  - Map integration
  - Progress indicators
- **Styling Notes**: Tracking interface, map styling

### **Delivery/ScanQRCode.cshtml**
- **Purpose**: QR code scanning (Driver)
- **Content**:
  - QR scanner interface
  - Delivery confirmation
  - Customer information
- **Key Elements**:
  - Camera interface
  - Confirmation form
- **Styling Notes**: Mobile-friendly, camera interface

---

## 👨‍💼 **Admin Views**

### **Admin/Index.cshtml**
- **Purpose**: Admin dashboard
- **Content**:
  - System statistics
  - Recent activities
  - Quick actions
  - Delivery overview
- **Key Elements**:
  - Statistics cards
  - Activity feed
  - Action buttons
- **Styling Notes**: Admin dashboard, metric cards

### **Admin/Users.cshtml**
- **Purpose**: User management
- **Content**:
  - User list
  - Role management
  - User actions
- **Key Elements**:
  - User table
  - Role badges
  - Action buttons
- **Styling Notes**: Admin table, role indicators

### **Admin/Purchases.cshtml**
- **Purpose**: Purchase management
- **Content**:
  - Purchase list
  - Status management
  - Filter options
- **Key Elements**:
  - Purchase table
  - Status filters
  - Action buttons
- **Styling Notes**: Admin interface, status management

---

## 👥 **Recruitment Views**

### **RecruitmentAdmin/Index.cshtml**
- **Purpose**: Recruitment dashboard
- **Content**:
  - Job posting statistics
  - Recent applications
  - Quick actions
- **Key Elements**:
  - Statistics cards
  - Application list
  - Action buttons
- **Styling Notes**: Admin dashboard, recruitment metrics

### **RecruitmentAdmin/JobPostings.cshtml**
- **Purpose**: Job posting management
- **Content**:
  - Job posting list
  - Create/edit actions
  - Status management
- **Key Elements**:
  - Job posting cards
  - Status badges
  - Action buttons
- **Styling Notes**: Job posting cards, status indicators

### **RecruitmentAdmin/Applications.cshtml**
- **Purpose**: Application management
- **Content**:
  - Application list
  - Filter options
  - Bulk actions
- **Key Elements**:
  - Application table
  - Filter sidebar
  - Bulk action buttons
- **Styling Notes**: Application management interface

### **RecruitmentApplicant/VacantPositions.cshtml**
- **Purpose**: Job browsing for applicants
- **Content**:
  - Job posting grid
  - Search and filters
  - Application buttons
- **Key Elements**:
  - Job posting cards
  - Search interface
  - Apply buttons
- **Styling Notes**: Job board style, application CTAs

### **RecruitmentApplicant/Apply.cshtml**
- **Purpose**: Job application form
- **Content**:
  - Multi-step application
  - Document upload
  - Progress tracking
- **Key Elements**:
  - Step progress bar
  - Form sections
  - File upload
- **Styling Notes**: Multi-step form, progress indicators

### **RecruitmentApplicant/Assessment.cshtml**
- **Purpose**: Online assessment
- **Content**:
  - Assessment questions
  - Timer
  - Submission interface
- **Key Elements**:
  - Question interface
  - Timer display
  - Navigation buttons
- **Styling Notes**: Assessment interface, timer styling

---

## 🔧 **Service Views**

### **ServiceBooking/Index.cshtml**
- **Purpose**: Service booking list
- **Content**:
  - Booking history
  - Status tracking
  - New booking button
- **Key Elements**:
  - Booking cards
  - Status badges
  - Action buttons
- **Styling Notes**: Service booking interface

### **ServiceBooking/Create.cshtml**
- **Purpose**: New service booking
- **Content**:
  - Service form
  - Vehicle information
  - Scheduling options
- **Key Elements**:
  - Service form
  - Date picker
  - Vehicle selector
- **Styling Notes**: Service booking form

---

## 📊 **Reports Views**

### **Reports/Index.cshtml**
- **Purpose**: Reports dashboard
- **Content**:
  - Report categories
  - Quick access
  - Recent reports
- **Key Elements**:
  - Report cards
  - Category navigation
- **Styling Notes**: Reports dashboard

### **Reports/Sales.cshtml**
- **Purpose**: Sales reports
- **Content**:
  - Sales charts
  - Data tables
  - Export options
- **Key Elements**:
  - Charts/graphs
  - Data tables
  - Export buttons
- **Styling Notes**: Data visualization, export interface

---

## 🎨 **Shared Components**

### **Shared/_Layout.cshtml**
- **Purpose**: Main layout template
- **Content**:
  - Navigation bar
  - Sidebar (if applicable)
  - Footer
  - Scripts and styles
- **Key Elements**:
  - Responsive navbar
  - Role-based navigation
  - User authentication
  - Mobile menu
- **Styling Notes**: Main navigation, responsive design

### **Shared/_LoginPartial.cshtml**
- **Purpose**: User authentication partial
- **Content**:
  - Login/logout links
  - User profile link
  - Role-based menu items
- **Key Elements**:
  - User dropdown
  - Authentication links
- **Styling Notes**: User menu, authentication styling

### **Shared/Error.cshtml**
- **Purpose**: Error page template
- **Content**:
  - Error message
  - Error details
  - Navigation options
- **Key Elements**:
  - Error display
  - Navigation buttons
- **Styling Notes**: Error page styling

---

## 🎯 **Styling Requirements Summary**

### **Design System Elements**
1. **Color Scheme**: Primary blue (#007bff), Success green, Warning yellow, Danger red
2. **Typography**: Bootstrap default with custom headings
3. **Spacing**: Consistent padding and margins
4. **Components**: Cards, buttons, forms, tables, modals
5. **Icons**: Font Awesome 6 integration
6. **Responsive**: Mobile-first approach

### **Key Styling Patterns**
- **Cards**: Consistent card styling for content blocks
- **Tables**: Admin-style tables with hover effects
- **Forms**: Bootstrap form styling with validation
- **Buttons**: Consistent button styling with hover states
- **Navigation**: Responsive navbar with dropdown menus
- **Modals**: Bootstrap modal styling
- **Status Indicators**: Color-coded badges and indicators
- **Progress Bars**: Multi-step form progress indicators

### **Layout Patterns**
- **Dashboard**: Card-based widget layout
- **List Views**: Table or card-based listings
- **Detail Views**: Image-focused with sidebar information
- **Form Views**: Multi-section forms with progress tracking
- **Admin Views**: Data-heavy interfaces with bulk actions

### **Interactive Elements**
- **Hover Effects**: Subtle animations on cards and buttons
- **Loading States**: Spinner indicators for async operations
- **Success/Error Messages**: Toast notifications or modal alerts
- **File Upload**: Drag-and-drop styling
- **Search/Filter**: Sidebar filters with real-time updates

---

## 📱 **Responsive Considerations**

### **Breakpoints**
- **Mobile**: < 768px (single column, stacked layout)
- **Tablet**: 768px - 1024px (two column, adjusted spacing)
- **Desktop**: > 1024px (full layout, sidebar navigation)

### **Mobile-Specific Styling**
- **Navigation**: Collapsible hamburger menu
- **Tables**: Horizontal scroll or card conversion
- **Forms**: Full-width inputs, larger touch targets
- **Modals**: Full-screen on mobile
- **Images**: Responsive image sizing

---

This documentation provides a complete overview of all views in the AutoEdge application. Use this to create consistent styling prompts for Cursor that will ensure a cohesive design system across all pages.
