namespace RabbitMQ_Mensageria.Services
{
    public interface INotificationService
    {
        void NotifyUser(string FromUser, string ToUser, string Content);
    }
}