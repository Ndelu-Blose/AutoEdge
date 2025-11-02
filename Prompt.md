# AutoEdge Recruitment Subsystem - Complete Requirements Document

## Project Overview
Build a complete recruitment management system for AutoEdge car dealership using **ASP.NET Core MVC (.NET 8)** that integrates with the main vehicle dealership platform.

---

## Core Departments
The system must support recruitment for these 4 departments:
1. **Mechanical Engineer**
2. **Sales Representative**
3. **Driver**
4. **Desktop Technician**

---

## PHASE 1: ADMIN JOB POSTING MODULE

### Requirements:
- Admin can create job postings with predefined department templates
- Each department has specific requirements automatically populated
- Job posting includes:
  - Job Title
  - Department (dropdown with 4 options)
  - Job Description
  - Requirements (auto-filled based on department)
  - Qualifications needed
  - Years of experience required
  - Number of positions available
  - Closing date
  - Job status (Active/Closed/Draft)

### Features:
- Admin dashboard showing all active/closed job postings
- Ability to edit or close job postings
- View application count per job posting

---

## PHASE 2: APPLICANT APPLICATION MODULE

### Requirements:
- Applicants browse "Vacant Positions" page showing all active jobs
- Click on job to view full details
- Apply button opens application form

### Application Form (Similar to Indeed.com):
**Personal Information:**
- First Name
- Last Name
- Email Address
- Phone Number
- Physical Address

**Documents Upload (Required):**
- Resume/CV (PDF format)
- Cover Letter (optional, PDF)
- ID Document (PDF)
- Certificates/Qualifications (PDF)

**Additional Information:**
- Years of Experience
- Highest Qualification
- Why are you suitable for this role? (text area)

### Features:
- **Auto-fill functionality** where possible (parse email for name, etc.)
- **Application Progress Bar** showing:
  - Personal Info (Step 1)
  - Document Upload (Step 2)
  - Review & Submit (Step 3)
- Form validation before submission
- **Application Status Dashboard** for applicants to track:
  - Submitted
  - Under Review
  - Shortlisted
  - Interview Scheduled
  - Assessment Sent
  - Final Decision

### Automatic Email Confirmation:
After submission, system sends email immediately:

```
Subject: Application Received - [Job Title] at AutoEdge

Dear [Applicant Name],

Thank you for applying for the [Job Title] position at AutoEdge.

We have received your application and our recruitment team will review it.

IMPORTANT: If you do not hear from us within 3 working days, please consider your application unsuccessful for this position.

We appreciate your interest in AutoEdge.

Best regards,
AutoEdge Recruitment Team
```

---

## PHASE 3: INTELLIGENT ATS & RESUME PARSING API

### Requirements:
Implement an **ATS-friendly Resume Parser** using OCR technology that:

### Extracts from PDF Resume:
- Full Name
- Contact Information (email, phone)
- Years of Experience
- Skills (technical and soft skills)
- Education/Qualifications
- Certifications
- Previous Job Titles
- Work History

### AI Matching System:
- System analyzes resume against job requirements
- Calculates **Match Score (0-100%)**
- Scoring criteria:
  - Years of experience match (30 points)
  - Skills match (40 points)
  - Qualification match (20 points)
  - Certifications (10 points)

### Automatic Shortlisting:
- System automatically ranks all applications by match score
- **Shortlisted:** Applications scoring 70% or above
- **Rejected:** Applications scoring below 70%

### Admin Dashboard Shows:
1. **Shortlisted Candidates** (green section)
   - Name, Match Score, Years of Experience
   - "View Application" button
   
2. **Rejected Candidates** (red section)
   - Name, Match Score, Rejection reason
   - Can manually override if needed

---

## PHASE 4: ADMIN CANDIDATE SELECTION MODULE

### Requirements:
- Admin reviews **only shortlisted candidates**
- Admin can:
  - View full application details
  - Download resume
  - Read parsed information
  - Add internal notes
  - Manually adjust candidate status

### Selection Process:
1. Admin filters candidates by department criteria
2. Admin selects candidates based on:
   - Match score
   - Manual resume review
   - Number of positions available
3. Admin checks boxes next to selected candidates
4. Admin clicks "Schedule Interviews for Selected" button

