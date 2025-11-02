# AutoEdge Employee Onboarding Module - Implementation Requirements

## Overview
After a candidate is selected for hire, implement a **secure employee onboarding system** where the newly hired employee must complete two critical steps **inside the system** before officially joining AutoEdge.

---

## STEP 1: EMPLOYMENT CONTRACT REVIEW & ACCEPTANCE

### Requirements:

**Admin Action:**
- After making final hiring decision, Admin sends employment offer
- System generates unique secure access link/token for the hired employee
- Email sent to employee with congratulations and access link

**Employee Access (Token-Based):**
- Employee clicks unique link from email
- No login required - token-based authentication
- Link expires after 7 days

**Contract Review Interface:**
Display employment offer details:
- **Job Title:** Desktop Support Technician
- **Department:** IT Department
- **Salary Offered:** R15,000 per month
- **Start Date:** 24 October 2025
- **Employment Type:** Full-time
- **Work Location:** AutoEdge, Durban

**Full Employment Contract Display:**
- System generates and displays complete employment contract as PDF viewer embedded in page
- Employee can read full contract including:
  - Terms and conditions
  - Job responsibilities
  - Working hours (08:00 - 17:00, Monday-Friday)
  - Salary details
  - Leave entitlements (21 days annual leave)
  - Notice period (30 days)
  - Probation period (3 months)
  - Confidentiality agreement
  - Company policies
  - Benefits and perks

**Digital Signature Feature:**
- Canvas drawing pad for employee to sign electronically
- "Clear" button to redo signature
- Signature preview shown

**Contract Acceptance:**
Employee must:
1. ✅ Check box: "I have read and understood the employment contract"
2. ✅ Check box: "I accept the terms and conditions"
3. Draw digital signature
4. Click **"Accept Contract"** button

Alternative:
- "Decline Offer" button with reason text field

**After Acceptance:**
- System saves contract with signature and timestamp
- Contract marked as "Accepted" in database
- Employee automatically redirected to Step 2 (Pre-Employment Documentation)
- Admin receives notification of contract acceptance
- Confirmation email sent to employee

**After Decline:**
- System records rejection with reason
- Admin receives notification
- Employee sees thank you message
- Process ends

---

## STEP 2: PRE-EMPLOYMENT DOCUMENTATION

### Requirements:

**Access:**
- Automatically redirected after contract acceptance
- Can also access via unique link if not completed
- Multi-page form with progress indicator (shows "Step 1 of 8" etc.)
- Auto-save draft functionality (save progress and complete later)

### Form Sections:

#### **SECTION 1: PERSONAL INFORMATION**

**Identity Details:**
- South African ID Number (13 digits, validated)
- Date of Birth (auto-extracted from ID number)
- Gender (dropdown: Male, Female, Other)
- Nationality (dropdown with country list)
- Marital Status (Single, Married, Divorced, Widowed)

**Contact Information:**
- Residential Address (Street address)
- City/Town
- Province (dropdown: Gauteng, KwaZulu-Natal, Western Cape, etc.)
- Postal Code
- Alternative Phone Number (optional)

---

#### **SECTION 2: EMERGENCY CONTACT**

**Primary Emergency Contact:**
- Full Name (required)
- Relationship (dropdown: Spouse, Parent, Sibling, Friend, Other)
- Phone Number (required)
- Alternative Phone Number (optional)
- Residential Address

**Secondary Emergency Contact (Optional):**
- Same fields as primary

---

#### **SECTION 3: BANKING DETAILS**

**Bank Account Information:**
- Bank Name (dropdown: FNB, Standard Bank, ABSA, Nedbank, Capitec, etc.)
- Account Type (dropdown: Savings, Cheque/Current)
- Account Number (validated format)
- Branch Code (6 digits)
- Account Holder Name (must match employee name)
- Proof of Bank Account Upload:
  - Upload recent bank statement (PDF, max 5MB)
  - Or upload bank confirmation letter
  - Preview uploaded document

