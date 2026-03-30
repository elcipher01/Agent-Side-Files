namespace NextHorizon.Models
{
    public class SupportConversation
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public int AgentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}