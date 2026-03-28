-- ============================================================
-- Support FAQ and Messages Tables for User 221 (Arch)
-- ============================================================

-- Drop existing tables if they exist (optional)
-- DROP TABLE IF EXISTS support_messages;
-- DROP TABLE IF EXISTS support_faq;

-- Create support_faq table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'support_faq')
BEGIN
    CREATE TABLE support_faq (
        id INT PRIMARY KEY IDENTITY(1,1),
        user_id INT NOT NULL,
        title NVARCHAR(255) NOT NULL,
        description NVARCHAR(MAX),
        category NVARCHAR(100),
        status NVARCHAR(20) DEFAULT 'Active',
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_support_faq_user_id ON support_faq(user_id);
    CREATE INDEX IX_support_faq_status ON support_faq(status);
    CREATE INDEX IX_support_faq_created_at ON support_faq(created_at);
END

-- Create support_messages table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'support_messages')
BEGIN
    CREATE TABLE support_messages (
        id INT PRIMARY KEY IDENTITY(1,1),
        conversation_id INT NOT NULL,
        user_id INT NOT NULL,
        sender_staff_id INT NOT NULL,
        message NVARCHAR(MAX) NOT NULL,
        sent_at DATETIME DEFAULT GETDATE(),
        is_read BIT DEFAULT 0,
        message_type NVARCHAR(50) DEFAULT 'text',
        attachment_url NVARCHAR(500),
        FOREIGN KEY (conversation_id) REFERENCES support_faq(id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_support_messages_conversation_id ON support_messages(conversation_id);
    CREATE INDEX IX_support_messages_user_id ON support_messages(user_id);
    CREATE INDEX IX_support_messages_sender_staff_id ON support_messages(sender_staff_id);
    CREATE INDEX IX_support_messages_sent_at ON support_messages(sent_at);
END

-- ============================================================
-- Insert Test Data for User 221 (Arch)
-- ============================================================

-- Check if conversations already exist for user 221
IF NOT EXISTS (SELECT 1 FROM support_faq WHERE user_id = 221)
BEGIN
    -- Insert sample conversations for user 221 (Arch)
    INSERT INTO support_faq (user_id, title, description, category, status, created_at, updated_at)
    VALUES 
        (221, 'GCash Payment Issue', 'My GCash was charged P2,350 but the order shows payment failed. Please refund or confirm the order.', 'Billing', 'Active', GETDATE(), GETDATE()),
        (221, 'Order Tracking Problem', 'I cannot track my order #12345. The tracking number is not working.', 'Orders', 'Active', GETDATE(), GETDATE()),
        (221, 'Product Quality Concern', 'The product I received is damaged. Please help me with a replacement.', 'Returns', 'Active', GETDATE(), GETDATE()),
        (221, 'Delivery Delay', 'My order was supposed to arrive on March 20 but it hasnt arrived yet. Can you help?', 'Shipping', 'Active', GETDATE(), GETDATE());
    
    -- Get the IDs of the inserted conversations
    DECLARE @conv1 INT = (SELECT TOP 1 id FROM support_faq WHERE user_id = 221 AND title = 'GCash Payment Issue');
    DECLARE @conv2 INT = (SELECT TOP 1 id FROM support_faq WHERE user_id = 221 AND title = 'Order Tracking Problem');
    DECLARE @conv3 INT = (SELECT TOP 1 id FROM support_faq WHERE user_id = 221 AND title = 'Product Quality Concern');
    DECLARE @conv4 INT = (SELECT TOP 1 id FROM support_faq WHERE user_id = 221 AND title = 'Delivery Delay');
    
    -- Insert sample messages for conversation 1 (GCash Payment Issue)
    IF @conv1 IS NOT NULL
    BEGIN
        INSERT INTO support_messages (conversation_id, user_id, sender_staff_id, message, sent_at, is_read, message_type)
        VALUES 
            (@conv1, 221, 1, 'Hi Arch, I can see your GCash transaction of P2,350. Let me investigate this payment failure with our payments team.', DATEADD(HOUR, -2, GETDATE()), 1, 'text'),
            (@conv1, 221, 1, 'We have confirmed the payment was received but the order status wasnt updated. This has been fixed now. Please check your orders.', DATEADD(HOUR, -1, GETDATE()), 0, 'text'),
            (@conv1, 221, 1, 'Your refund of P2,350 has been processed and should appear in your GCash account within 24 hours.', GETDATE(), 0, 'text');
    END
    
    -- Insert sample messages for conversation 2 (Order Tracking)
    IF @conv2 IS NOT NULL
    BEGIN
        INSERT INTO support_messages (conversation_id, user_id, sender_staff_id, message, sent_at, is_read, message_type)
        VALUES 
            (@conv2, 221, 2, 'Hi Arch! Let me help you track your order. Can you please provide your order number?', DATEADD(HOUR, -3, GETDATE()), 1, 'text'),
            (@conv2, 221, 221, 'Order number is 12345', DATEADD(HOUR, -2.5, GETDATE()), 1, 'text'),
            (@conv2, 221, 2, 'Thank you! I found your order. The tracking number is XYZ789. You can track it here: [link]', DATEADD(HOUR, -2, GETDATE()), 0, 'text');
    END
    
    -- Insert sample messages for conversation 3 (Product Quality)
    IF @conv3 IS NOT NULL
    BEGIN
        INSERT INTO support_messages (conversation_id, user_id, sender_staff_id, message, sent_at, is_read, message_type)
        VALUES 
            (@conv3, 221, 3, 'Hi Arch, Im sorry to hear the product arrived damaged. Can you send us photos?', DATEADD(HOUR, -4, GETDATE()), 1, 'text'),
            (@conv3, 221, 221, 'Yes, I can send photos. How do I attach them?', DATEADD(HOUR, -3.5, GETDATE()), 1, 'text'),
            (@conv3, 221, 3, 'You can use the attachment feature in the chat. Well process a replacement once we receive the photos.', DATEADD(HOUR, -3, GETDATE()), 0, 'text');
    END
    
    -- Insert sample messages for conversation 4 (Delivery Delay)
    IF @conv4 IS NOT NULL
    BEGIN
        INSERT INTO support_messages (conversation_id, user_id, sender_staff_id, message, sent_at, is_read, message_type)
        VALUES 
            (@conv4, 221, 1, 'Hi Arch, I see your order is delayed. Let me check with our logistics partner.', DATEADD(HOUR, -5, GETDATE()), 1, 'text'),
            (@conv4, 221, 1, 'Your package is currently at the sorting facility and will be out for delivery tomorrow.', DATEADD(HOUR, -4, GETDATE()), 0, 'text'),
            (@conv4, 221, 1, 'You should receive it by March 25. Here is your updated tracking: [link]', DATEADD(HOUR, -3, GETDATE()), 0, 'text');
    END
    
    PRINT 'Test data for user 221 has been inserted successfully!';
END
ELSE
BEGIN
    PRINT 'Conversations for user 221 already exist.';
END

-- ============================================================
-- Verify the data was created
-- ============================================================
SELECT 'Conversations for User 221:' AS Info;
SELECT id, user_id, title, description, category, status, created_at, updated_at 
FROM support_faq 
WHERE user_id = 221;

SELECT 'Messages in support_messages:' AS Info;
SELECT id, conversation_id, user_id, sender_staff_id, message, sent_at, is_read
FROM support_messages 
ORDER BY sent_at ASC;
