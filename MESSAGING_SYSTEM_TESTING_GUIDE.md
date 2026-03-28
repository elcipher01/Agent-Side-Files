# Messaging System Setup & Testing Guide

## ✅ What Has Been Implemented

### 1. **Database Initialization (Automatic)**
- File: `Data/DbInitializer.cs`
- **Automatically creates test data on first application startup**
- Creates 4 conversations for user 221 (Arch):
  1. GCash Payment Issue
  2. Order Tracking Problem
  3. Product Quality Concern
  4. Delivery Delay
- Creates 10 test messages across all conversations
- Messages marked as read/unread for testing purposes

### 2. **API Endpoints** (`Controllers/MessagingController.cs`)
Five RESTful endpoints for messaging:

- **GET `/api/messaging/conversations/221`**
  - Returns all conversations for user 221
  - Includes unread message count for each conversation
  - Ordered by most recently updated

- **GET `/api/messaging/messages/{conversationId}`**
  - Returns all messages in a specific conversation
  - Automatically marks messages as read
  - Ordered chronologically

- **POST `/api/messaging/send`**
  - Send a new message to a conversation
  - Updates conversation's `UpdatedAt` timestamp
  - Request body:
    ```json
    {
      "conversationId": 1,
      "userId": 221,
      "senderStaffId": 1,
      "message": "Your message here",
      "messageType": "text"
    }
    ```

- **POST `/api/messaging/create-conversation`**
  - Create a new conversation for a user
  - Request body:
    ```json
    {
      "userId": 221,
      "title": "Conversation Title",
      "description": "Description here",
      "category": "General"
    }
    ```

- **GET `/api/messaging/conversation-detail/{conversationId}`**
  - Get full conversation details with all messages

### 3. **Frontend UI** (`Views/Agent/Messaging.cshtml`)
- 2-column layout:
  - **Left Panel (300px)**: Conversations list
  - **Right Panel**: Messages thread
- Features:
  - Conversation list with title, preview, and unread count
  - Active conversation highlighting
  - Message display with sender distinction (user vs staff)
  - Message input form
  - Real-time UI updates

### 4. **JavaScript API Client** (`wwwroot/js/messaging.js`)
- `MESSAGING_API` object with all 5 API methods
- `MessagingUI` class for DOM management
- Automatically loads conversations on page load
- Keyboard support (Ctrl+Enter to send)
- Auto-scroll to latest message

---

## 🚀 How to Test

### **Step 1: Start the Application**
```bash
dotnet run
```
- Application will automatically initialize the database on first run
- Watch the console for: `✅ Database initialized successfully with test data for user 221 (Arch)`

### **Step 2: Login to the Agent Dashboard**
- URL: `https://localhost:7090` (or your local port)
- Credentials:
  - Username: `elton.bernil`
  - Password: `Bernil2026`
- Expected: Redirected to `/Agent/HelpCenter`

### **Step 3: Navigate to Messaging**
- Click "Messaging" in the agent navigation menu
- OR go directly to: `/Agent/Messaging`
- Expected: 4 conversations should load:
  1. ✉️ GCash Payment Issue (1 unread)
  2. ✉️ Order Tracking Problem
  3. ✉️ Product Quality Concern (1 unread)
  4. ✉️ Delivery Delay (2 unread)

### **Step 4: Test Conversation Selection**
- Click on "GCash Payment Issue" conversation
- Expected: Message thread displays with 3 messages showing:
  - Messages alternating between customer (Arch) and support staff
  - Timestamps for each message
  - Read/Unread status

### **Step 5: Test Message Viewing**
- Click on "Product Quality Concern"
- Expected: 3 messages display
- Expected: Messages marked as unread become read (badge updates in list)

### **Step 6: Test Sending a Message**
- In the message input field, type: "This is a test message"
- Press "Send" or press Ctrl+Enter
- Expected:
  - Message appears in chat thread immediately
  - Sent as staff member (SenderStaffId from session)
  - Timestamp updates
  - Input field clears
  - Conversation "Updated" timestamp changes

### **Step 7: Test Creating New Conversation**
- Click "+ New Conversation" button
- Fill in the form:
  - Title: "Test Issue"
  - Description: "Testing new conversation"
  - Category: "Billing"
- Click "Create"
- Expected:
  - New conversation appears at top of list
  - Can click and add messages to it

---

## 🔍 Testing with Postman/curl

### **Test 1: Get Conversations for User 221**
```bash
curl -X GET "https://localhost:7090/api/messaging/conversations/221"
```

Expected Response:
```json
{
  "success": true,
  "message": "Conversations retrieved successfully",
  "data": [
    {
      "id": 1,
      "userId": 221,
      "title": "GCash Payment Issue",
      "description": "...",
      "category": "Billing",
      "status": "Active",
      "createdAt": "2024-03-22T10:00:00",
      "updatedAt": "2024-03-22T10:00:00",
      "unreadCount": 1
    },
    ...
  ]
}
```