### Bulk Actions:
- Select multiple candidates at once
- Send rejection emails to unsuccessful candidates (bulk action)
- Move candidates to different stages

---

## PHASE 5: INTERVIEW SCHEDULING MODULE

### Requirements:
Admin schedules virtual interviews for all selected candidates **simultaneously**.

### Scheduling Form:
- **Select Interview Date** (date picker)
- **Select Time Slots** with validation:
  - No time slot conflicts
  - Each slot has duration (e.g., 30 min, 1 hour)
  - System shows available slots only
  - Slots are color-coded (green = available, red = booked)
- **Assign Recruiter/Interviewer:**
  - Name
  - Email
- **Meeting Platform:** Auto-generate virtual meeting link
- **Additional Instructions** (optional text)

### Time Slot Validation:
```
Example:
- 09:00 - 10:00 (Available)
- 10:00 - 11:00 (Booked - John Doe interview)
- 11:00 - 12:00 (Available)
```

### Bulk Interview Scheduling:
Admin can schedule multiple interviews at once:
- Select 5 candidates
- Choose 5 different time slots
- Assign same or different recruiters
- Click "Send All Interview Invitations"

### Automatic Interview Invitation Email:
System sends to all selected candidates simultaneously:

```
Subject: Interview Invitation - [Job Title] at AutoEdge

Congratulations [Applicant Name]!

Your application for [Job Title] has been shortlisted.

INTERVIEW DETAILS:
📅 Date: [Day, DD Month YYYY]
⏰ Time: [HH:MM]
⏱️ Duration: [30/60 minutes]
👤 Interviewer: [Recruiter Name]

JOIN VIRTUAL INTERVIEW:
🔗 Meeting Link: [Generated Virtual Meeting Link]
🔑 Meeting Password: [If applicable]

INSTRUCTIONS:
✓ Join 5 minutes before scheduled time
✓ Ensure stable internet connection
✓ Test camera and microphone beforehand
✓ Find a quiet, well-lit location
✓ Have your resume available

To reschedule, contact us 24 hours in advance.

We look forward to speaking with you!

Best regards,
[Recruiter Name]
AutoEdge Recruitment Team
Email: [Recruiter Email]
```

---

## PHASE 6: RECRUITER INTERVIEW MANAGEMENT

### Requirements:
Recruiter dashboard showing:
- **Upcoming Interviews** (sorted by date/time)
- Interview details for each candidate
- Quick access to candidate information

### Interview Card Display:
```
┌─────────────────────────────────────┐
│ John Doe - Mechanical Engineer      │
│ 📅 15 Oct 2025 | ⏰ 10:00 - 11:00   │
│ 🔗 Join Meeting                     │
│ 📄 View Application | 📝 Add Notes  │
└─────────────────────────────────────┘
```

### Interview Functionality:
- **Join Meeting Button** → Opens integrated virtual meeting
- **Interview Notes Section:**
  - Real-time note taking
  - Rate candidate (1-10 scale)
  - Mark technical skills
  - Communication assessment
  - Overall recommendation
- **Mark Interview as Completed** button
- System timestamps completion automatically

### Virtual Meeting Integration:
Integrate with **one** of these APIs:
- **Zoom API** (preferred)
- **Microsoft Teams API**
- **Google Meet API**
- **Daily.co API** (simple alternative)

Meeting features needed:
- Video/audio call
- Screen sharing
- Recording capability (optional)
- Chat functionality

---

## PHASE 7: AUTOMATED ASSESSMENT MODULE

### Trigger:
After interview is marked "Completed" by recruiter, system **automatically**:
1. Generates assessment test
2. Creates unique access token/link
3. Sends assessment email to candidate

### Assessment Email:
```
Subject: Complete Your Assessment - [Job Title] at AutoEdge

Dear [Applicant Name],

Thank you for attending the interview for [Job Title].

As the next step, please complete an online assessment.

ASSESSMENT DETAILS:
📝 Assessment: [Title]
⏰ Time Limit: [Duration]
📅 Due Date: [Date & Time]
🔗 Assessment Link: [Unique Token Link]

IMPORTANT:
- Must be completed before due date
- Once started, cannot be paused
- Answer all questions
- Ensure stable internet connection

Good luck!

AutoEdge Recruitment Team
```

