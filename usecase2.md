## Use Case Descriptions

### Use Case 16: Advertise Post

| **Field** | **Description** |
|-----------|-----------------|
| **Use case number** | 16 |
| **Use case name** | Advertise Post |
| **Brief description** | The admin creates and publishes new job or internship posts for open positions within the car dealership, such as IT, engineering, or sales departments. The system makes these advertisements visible to potential applicants. |
| **Triggering event** | Admin decides to advertise a new job opening or internship program |
| **Actor(s)** | Admin (HR/Recruitment Staff) |
| **Preconditions** | 1. The admin has valid login credentials.<br>2. The system is active and connected to the job postings database.<br>3. The position details and requirements are approved for posting. |
| **Flow of activities** | 1. Admin logs into the recruitment system.<br>2. Admin navigates to the "Advertise Post" section.<br>3. Admin fills in job details (title, department, description, requirements, deadlines).<br>4. System validates input information.<br>5. Admin confirms and submits the post.<br>6. System publishes the job post and makes it visible to applicants.<br>7. Admin can later edit or remove the post if necessary. |

### Use Case 17: Complete Application

| **Field** | **Description** |
|-----------|-----------------|
| **Use case number** | 17 |
| **Use case name** | Complete Application |
| **Brief description** | Applicants complete and submit online applications for job or internship openings, uploading all required documents such as CVs, certificates, and identification. |
| **Triggering event** | An applicant selects a job post and decides to apply. |
| **Actor(s)** | Applicant |
| **Preconditions** | 1. Applicant has access to the system.<br>2. Job posts are available in the system.<br>3. Applicant has the required documents ready for upload. |
| **Flow of activities** | 1. Applicant logs into the system.<br>2. Applicant views available job posts.<br>3. Applicant selects a post and clicks "Apply."<br>4. System opens the application form.<br>5. Applicant completes the form and uploads required documents.<br>6. System validates and stores the application.<br>7. Applicant receives confirmation of submission. |

### Use Case 18: Review Application

| **Field** | **Description** |
|-----------|-----------------|
| **Use case number** | 18 |
| **Use case name** | Review Application |
| **Brief description** | The admin reviews submitted applications, verifies applicants' qualifications, and selects suitable candidates for the next recruitment stage such as interviews or assessments. |
| **Triggering event** | The application period for a job post close or the admin chooses to review submitted applications. |
| **Actor(s)** | Admin (Recruiter/HR Staff) |
| **Preconditions** | 1. Applications have been submitted.<br>2. Admin has authorization to review applications.<br>3. System database contains all applicant records. |
| **Flow of activities** | 1. Admin logs into the system.<br>2. Admin opens the "Review Applications" section.<br>3. System displays all submitted applications for the post.<br>4. Admin filters or searches applications based on requirements.<br>5. Admin reviews documents and applicant details.<br>6. Admin shortlists qualified applicants.<br>7. System updates shortlisted applicants' status.<br>8. Shortlisted applicants are moved to the interview scheduling stage. |

### Use Case 19: Schedule Interview

| **Field** | **Description** |
|-----------|-----------------|
| **Use case number** | 19 |
| **Use case name** | Schedule Interview |
| **Brief description** | The admin schedules virtual live interviews for shortlisted applicants and sends notifications via email. |
| **Triggering event** | Applicants are shortlisted and ready for interviews. |
| **Actor(s)** | Admin, Applicant |
| **Preconditions** | 1. Applicants have been shortlisted.<br>2. Admin has access to scheduling tools.<br>3. Interview slots are available. |
| **Flow of activities** | 1. Admin logs into the system.<br>2. Admin selects the shortlisted applicants.<br>3. Admin chooses interview dates and times.<br>4. System sends interview invitations and details to applicants.<br>5. Applicants confirm availability through the system.<br>6. Admin finalizes and publishes the schedule. |

### Use Case 20: Conduct Interview

| **Field** | **Description** |
|-----------|-----------------|
| **Use case number** | 20 |
| **Use case name** | Conduct Interview |
| **Brief description** | Applicants attend a virtual live interview session where they answer predefined questions through a real-time video feature. The recruiter observes and records the session for evaluation. |
| **Triggering event** | Scheduled interview time arrives. |
| **Actor(s)** | Applicant, Recruiter |
| **Preconditions** | 1. The interview has been successfully scheduled.<br>2. The applicant has access to a device with a camera and stable connection.<br>3. The system supports live video functionality or integrated API. |
| **Flow of activities** | 1. Applicant logs into the system at the scheduled time.<br>2. System launches the live video interview interface.<br>3. Recruiter joins and greets the applicant.<br>4. Applicant answers predefined questions.<br>5. Recruiter evaluates and records notes.<br>6. System saves the interview video and data.<br>7. Recruiter ends the session and marks interview as complete. |

### Use Case 21: Complete Assessment

| **Field** | **Description** |
|-----------|-----------------|
| **Use case number** | 21 |
| **Use case name** | Complete Assessment |
| **Brief description** | After the interview, the system automatically assigns a relevant online assessment for the applicant to complete. The assessment evaluates skills related to the applied position. |
| **Triggering event** | Interview process is completed successfully. |
| **Actor(s)** | Applicant |
| **Preconditions** | 1. Applicant passed the interview stage.<br>2. Assessment questions are uploaded in the system.<br>3. Applicant has access to a stable internet connection. |
| **Flow of activities** | 1. System generates an assessment link for the applicant.<br>2. Applicant opens the link and starts the test.<br>3. System tracks time and submission progress.<br>4. Applicant submits answers.<br>5. System stores responses for grading. |

### Use Case 22: Grade Assessment

| **Field** | **Description** |
|-----------|-----------------|
| **Use case number** | 22 |
| **Use case name** | Grade Assessment |
| **Brief description** | The system automatically grades the completed assessment, stores the results, and notifies the admin for final review and hiring decision. |
| **Triggering event** | Applicant submits the completed assessment. |
| **Actor(s)** | System, Admin |
| **Preconditions** | 1. Applicant has completed and submitted the assessment.<br>2. The grading algorithm is configured in the system.<br>3. Admin has access rights to view results. |
| **Flow of activities** | 1. System receives submitted assessment answers.<br>2. System automatically grades the test.<br>3. Admin is notified of the results.<br>4. Admin reviews performance scores.<br>5. System updates applicant's status (successful/unsuccessful).<br>6. Notifications are sent to the applicant. |