---

#### **SECTION 4: TAX INFORMATION**

**SARS Details:**
- Tax Number (validated format)
- Registered for Tax? (Yes/No radio buttons)
- If Yes: Upload Tax Clearance Certificate (optional, PDF)
- Tax Directive Number (optional)

---

#### **SECTION 5: MEDICAL INFORMATION**

**Medical Aid (Optional):**
- Do you have medical aid? (Yes/No)
- If Yes:
  - Medical Aid Provider Name
  - Medical Aid Number
  - Main Member or Dependant?

**Health Declaration:**
- Do you have any chronic medical conditions? (Yes/No)
- If Yes: Provide details (text area, 500 characters)
- Are you on any chronic medication? (Yes/No)
- Do you have any disabilities requiring workplace accommodation? (Yes/No)
  - If Yes: Provide details

---

#### **SECTION 6: DEPENDENTS INFORMATION**

**Dependents:**
- Number of Dependents (dropdown: 0-10+)
- If more than 0, add dependents dynamically:

For each dependent:
- Full Name
- Relationship (Child, Spouse, Parent, Other)
- Date of Birth
- ID Number (optional)
- "Add Another Dependent" button
- "Remove" button for each entry

---

#### **SECTION 7: REQUIRED DOCUMENTS UPLOAD**

**Mandatory Documents:**

1. **Certified Copy of ID Document**
   - Upload PDF/Image (max 5MB)
   - Must be certified within last 3 months
   - Preview after upload

2. **Proof of Residential Address**
   - Upload utility bill, bank statement, or municipal account
   - Must be dated within last 3 months
   - PDF/Image (max 5MB)

3. **Certified Qualification Certificates**
   - Upload all relevant qualifications
   - Multiple file upload (up to 10 files)
   - PDF format

4. **Driver's License** (if applicable for role)
   - Upload both sides
   - PDF/Image format

**Optional Documents:**
- Professional Certifications
- Reference Letters
- Portfolio/Work Samples

**Upload Features:**
- Drag-and-drop file upload
- Click to browse
- File type validation (PDF, JPG, PNG only)
- File size validation (max 5MB per file)
- Progress bar during upload
- Preview uploaded files
- Delete/replace uploaded files
- "Upload All" or individual upload buttons

---

#### **SECTION 8: DECLARATIONS & DIGITAL SIGNATURE**

**Legal Declarations:**

Employee must check all boxes:

- ☐ **Criminal Record Declaration**
  - "Do you have any criminal convictions?" (Yes/No)
  - If Yes: Provide details (text area)

- ☐ **Accuracy Declaration**
  - "I declare that all information provided is true and accurate to the best of my knowledge"

- ☐ **Background Check Consent**
  - "I consent to AutoEdge conducting background verification checks including criminal, credit, and qualification verification"

- ☐ **Data Processing Consent**
  - "I consent to AutoEdge processing my personal information in accordance with POPIA (Protection of Personal Information Act)"

- ☐ **Document Authenticity**
  - "I confirm that all uploaded documents are authentic and not forged"

**Digital Signature:**
- Canvas signature pad (larger than contract signature)
- "Clear Signature" button
- "I have read and agree to all declarations above" checkbox
- Date auto-populated (current date)

**Submit Button:**
- Large "Submit Pre-Employment Documentation" button
- Confirmation dialog: "Are you sure? You cannot edit after submission"
- Loading spinner during submission

**Validation:**
- All required fields must be completed
- All required documents must be uploaded
- All checkboxes must be ticked
- Signature must be drawn
- Show error messages for missing fields with red highlighting

---

## AFTER SUBMISSION (Employee View)

**Success Page:**
Display:
- ✅ Checkmark icon
- "Documentation Submitted Successfully!"
- Submission reference number
- Timestamp of submission
- "What happens next?" section:
  - HR will review your documentation within 2 working days
  - You will receive an email confirmation
  - Onboarding session will be scheduled
  - You will receive onboarding details via email

