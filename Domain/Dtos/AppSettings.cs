namespace Domain.Dtos;
public class APISettings
{
    #region Properties
    public string Version { get; set; }
    public string AppName { get; set; }
    public string DateFormat { get; set; }
    public string DateTimeFormat { get; set; }
    public string TimeFormat { get; set; }
    public string SitePath { get; set; }
    public string ContactUs { get; set; }
    public string PrivacyAndPolicy { get; set; }
    public string TermsAndConditions { get; set; }
    public string About { get; set; }
    public string EmailSupport { get; set; }
    public EmailSetting EmailSetting { get; set; }
    #endregion
}
public class AppSettings
{
    #region Properties
    public string Version { get; set; }
    public string AppName { get; set; }
    public string DateFormat { get; set; }
    public string DateTimeFormat { get; set; }
    public string TimeFormat { get; set; }
    public string APIPath { get; set; }
    public string SitePath { get; set; }
    public string ContactUs { get; set; }
    public string PrivacyAndPolicy { get; set; }
    public string TermsAndConditions { get; set; }
    public string About { get; set; }
    public string EmailSupport { get; set; }
    #endregion
}
public class EmailSetting
{
    #region Properties
    public string Host { get; set; }
    public string Port { get; set; }
    public string EnableSsl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string From { get; set; }
    public string CC { get; set; }

    #endregion
}
