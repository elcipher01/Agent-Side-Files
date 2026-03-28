// Messaging API Integration
const MESSAGING_API = {
    baseUrl: '/api/messaging',
    userId: 221, // Arch's user ID

    // Get all conversations for the user
    async getConversations() {
        try {
            const response = await fetch(`${this.baseUrl}/conversations/${this.userId}`);
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching conversations:', error);
            return { success: false, message: error.message };
        }
    },

    // Get messages for a specific conversation
    async getMessages(conversationId) {
        try {
            const response = await fetch(`${this.baseUrl}/messages/${conversationId}`);
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching messages:', error);
            return { success: false, message: error.message };
        }
    },

    // Send a message to a conversation
    async sendMessage(conversationId, message, senderStaffId) {
        try {
            const response = await fetch(`${this.baseUrl}/send`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    conversationId: conversationId,
                    userId: this.userId,
                    senderStaffId: senderStaffId,
                    message: message,
                    messageType: 'text'
                })
            });
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error sending message:', error);
            return { success: false, message: error.message };
        }
    },

    // Create a new conversation
    async createConversation(title, description, category) {
        try {
            const response = await fetch(`${this.baseUrl}/create-conversation`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    userId: this.userId,
                    title: title,
                    description: description,
                    category: category
                })
            });
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error creating conversation:', error);
            return { success: false, message: error.message };
        }
    },

    // Get full conversation details
    async getConversationDetail(conversationId) {
        try {
            const response = await fetch(`${this.baseUrl}/conversation-detail/${conversationId}`);
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching conversation detail:', error);
            return { success: false, message: error.message };
        }
    }
};

// UI Management
class MessagingUI {
    constructor() {
        this.currentConversationId = null;
        this.staffId = parseInt(document.body.getAttribute('data-staff-id') || '1');
        this.init();
    }

    async init() {
        // Load conversations on page load
        await this.loadConversations();
        this.setupEventListeners();
    }

    async loadConversations() {
        console.log('Loading conversations for user 221...');
        const result = await MESSAGING_API.getConversations();
        
        if (result.success && result.data) {
            this.displayConversations(result.data);
        } else {
            console.log('No conversations found or error:', result.message);
        }
    }

    displayConversations(conversations) {
        console.log('Displaying conversations:', conversations);
        const container = document.getElementById('conversations-list') || document.getElementById('hc-list');
        
        if (!container) {
            console.log('Conversations container not found');
            return;
        }

        container.innerHTML = '';

        conversations.forEach((conv, index) => {
            const convElement = document.createElement('div');
            convElement.className = `conversation-item ${index === 0 ? 'active' : ''}`;
            convElement.onclick = () => this.selectConversation(conv.id);
            
            convElement.innerHTML = `
                <div style="padding: 12px; border-bottom: 1px solid #e0e0e0; cursor: pointer; hover:background:#f5f5f5;">
                    <h4 style="margin: 0 0 5px 0; font-size: 14px; font-weight: 600;">${conv.title}</h4>
                    <p style="margin: 0; font-size: 12px; color: #666; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">${conv.description}</p>
                    <small style="color: #999;">${new Date(conv.updatedAt).toLocaleDateString()}</small>
                    ${conv.unreadCount > 0 ? `<span style="background: red; color: white; border-radius: 10px; padding: 2px 6px; font-size: 11px; margin-left: 8px;">${conv.unreadCount}</span>` : ''}
                </div>
            `;
            
            container.appendChild(convElement);
        });

        // Select first conversation by default
        if (conversations.length > 0) {
            this.selectConversation(conversations[0].id);
        }
    }

    async selectConversation(conversationId) {
        this.currentConversationId = conversationId;
        console.log('Selected conversation:', conversationId);
        
        const result = await MESSAGING_API.getConversationDetail(conversationId);
        
        if (result.success) {
            this.displayMessages(result.data.messages, result.data.conversation);
        }
    }

    displayMessages(messages, conversation) {
        console.log('Displaying messages:', messages);
        const messagesContainer = document.getElementById('messages-container') || document.getElementById('hc-chat');
        
        if (!messagesContainer) {
            console.log('Messages container not found');
            return;
        }

        messagesContainer.innerHTML = `
            <div style="padding: 20px; border-bottom: 1px solid #e0e0e0; background: white;">
                <h3 style="margin: 0 0 5px 0;">${conversation.title}</h3>
                <p style="margin: 0; color: #666; font-size: 14px;">${conversation.description}</p>
            </div>
            <div id="messages-list" style="flex: 1; overflow-y: auto; padding: 20px; background: #f9f9f9;">
            </div>
            <div style="padding: 15px; background: white; border-top: 1px solid #e0e0e0;">
                <div style="display: flex; gap: 10px;">
                    <textarea id="message-input" placeholder="Type your message..." style="flex: 1; padding: 10px; border: 1px solid #ddd; border-radius: 5px; font-family: inherit; resize: none; height: 50px;"></textarea>
                    <button id="send-btn" style="padding: 10px 20px; background: #007bff; color: white; border: none; border-radius: 5px; cursor: pointer;">Send</button>
                </div>
            </div>
        `;

        const messagesList = document.getElementById('messages-list');
        messages.forEach(msg => {
            const msgElement = document.createElement('div');
            msgElement.style.cssText = `
                margin-bottom: 15px;
                padding: 10px 15px;
                border-radius: 8px;
                background: ${msg.userId === MESSAGING_API.userId ? '#e3f2fd' : '#f5f5f5'};
                text-align: ${msg.userId === MESSAGING_API.userId ? 'right' : 'left'};
            `;
            msgElement.innerHTML = `
                <p style="margin: 0; font-size: 14px;">${msg.message}</p>
                <small style="color: #999; font-size: 11px;">${new Date(msg.sentAt).toLocaleString()}</small>
            `;
            messagesList.appendChild(msgElement);
        });

        // Setup send button
        document.getElementById('send-btn').onclick = () => this.sendMessage();
        document.getElementById('message-input').addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && e.ctrlKey) {
                this.sendMessage();
            }
        });

        // Scroll to bottom
        messagesList.scrollTop = messagesList.scrollHeight;
    }

    async sendMessage() {
        const input = document.getElementById('message-input');
        const message = input.value.trim();
        
        if (!message) {
            alert('Please enter a message');
            return;
        }

        const result = await MESSAGING_API.sendMessage(
            this.currentConversationId,
            message,
            this.staffId
        );

        if (result.success) {
            input.value = '';
            // Reload messages to show the new one
            await this.selectConversation(this.currentConversationId);
        } else {
            alert('Error sending message: ' + result.message);
        }
    }

    setupEventListeners() {
        // Refresh button
        const refreshBtn = document.getElementById('refresh-conversations');
        if (refreshBtn) {
            refreshBtn.onclick = () => this.loadConversations();
        }

        // New conversation button
        const newConvBtn = document.getElementById('new-conversation');
        if (newConvBtn) {
            newConvBtn.onclick = () => this.showNewConversationDialog();
        }
    }

    async showNewConversationDialog() {
        const title = prompt('Enter conversation title:');
        if (!title) return;

        const description = prompt('Enter description (optional):');
        const category = prompt('Enter category (optional):');

        const result = await MESSAGING_API.createConversation(title, description || '', category || 'General');
        
        if (result.success) {
            alert('Conversation created successfully');
            this.loadConversations();
        } else {
            alert('Error creating conversation: ' + result.message);
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    console.log('Messaging system initializing...');
    new MessagingUI();
});
