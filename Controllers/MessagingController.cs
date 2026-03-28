using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextHorizon.Models;
using NextHorizon.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextHorizon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthenticationFilter]
    public class MessagingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessagingController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all conversations for a specific user (e.g., userId = 221)
        /// Returns all SupportFAQ records where UserId matches the provided user
        /// </summary>
        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetConversations(int userId)
        {
            try
            {
                var conversations = await _context.SupportFAQs
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.UpdatedAt)
                    .Select(s => new
                    {
                        s.Id,
                        s.UserId,
                        s.Title,
                        s.Description,
                        s.Category,
                        s.Status,
                        s.CreatedAt,
                        s.UpdatedAt,
                        UnreadCount = _context.SupportMessages
                            .Where(m => m.ConversationId == s.Id && !m.IsRead)
                            .Count()
                    })
                    .ToListAsync();

                if (!conversations.Any())
                {
                    return Ok(new { success = false, message = "No conversations found for this user", data = new List<object>() });
                }

                return Ok(new { success = true, message = "Conversations retrieved successfully", data = conversations });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all messages for a specific conversation
        /// </summary>
        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            try
            {
                // Verify conversation exists
                var conversation = await _context.SupportFAQs.FindAsync(conversationId);
                if (conversation == null)
                {
                    return NotFound(new { success = false, message = "Conversation not found" });
                }

                // Get all messages for this conversation
                var messages = await _context.SupportMessages
                    .Where(m => m.ConversationId == conversationId)
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        m.Id,
                        m.ConversationId,
                        m.UserId,
                        m.SenderStaffId,
                        m.Message,
                        m.SentAt,
                        m.IsRead,
                        m.MessageType,
                        m.AttachmentUrl
                    })
                    .ToListAsync();

                // Mark messages as read
                var unreadMessages = _context.SupportMessages
                    .Where(m => m.ConversationId == conversationId && !m.IsRead);
                
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Messages retrieved successfully", data = messages });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Send a new message to a conversation
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { success = false, message = "Message cannot be empty" });
                }

                // Verify conversation exists
                var conversation = await _context.SupportFAQs.FindAsync(request.ConversationId);
                if (conversation == null)
                {
                    return NotFound(new { success = false, message = "Conversation not found" });
                }

                // Create new message
                var supportMessage = new SupportMessage
                {
                    ConversationId = request.ConversationId,
                    UserId = request.UserId,
                    SenderStaffId = request.SenderStaffId,
                    Message = request.Message,
                    SentAt = DateTime.UtcNow,
                    IsRead = false,
                    MessageType = request.MessageType ?? "text",
                    AttachmentUrl = request.AttachmentUrl ?? string.Empty
                };

                _context.SupportMessages.Add(supportMessage);

                // Update conversation UpdatedAt
                conversation.UpdatedAt = DateTime.UtcNow;
                _context.SupportFAQs.Update(conversation);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Message sent successfully",
                    data = new
                    {
                        supportMessage.Id,
                        supportMessage.ConversationId,
                        supportMessage.UserId,
                        supportMessage.SenderStaffId,
                        supportMessage.Message,
                        supportMessage.SentAt,
                        supportMessage.MessageType
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new conversation for a user
        /// </summary>
        [HttpPost("create-conversation")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { success = false, message = "Title is required" });
                }

                var conversation = new SupportFAQ
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Description = request.Description ?? string.Empty,
                    Category = request.Category ?? "General",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SupportFAQs.Add(conversation);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Conversation created successfully",
                    data = new
                    {
                        conversation.Id,
                        conversation.UserId,
                        conversation.Title,
                        conversation.Status,
                        conversation.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get conversation details with latest messages
        /// </summary>
        [HttpGet("conversation-detail/{conversationId}")]
        public async Task<IActionResult> GetConversationDetail(int conversationId)
        {
            try
            {
                var conversation = await _context.SupportFAQs.FindAsync(conversationId);
                if (conversation == null)
                {
                    return NotFound(new { success = false, message = "Conversation not found" });
                }

                var messages = await _context.SupportMessages
                    .Where(m => m.ConversationId == conversationId)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Conversation detail retrieved successfully",
                    data = new
                    {
                        conversation = new
                        {
                            conversation.Id,
                            conversation.UserId,
                            conversation.Title,
                            conversation.Description,
                            conversation.Category,
                            conversation.Status,
                            conversation.CreatedAt,
                            conversation.UpdatedAt,
                            MessageCount = messages.Count,
                            UnreadCount = messages.Count(m => !m.IsRead),
                            LastMessage = messages.LastOrDefault()?.Message,
                            LastMessageTime = messages.LastOrDefault()?.SentAt
                        },
                        messages = messages.Select(m => new
                        {
                            m.Id,
                            m.ConversationId,
                            m.UserId,
                            m.SenderStaffId,
                            m.Message,
                            m.SentAt,
                            m.IsRead,
                            m.MessageType,
                            m.AttachmentUrl
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    // Request Models
    public class SendMessageRequest
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public int SenderStaffId { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
        public string AttachmentUrl { get; set; }
    }

    public class CreateConversationRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
}
