# Office Calendar and Event Application 

## Description
The Office Calendar and Event application is designed to streamline event management and employee status tracking within an organization. 
There are *two types of users*: **admins** and **employees**. 
- **Admins** have access to a dashboard where they can create events and perform CRUD (Create, Read, Update, Delete) operations on them. Admins can also view who is attending events, access event ratings and feedback from past events, and filter events based on specific criteria.

- **Employees** can log in to view upcoming events and indicate their interest in attending future events. They can also provide feedback and ratings for events they have attended. Additionally, employees can indicate their working status—whether they are on-premises, working from home, or away. This status is visible to their peers, allowing everyone to see who is available in the office on a given day or time.

- **Extra** To enhance the application further, several exciting features could be included: 
    - a notification system for event reminders and status updates, 
    - real-time updates using WebSockets or SignalR, 
    - an analytics dashboard for admins to view event attendance and feedback trends, 
    - event categorization for easier filtering and searching, 
    - integration with external calendars like Google Calendar and Outlook, 
    - user profile pages for employees to manage their information, 
    - an event invitation system to send and track RSVPs, 
    - and full mobile responsiveness to ensure a seamless experience on any device.

----

# Project Breakdown

## 1. Backend (ASP.NET)

1.1. **Authentication and Authorization**

    - Develop user authentication (login, logout, register).
    - Implement authorization for admin and employee roles.
    - [optional] Use Identity framework for handling user roles.

1.2. **Event Management**

    - CRUD operations for events.
    - Filtering events based on criteria.
    - View attendees of the events.

1.3. **Feedback and Ratings**

    - CRUD operations for feedback and ratings.
    - Associate feedback and ratings with events.
    - Display past event feedback and ratings to the admin.

1.4. **Employee Status Management**

    - CRUD operations for employee working status.
    - Track whether employees are working on-premises, from home, or away.
    - Display employee status to peers.

> **Very Important note:**
>    - Implement dependency injection for services.
>    - Create middleware for logging, error handling, etc.
>    - Develop controllers and service layers for all modules.

## 2. Frontend (React with TypeScript)

2.1.  **Authentication and Authorization UI**

    - Create login, logout, and registration components.
    - Handle authentication state and role-based UI rendering.

2.2.  **Event Management UI**

    - Develop components for viewing, creating, updating, and deleting events.
    - Implement event filtering and search functionality.
    - Display event details and attendees list.

2.3. **Feedback and Ratings UI**

    - Create components for submitting feedback and ratings.
    - Display past event feedback and ratings to employees and admins.
    
2.4. **Employee Status Management UI**

    - Develop components for setting and displaying employee status.
    - Implement a calendar view showing employee availability.

2.5. **Global Components and State Management**

    - Implement global state management.
    - Create common UI components (e.g., navigation, modals).
    - Ensure responsive design and accessibility.

----

# Diagrams

## ERD (Entity-Relationship Diagram)

- User (UserID, Name, Email, PasswordHash, Role)
- Event (EventID, Title, Description, Date, Location, CreatedBy)
- EmployeeStatus (StatusID, UserID, Date, Status)

- Attendance (AttendanceID, EventID, UserID, Status)
- Feedback (FeedbackID, EventID, UserID, Rating, Comments)
- -*OR* -
- EventResponse (ResponseID, EventID, UserID, Rating, Comments)
* It could be a database design choice to have either EventResponse as 1 table to record the attendance and feedback or use two tables as above.

## Use Case Diagram

- **Admin:** Manage Events, View Attendees, View Feedback, Filter Events
- **Employee:** View Events, Attend Events, Give Feedback, Set Status, View Peer Status
- *both users Admin and Employee have common use of login etc*

## Wire-frames

1. [**Login**](https://htmlpreview.github.io/?https://github.com/hogeschool/webdev-semester/blob/main/24-25/Project%20Ideas/Office%20Calendar/WireFrame/index.html) Dummy username emp or admin, password pwd, 

2. [**Admin Dashboard**](https://htmlpreview.github.io/?https://github.com/hogeschool/webdev-semester/blob/main/24-25/Project%20Ideas/Office%20Calendar/WireFrame/admin-dashboard.html)
Event Management: List of events with CRUD options.
Attendees View: List of attendees for a selected event.
Feedback View: Feedback and ratings for past events.
Filter Events: Filters for events based on date, type, etc.

3. [**Employee Dashboard**](https://htmlpreview.github.io/?https://github.com/hogeschool/webdev-semester/blob/main/24-25/Project%20Ideas/Office%20Calendar/WireFrame/employee-dashboard.html)
Event List: List of upcoming events with option to attend.
Feedback Form: Form for submitting feedback on attended events.
Status Management: Calendar view for setting and viewing status.
Peer Status: View the status of peers.