### Assessment Types by Department:

**Mechanical Engineer:**
- Technical CAD/design questions
- Problem-solving scenarios
- Mathematical calculations

**Sales Representative:**
- Customer service scenarios
- Sales pitch evaluation
- Product knowledge

**Driver:**
- Road safety rules
- Vehicle maintenance basics
- Route optimization

**Desktop Technician:**
- IT troubleshooting scenarios
- Hardware/software knowledge
- Network basics

### Assessment Interface:
- Candidate clicks unique link
- System verifies token validity and due date
- Shows assessment instructions
- Timer starts when candidate clicks "Begin"
- Questions displayed one by one or all at once
- Question types:
  - Multiple choice
  - Short answer
  - Essay/paragraph
  - True/False
- Progress bar showing completion %
- Submit button (with confirmation)

### Assessment Features:
- Auto-save answers periodically
- Warning before time expires
- Cannot access after due date
- Single submission only (no retakes)

---

## PHASE 8: AUTOMATIC GRADING SYSTEM

### Requirements:
System automatically grades assessment **immediately** after submission.

### Grading Logic:

**Multiple Choice & True/False:**
- Auto-graded against correct answers
- Points awarded/deducted automatically

**Short Answer:**
- Keyword matching algorithm
- Checks for required terms
- Partial points for partial matches

**Essay Questions:**
- Word count check
- Keyword presence
- Basic sentiment analysis (positive/relevant content)
- Manual review option for admin

### Grading Calculation:
```
Total Points: 100
Question 1 (Multiple Choice): 10 points - ✓ Correct (10/10)
Question 2 (Short Answer): 15 points - ✓ Partial (10/15)
Question 3 (Essay): 20 points - ✓ Keywords found (18/20)
...

FINAL SCORE: 85/100 (85%)
```

### Pass/Fail Threshold:
- **Pass:** 70% or above
- **Fail:** Below 70%

### Admin Review Dashboard:
After grading, admin sees:

```
┌────────────────────────────────────────┐
│ Assessment Results - [Job Title]       │
├────────────────────────────────────────┤
│ Candidate: John Doe                    │
│ Score: 85/100 (85%) ✓ PASSED          │
│ Completed: 14 Oct 2025, 14:30         │
│                                        │
│ [View Detailed Answers]                │
│ [Download Report]                      │
│                                        │
│ Interview Rating: 8/10                 │
│ Overall Match Score: 82%               │
│                                        │
│ FINAL DECISION:                        │
│ ○ Hire  ○ Reject  ○ Further Review   │
│                                        │
│ [Send Decision Email]                  │
└────────────────────────────────────────┘
```

### Admin Actions:
1. Review auto-graded results
2. Manually adjust scores if needed
3. Compare with interview notes
4. Make final hiring decision
5. Send result notification

---

## PHASE 9: FINAL DECISION & NOTIFICATION

### Admin Final Decision Page:
Shows comprehensive candidate profile:
- Personal information
- Resume/documents
- Match score (from ATS)
- Interview rating
- Interview notes
- Assessment score
- Overall recommendation score

### Decision Actions:
1. **HIRE** → Sends success email with next steps
2. **REJECT** → Sends polite rejection email
3. **WAITLIST** → Keeps candidate for future openings

### Success Email:
```
Subject: Congratulations! Job Offer - [Job Title] at AutoEdge

Dear [Applicant Name],

Congratulations! We are pleased to offer you the position of [Job Title] at AutoEdge.

Your performance throughout the recruitment process has been outstanding.

NEXT STEPS:
1. Review attached employment contract
2. Complete pre-employment documentation
3. Schedule onboarding session

Please confirm your acceptance by [Date].

Welcome to the AutoEdge team!

Best regards,
AutoEdge HR Team
```

### Rejection Email:
```
Subject: Application Update - [Job Title] at AutoEdge

Dear [Applicant Name],

Thank you for your interest in the [Job Title] position.

After careful consideration, we have decided to move forward with other candidates.

We encourage you to apply for future opportunities at AutoEdge.

We wish you success in your career.

Best regards,
AutoEdge Recruitment Team
```

---

## TECHNICAL REQUIREMENTS