**Confirmation Email to Employee:**
```
Subject: Pre-Employment Documentation Received - AutoEdge

Dear [Employee Name],

Thank you for completing your pre-employment documentation.

We have successfully received all your information and documents.

NEXT STEPS:
✓ Our HR team will review your documentation
✓ You will be contacted within 2 working days
✓ We will schedule your onboarding session
✓ Onboarding details will be sent via email

Reference Number: [REF123456]
Submitted: [Date & Time]

Welcome to the AutoEdge family!

Best regards,
AutoEdge HR Team
```

---

## ADMIN REVIEW DASHBOARD

**Admin Interface Requirements:**

**Pending Documentation Review Page:**
Show list of all submitted documentations:
- Employee Name
- Position
- Submission Date
- Status (Pending Review, Approved, Rejected)
- "Review" button

**Documentation Review Page:**
Display all information in organized sections with tabs:

**TAB 1: Personal Information**
- All personal details displayed
- Editable fields if corrections needed

**TAB 2: Contact & Emergency**
- Contact details
- Emergency contact information

**TAB 3: Financial**
- Banking details
- Tax information
- Downloaded bank statement link

**TAB 4: Medical & Dependents**
- Medical information
- List of dependents

**TAB 5: Documents**
- All uploaded documents displayed
- Click to view/download each document
- PDF viewer embedded in page
- "Document appears valid" checkbox for each document
- Admin notes field for each document

**TAB 6: Declarations**
- All declarations shown
- View criminal record details if declared
- View digital signature

**Admin Actions:**

**Bottom of Page:**
1. **Approve Documentation** button (green)
   - Text area for approval notes
   - Select "Schedule Onboarding" checkbox
   - Click Approve

2. **Request Corrections** button (yellow)
   - Text area to specify what needs correction
   - Sends email to employee with correction requests
   - Employee can re-upload or edit specific fields

3. **Reject Documentation** button (red)
   - Text area for rejection reason
   - Confirmation dialog
   - Ends onboarding process

**After Admin Approval:**

System actions:
- Update status to "Approved"
- Record admin name and approval timestamp
- Send approval email to employee
- Unlock onboarding session scheduling

**Email to Employee (Approval):**
```
Subject: Documentation Approved - Next Steps for Onboarding

Dear [Employee Name],

Great news! Your pre-employment documentation has been approved.

We are excited to welcome you to the AutoEdge team.

Your onboarding session will be scheduled soon. You will receive 
a separate email with the date, time, and location details.

START DATE: 24 October 2025

Please prepare to bring:
- Original ID document
- Original qualification certificates
- Driver's license (if applicable)

See you soon!

Best regards,
AutoEdge HR Team
```

---

## TECHNICAL IMPLEMENTATION REQUIREMENTS

### Database Tables Needed:

**EmploymentOffers Table:**
- OfferId (PK)
- ApplicationId (FK)
- JobTitle
- Department
- SalaryOffered
- StartDate
- EmploymentType
- WorkLocation
- ContractFilePath
- OfferSentDate
- OfferExpiryDate
- AccessToken (unique, 40 characters)
- ContractAccepted (boolean)
- ContractAcceptedDate
- ContractSignature (base64 text)
- RejectionReason
- Status (enum: Pending, ContractAccepted, ContractRejected, DocumentationCompleted, Approved)

