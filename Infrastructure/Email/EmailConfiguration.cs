namespace Infrastructure.Email;

public class EmailConfiguration
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "School Management";
    public bool UseSsl { get; set; } = true;
    public int SmtpTimeoutMilliseconds { get; set; } = 10000;

    /// <summary>Public URL of the frontend, used to build login and reset links.</summary>
    public string AppBaseUrl { get; set; } = "https://campuserpt.vercel.app";

    /// <summary>Reply-to / support mailbox shown in email templates.</summary>
    public string SupportEmail { get; set; } = "support@campuserp.com";
}
