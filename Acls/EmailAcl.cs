using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

public class EmailAcl : IDisposable
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailAcl(IConfiguration configuration)
    {
        // Aqui lemos as configs diretamente da configuração (ex: .env ou appsettings)
        _smtpHost = configuration["SMTP_HOST"] ?? throw new ArgumentNullException("SMTP_HOST");
        _smtpPort = int.Parse(configuration["SMTP_PORT"] ?? "587");
        _smtpUser = configuration["SMTP_USERNAME"] ?? throw new ArgumentNullException("SMTP_USERNAME");
        _smtpPassword = configuration["SMTP_PASSWORD"] ?? throw new ArgumentNullException("SMTP_PASSWORD");
        _fromEmail = configuration["SMTP_EMAIL_FROM"] ?? throw new ArgumentNullException("SMTP_EMAIL_FROM");
        _fromName = configuration["SMTP_EMAIL_NAME"] ?? "SteamStats";
    }

    public async Task<(bool Success, string ErrorMessage)> SendPromotionEmail(string toEmail, string gameName, int discount, int appid)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Um jogo da sua Wish List entrou de promoção!";

        var bodyBuilder = new BodyBuilder
        {
            TextBody = $"O jogo \"{gameName}\" entrou em promoção com desconto de {discount}%. Aproveite!",
            HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                <h2 style='color: #333;'>🎮 Promoção na sua Lista de Desejos!</h2>
                <p style='font-size: 16px; color: #555;'>O jogo <strong>{gameName}</strong> está com um super desconto de <strong style='color: #28a745;'>{discount}%</strong>!</p>
                <p style='font-size: 15px; color: #555;'>Aproveite a oferta antes que acabe!</p>
                <a href='https://store.steampowered.com/app/{appid}' target='_blank' style='display: inline-block; margin-top: 20px; padding: 10px 15px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Ver jogo na Steam</a>
                <p style='font-size: 12px; color: #999; margin-top: 30px;'>Você está recebendo este e-mail porque adicionou este jogo à sua lista de desejos no Steam Stats.</p>
            </div>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpUser, _smtpPassword);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao enviar e-mail: {ex.Message}");
        }
    }

    public void Dispose()
    {
        // Nada a liberar
    }
}
