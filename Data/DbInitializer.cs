using Microsoft.EntityFrameworkCore;
using NextHorizon.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NextHorizon.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Initialize the database with tables and test data for user 221 (Arch)
        /// Call this in Program.cs after building the app
        /// </summary>
        public static async Task InitializeAsync(AppDbContext context)
        {
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Check if support_faq table has data, if so, skip initialization
                if (await context.SupportFAQs.AnyAsync())
                {
                    return; // Database already initialized
                }

                // Create test conversations for user 221 (Arch)
                var conversations = new[]
                {
                    new SupportFAQ
                    {
                        UserId = 221,
                        Title = "GCash Payment Issue",
                        Description = "My GCash was charged P2,350 but order shows payment failed. I need this issue resolved immediately.",
                        Category = "Billing",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        UpdatedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    new SupportFAQ
                    {
                        UserId = 221,
                        Title = "Order Tracking Problem",
                        Description = "I cannot track my order #12345. The tracking system shows no updates since it was shipped.",
                        Category = "Orders",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new SupportFAQ
                    {
                        UserId = 221,
                        Title = "Product Quality Concern",
                        Description = "The product I received is damaged. The packaging was torn and the item has visible cracks.",
                        Category = "Returns",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        UpdatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new SupportFAQ
                    {
                        UserId = 221,
                        Title = "Delivery Delay",
                        Description = "My order was supposed to arrive on March 20 but it's now March 25. Where is my package?",
                        Category = "Shipping",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow.AddHours(-6),
                        UpdatedAt = DateTime.UtcNow.AddHours(-6)
                    }
                };

                await context.SupportFAQs.AddRangeAsync(conversations);
                await context.SaveChangesAsync();

                // Get the IDs of the newly created conversations
                var createdConversations = await context.SupportFAQs
                    .Where(c => c.UserId == 221)
                    .ToListAsync();

                // Create test messages for each conversation
                var messages = new[]
                {
                    // Messages for GCash Payment Issue (Conversation 1)
                    new SupportMessage
                    {
                        ConversationId = createdConversations[0].Id,
                        UserId = 221,
                        SenderStaffId = 1,
                        Message = "Hello Arch, I see your concern about the GCash charge. Let me check the payment records for you.",
                        SentAt = DateTime.UtcNow.AddDays(-3).AddHours(2),
                        IsRead = true,
                        MessageType = "text"
                    },
                    new SupportMessage
                    {
                        ConversationId = createdConversations[0].Id,
                        UserId = 221,
                        SenderStaffId = 0,
                        Message = "Yes, I need help. I was charged but didn't receive the order.",
                        SentAt = DateTime.UtcNow.AddDays(-3).AddHours(3),
                        IsRead = true,
                        MessageType = "text"
                    },
                    new SupportMessage
                    {
                        ConversationId = createdConversations[0].Id,
                        UserId = 221,
                        SenderStaffId = 1,
                        Message = "I found the issue. Your payment was processed but the order wasn't created. I'm creating a replacement order now with a 20% discount as compensation.",
                        SentAt = DateTime.UtcNow.AddDays(-3).AddHours(4),
                        IsRead = true,
                        MessageType = "text"
                    },

                    // Messages for Order Tracking Problem (Conversation 2)
                    new SupportMessage
                    {
                        ConversationId = createdConversations[1].Id,
                        UserId = 221,
                        SenderStaffId = 2,
                        Message = "Hi Arch, thank you for reaching out. I'll investigate your tracking issue right away.",
                        SentAt = DateTime.UtcNow.AddDays(-2).AddHours(1),
                        IsRead = true,
                        MessageType = "text"
                    },
                    new SupportMessage
                    {
                        ConversationId = createdConversations[1].Id,
                        UserId = 221,
                        SenderStaffId = 0,
                        Message = "The tracking number is TRK-2024-0012345. Can you help?",
                        SentAt = DateTime.UtcNow.AddDays(-2).AddHours(2),
                        IsRead = true,
                        MessageType = "text"
                    },
                    new SupportMessage
                    {
                        ConversationId = createdConversations[1].Id,
                        UserId = 221,
                        SenderStaffId = 2,
                        Message = "Found it! Your package is currently in transit and will be delivered tomorrow by 6 PM. The tracking system will update within 2 hours.",
                        SentAt = DateTime.UtcNow.AddDays(-2).AddHours(3),
                        IsRead = true,
                        MessageType = "text"
                    },

                    // Messages for Product Quality Concern (Conversation 3)
                    new SupportMessage
                    {
                        ConversationId = createdConversations[2].Id,
                        UserId = 221,
                        SenderStaffId = 3,
                        Message = "We sincerely apologize for the damaged product. Can you send us photos for documentation?",
                        SentAt = DateTime.UtcNow.AddDays(-1).AddHours(1),
                        IsRead = true,
                        MessageType = "text"
                    },
                    new SupportMessage
                    {
                        ConversationId = createdConversations[2].Id,
                        UserId = 221,
                        SenderStaffId = 0,
                        Message = "Yes, I'll upload the photos now. This is the second time this happened.",
                        SentAt = DateTime.UtcNow.AddDays(-1).AddHours(2),
                        IsRead = true,
                        MessageType = "text"
                    },
                    new SupportMessage
                    {
                        ConversationId = createdConversations[2].Id,
                        UserId = 221,
                        SenderStaffId = 3,
                        Message = "Thank you for the photos. We'll send a replacement immediately with priority shipping at no extra charge. Return label is attached.",
                        SentAt = DateTime.UtcNow.AddDays(-1).AddHours(3),
                        IsRead = false,
                        MessageType = "text"
                    },

                    // Messages for Delivery Delay (Conversation 4)
                    new SupportMessage
                    {
                        ConversationId = createdConversations[3].Id,
                        UserId = 221,
                        SenderStaffId = 4,
                        Message = "Hello Arch, I see your concern. Let me check the courier status for your package.",
                        SentAt = DateTime.UtcNow.AddHours(-5),
                        IsRead = false,
                        MessageType = "text"
                    },
                    new SupportMessage
                    {
                        ConversationId = createdConversations[3].Id,
                        UserId = 221,
                        SenderStaffId = 0,
                        Message = "Please help. I need this urgently for an event tomorrow.",
                        SentAt = DateTime.UtcNow.AddHours(-4),
                        IsRead = false,
                        MessageType = "text"
                    }
                };

                await context.SupportMessages.AddRangeAsync(messages);
                await context.SaveChangesAsync();

                Console.WriteLine("✅ Database initialized successfully with test data for user 221 (Arch)");
                Console.WriteLine($"   - Created 4 conversations");
                Console.WriteLine($"   - Created 10 test messages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing database: {ex.Message}");
                throw;
            }
        }
    }
}