### **Test 2: Get Messages for Conversation 1**
```bash
curl -X GET "https://localhost:7090/api/messaging/messages/1"
```

### **Test 3: Send a New Message**
```bash
curl -X POST "https://localhost:7090/api/messaging/send" \
  -H "Content-Type: application/json" \
  -d '{
    "conversationId": 1,
    "userId": 221,
    "senderStaffId": 1,
    "message": "Test message from API"
  }'
```

### **Test 4: Create New Conversation**
```bash
curl -X POST "https://localhost:7090/api/messaging/create-conversation" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 221,
    "title": "New Issue",
    "description": "Testing",
    "category": "Support"
  }'
```

---

## 🗄️ Database Structure

### **support_faq Table** (Conversations)
```
id (PK)           | int
user_id           | int           (221 = Arch)
title             | nvarchar(255) 
description       | nvarchar(max)
category          | nvarchar(100)
status            | nvarchar(20)  (Active/Closed)
created_at        | datetime
updated_at        | datetime
```

### **support_messages Table** (Messages)
```
id (PK)           | int
conversation_id   | int           (FK → support_faq.id)
user_id           | int           (221 = customer)
sender_staff_id   | int           (staff member responding)
message           | nvarchar(max)
sent_at           | datetime
is_read           | bit           (0/1)
message_type      | nvarchar(50)  (text/file/etc)
attachment_url    | nvarchar(500)
```

---

## 🐛 Troubleshooting

### **Issue: "No conversations found"**
- Check if database initialization ran (check console output)
- Verify user 221 exists and is being sent to API
- Check if tables were created: Run SQL query:
  ```sql
  SELECT * FROM support_faq WHERE user_id = 221;
  SELECT * FROM support_messages;
  ```

### **Issue: "401 Unauthorized" on API calls**
- Ensure you're logged in first (session required)
- Verify AuthenticationFilter is in place
- Check session cookies in browser DevTools

### **Issue: Messages not sending**
- Check browser console for JavaScript errors (F12 → Console)
- Verify message is not empty
- Check Network tab to see actual API response
- Verify `sender_staff_id` is being sent in request

### **Issue: Messages appearing as read**
- This is expected behavior - the API automatically marks messages as read when fetched
- To test unread messages, check database directly before viewing in UI

---

## 📊 Verify Database Setup

### **In SQL Server Management Studio:**

```sql
-- Check if conversations exist for user 221
SELECT * FROM support_faq WHERE user_id = 221;

-- Check message count
SELECT COUNT(*) as MessageCount FROM support_messages;

-- Check unread messages
SELECT * FROM support_messages WHERE is_read = 0;

-- Check conversation with messages
SELECT 
    f.id,
    f.title,
    COUNT(m.id) as MessageCount,
    SUM(CASE WHEN m.is_read = 0 THEN 1 ELSE 0 END) as UnreadCount
FROM support_faq f
LEFT JOIN support_messages m ON f.id = m.conversation_id
WHERE f.user_id = 221
GROUP BY f.id, f.title;
```

---

## ✅ End-to-End Flow Verification

```
1. Start App → DbInitializer.InitializeAsync() runs
   ↓
2. Tables created (if not exist)
   ↓
3. 4 test conversations inserted for user 221
   ↓
4. 10 test messages inserted
   ↓
5. User logs in with session
   ↓
6. Navigate to /Agent/Messaging
   ↓
7. JavaScript calls GET /api/messaging/conversations/221
   ↓
8. MessagingController returns 4 conversations with unread counts
   ↓
9. UI renders conversation list
   ↓
10. Click conversation → calls GET /api/messaging/messages/{id}
    ↓
11. Messages display in thread
    ↓
12. User types message → clicks Send
    ↓
13. JavaScript calls POST /api/messaging/send
    ↓
14. Message saved to database
    ↓
15. Response returns new message
    ↓
16. UI updates to show message in thread
    ↓
✅ SUCCESS: Fetch and send messages working!
```

---

## 📝 Notes

- Database initializer runs **only once** - it checks if conversations already exist
- To reset test data: Delete rows from `support_messages` and `support_faq` or set up a reset endpoint
- User 221 (Arch) is hardcoded in `messaging.js` - to test with different users, change the `userId: 221` value
- Staff IDs 1-4 are used in test data - ensure these correspond to actual staff in your system

---

## 🎯 Success Criteria

You'll know everything is working when:
- ✅ 4 conversations appear on first load
- ✅ Clicking each shows different messages  
- ✅ Unread badges appear correctly
- ✅ Can type and send new messages
- ✅ Messages appear immediately in thread
- ✅ Database updates reflect new messages
- ✅ No JavaScript errors in console
- ✅ No API 404 or 500 errors

---

**You're all set! Start the application and begin testing.** 🚀