**PreEmploymentDocumentation Table:**
- DocumentationId (PK)
- OfferId (FK)
- IdNumber
- DateOfBirth
- Gender
- Nationality
- MaritalStatus
- ResidentialAddress
- City
- Province
- PostalCode
- EmergencyContactName
- EmergencyContactRelationship
- EmergencyContactPhone
- EmergencyContactAddress
- SecondaryEmergencyContact (JSON)
- BankName
- AccountType
- AccountNumber
- BranchCode
- AccountHolderName
- BankStatementPath
- TaxNumber
- RegisteredForTax
- TaxClearancePath
- HasMedicalAid
- MedicalAidProvider
- MedicalAidNumber
- HasChronicConditions
- ChronicConditionsDetails
- NumberOfDependents
- DependentsDetails (JSON)
- CertifiedIdPath
- ProofOfAddressPath
- QualificationCertificatesPath (JSON array)
- DriversLicensePath
- HasCriminalRecord
- CriminalRecordDetails
- DeclareAccurate
- ConsentBackgroundCheck
- ConsentDataProcessing
- DigitalSignature (base64)
- SignedDate
- IsCompleted
- CompletedDate
- AdminReviewed
- AdminReviewedDate
- ReviewedBy
- Approved
- AdminNotes

### Features to Implement:

**Security:**
- Unique token generation (40 character random string)
- Token expiry validation (7 days)
- One-time submission (cannot edit after final submit)
- Secure file upload with validation

**File Handling:**
- File type validation (PDF, JPG, PNG only)
- File size limit (5MB per file)
- Virus scanning (optional but recommended)
- Secure storage with unique filenames
- Preview functionality for uploaded files

**Form Features:**
- Multi-step wizard with progress bar
- Auto-save draft (every 2 minutes or on blur)
- Client-side and server-side validation
- Real-time validation feedback
- Signature pad (use library like signature_pad.js)
- Drag-and-drop file upload
- Dynamic form fields (add/remove dependents)

**Email Service:**
- Contract acceptance confirmation
- Documentation submission confirmation
- Admin notification on submission
- Approval notification to employee
- Correction request notification

**Admin Features:**
- Filter/search documentation by status
- Sort by submission date
- Bulk approve (if needed)
- Download all documents as ZIP
- Print documentation summary

---

## USER FLOW SUMMARY

### **Employee Journey:**
1. Receives congratulations email with unique access link
2. Clicks link → Lands on welcome page
3. Reviews employment offer details
4. Reads full employment contract (scrollable PDF viewer)
5. Signs contract digitally
6. Clicks "Accept Contract" → Auto-saved
7. Redirected to pre-employment documentation form
8. Completes 8 sections of form
9. Uploads required documents (with progress indicators)
10. Reviews all information
11. Signs declarations
12. Submits → Sees success message
13. Receives confirmation email
14. Waits for HR review (2 working days)
15. Receives approval email
16. Receives onboarding schedule email

### **Admin Journey:**
1. After hiring decision, creates employment offer
2. System generates contract and sends email to employee
3. Receives notification when employee accepts contract
4. Receives notification when documentation submitted
5. Reviews all submitted information and documents
6. Validates documents
7. Approves or requests corrections
8. Schedules onboarding session
9. Employee starts work on scheduled start date

---

## SUCCESS CRITERIA

The implementation is complete when:

✅ Admin can send employment offer to hired candidate
✅ Unique secure access link generated and emailed
✅ Employee can access offer using token (no login)
✅ Full employment contract displayed in PDF viewer
✅ Employee can sign contract digitally (signature pad)
✅ Contract acceptance saves signature and timestamp
✅ Employee automatically redirected to documentation form
✅ 8-section pre-employment form works with validation
✅ All required fields validated before submission
✅ File upload works with drag-and-drop and validation
✅ Dynamic dependents section (add/remove)
✅ Auto-save draft functionality works
✅ Digital signature captured on documentation
✅ All declarations must be checked before submit
✅ Submit confirmation dialog appears
✅ Success page shows after submission
✅ Confirmation emails sent automatically
✅ Admin sees pending documentation in dashboard
✅ Admin can review all information in organized tabs
✅ Admin can view/download all uploaded documents
✅ Admin can approve, reject, or request corrections
✅ Approval email sent to employee automatically
✅ Status tracking throughout entire process
✅ No editing allowed after final submission
✅ Token expires after 7 days

---

**END OF ONBOARDING MODULE REQUIREMENTS**

This module integrates seamlessly with the existing AutoEdge Recruitment System and ensures all new hires complete required documentation digitally before their start date.