### Technology Stack:
- **Framework:** ASP.NET Core MVC (.NET 8)
- **Database:** SQL Server / PostgreSQL
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **File Storage:** Local file system or Azure Blob Storage

### Required NuGet Packages:
```
- iTextSharp / iText7 (PDF parsing)
- Tesseract OCR (OCR functionality)
- MailKit / SendGrid (Email service)
- Hangfire (Background jobs for email scheduling)
- Newtonsoft.Json (JSON handling)
- Zoom.Net / DailyAPI (Video conferencing)
```

### API Integrations:
1. **Resume Parser API** (OCR/ATS):
   - Option 1: Custom implementation using iTextSharp + Tesseract
   - Option 2: Sovren Resume Parser API
   - Option 3: DaXtra Parser API
   - Option 4: Build custom using OpenAI API

2. **Video Conferencing API:**
   - Option 1: Zoom API (https://marketplace.zoom.us/docs/api-reference/zoom-api)
   - Option 2: Daily.co API (https://www.daily.co/)
   - Option 3: Whereby API (https://whereby.com/information/embedded/)

3. **Email Service:**
   - SendGrid API
   - Or SMTP configuration

---

## DATABASE SCHEMA

### Tables Required:

**1. JobPostings**
- JobId (PK)
- JobTitle
- Department (enum)
- JobDescription
- Requirements
- Responsibilities
- MinYearsExperience
- RequiredQualifications
- PositionsAvailable
- PostedDate
- ClosingDate
- Status (Active/Closed/Draft)

**2. Applications**
- ApplicationId (PK)
- JobId (FK)
- FirstName
- LastName
- Email
- PhoneNumber
- Address
- ResumeFilePath
- CoverLetterPath
- IdDocumentPath
- CertificatesPath
- ParsedResumeText
- YearsOfExperience
- ExtractedSkills
- ExtractedEducation
- MatchScore
- Status (enum)
- SubmittedDate
- ReviewedDate
- AdminNotes

**3. Interviews**
- InterviewId (PK)
- ApplicationId (FK)
- JobId (FK)
- ScheduledDateTime
- DurationMinutes
- MeetingLink
- MeetingPassword
- RecruiterName
- RecruiterEmail
- IsCompleted
- CompletedDate
- InterviewNotes
- InterviewRating (1-10)
- EmailSent
- EmailSentDate

**4. InterviewSlots**
- SlotId (PK)
- StartTime
- EndTime
- IsBooked
- InterviewId (FK, nullable)
- RecruiterName

**5. Assessments**
- AssessmentId (PK)
- ApplicationId (FK)
- AssessmentTitle
- Instructions
- SentDate
- DueDate
- CompletedDate
- AccessToken (unique)
- IsCompleted
- Score (0-100)
- QuestionsJson (serialized)
- AnswersJson (serialized)
- EmailSent

**6. Users** (Admin/Recruiter)
- UserId (PK)
- Username
- Email
- PasswordHash
- Role (Admin/Recruiter)
- FullName

---

## USER ROLES & PERMISSIONS

### 1. Admin
- Create/edit/delete job postings
- View all applications
- Review shortlisted candidates
- Select candidates for interviews
- Schedule interviews (bulk)
- View interview results
- Review assessment results
- Make final hiring decisions
- Send all email notifications

### 2. Recruiter
- View assigned interviews
- Conduct interviews
- Add interview notes and ratings
- Mark interviews as completed
- Cannot schedule interviews (Admin only)
- Cannot make final hiring decisions

### 3. Applicant
- Browse vacant positions
- Submit applications
- Upload documents
- Track application status
- Receive email notifications
- Join scheduled interviews
- Complete assessments
- View application history

---

## UI/UX REQUIREMENTS

### Design Style:
- Modern, professional automotive industry theme
- Color scheme: Dark blue, silver, white
- Responsive design (mobile-friendly)
- Clean, intuitive navigation
- Progress indicators throughout

### Key Pages:

**Public Pages:**
- Home / Landing
- Vacant Positions (job listings)
- Job Details page
- Application Form (multi-step)
- Assessment page (token-based access)

**Applicant Dashboard:**
- My Applications
- Application Status
- Upcoming Interviews
- Pending Assessments
- Profile Settings

**Admin Dashboard:**
- Dashboard overview (stats)
- Job Postings Management
- Applications List (filterable)
- Shortlisted Candidates
- Interview Schedule
- Assessment Results
- Reports & Analytics

**Recruiter Dashboard:**
- My Upcoming Interviews
- Interview History
- Candidate Notes
- Quick Access to Meeting Links

---

## ADDITIONAL FEATURES (NICE TO HAVE)

1. **Analytics Dashboard:**
   - Applications per job
   - Success rate by department
   - Average time-to-hire
   - Candidate source tracking

2. **Notifications:**
   - Email notifications (implemented)
   - SMS notifications (optional)
   - In-app notifications

3. **Calendar Integration:**
   - Sync interviews with Google Calendar
   - iCal export for interview schedules

4. **Document Viewer:**
   - View PDFs directly in browser
   - No need to download

5. **Candidate Comparison:**
   - Side-by-side comparison of multiple candidates
   - Visual comparison charts

6. **Interview Recording:**
   - Auto-record video interviews
   - Store for later review

---

## DELIVERABLES

### Code Structure:
```
AutoEdge.Recruitment/
├── Controllers/
│   ├── AdminController.cs
│   ├── ApplicantController.cs
│   ├── RecruiterController.cs
│   ├── JobController.cs
│   ├── InterviewController.cs
│   └── AssessmentController.cs
├── Models/
│   ├── JobPosting.cs
│   ├── Application.cs
│   ├── Interview.cs
│   ├── Assessment.cs
│   └── ViewModels/
├── Services/
│   ├── IResumeParserService.cs
│   ├── ResumeParserService.cs
│   ├── IEmailService.cs
│   ├── EmailService.cs
│   ├── IVideoMeetingService.cs
│   └── VideoMeetingService.cs
├── Data/
│   ├── RecruitmentDbContext.cs
│   └── Migrations/
├── Views/
│   ├── Admin/
│   ├── Applicant/
│   ├── Recruiter/
│   ├── Jobs/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── uploads/
│   └── documents/
└── appsettings.json
```

### Documentation:
- README.md with setup instructions
- API documentation
- Database schema diagram
- User guide for Admin/Recruiter/Applicant

---

## SUCCESS CRITERIA

The system is complete when:
✅ Admin can post jobs for all 4 departments
✅ Applicants can apply with progress tracking
✅ Auto-email sent on application submission
✅ Resume parser extracts key information
✅ ATS system auto-shortlists candidates (70%+ match)
✅ Admin sees shortlisted vs rejected candidates
✅ Admin can schedule multiple interviews simultaneously
✅ No time slot conflicts in scheduling
✅ Bulk interview emails sent to all selected candidates
✅ Recruiter can view and join scheduled interviews
✅ Virtual meeting integration works (Zoom/Teams/Daily.co)
✅ Assessment auto-sent after interview completion
✅ Assessment auto-graded with score calculation
✅ Admin can make final hiring decision
✅ Success/rejection emails sent automatically
✅ All email templates are professional and complete

---

## IMPLEMENTATION PRIORITY

### Phase 1 (Week 1): Core Setup
- Database models & migrations
- Admin job posting CRUD
- Applicant registration

### Phase 2 (Week 2): Application System
- Multi-step application form
- Document upload
- Auto-confirmation email
- Application status tracking

### Phase 3 (Week 3): ATS & Resume Parser
- PDF parsing implementation
- Skills extraction
- Match score calculation
- Auto-shortlisting

### Phase 4 (Week 4): Interview Management
- Interview scheduling
- Time slot validation
- Bulk email invitations
- Recruiter dashboard

### Phase 5 (Week 5): Virtual Meetings
- Video API integration
- Meeting link generation
- Interview interface

### Phase 6 (Week 6): Assessment System
- Assessment creation
- Auto-send after interview
- Assessment interface
- Auto-grading algorithm

### Phase 7 (Week 7): Final Decision & Polish
- Decision workflow
- Final notification emails
- UI/UX improvements
- Testing & bug fixes

---

**END OF REQUIREMENTS DOCUMENT**

This document contains everything needed to build the complete AutoEdge Recruitment Subsystem. Follow each phase sequentially for best results.