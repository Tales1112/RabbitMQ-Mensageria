using System;

namespace RabbitMQ_Mensageria.Models
{
    public class SendMessageModel
    {
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
