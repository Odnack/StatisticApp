namespace Api.Api.Notifications.Application
{
    public class ApplicationDto : ApplicationAddingDto
    {
        public int Id { get; set; }
        public int Views { get; set; }
    }
}