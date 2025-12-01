namespace ProjetoFinal.Services
{
    public interface IEmailService
    {
        //Melhoria: criar Dto para o email
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
