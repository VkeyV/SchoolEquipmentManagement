namespace SchoolEquipmentManagement.Web.Security
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        public bool Enabled { get; set; }
        public string FromName { get; set; } = "School Equipment Management";
        public string FromAddress { get; set; } = string.Empty;
        public string SmtpHost { get; set; } = "smtp.yandex.ru";
        public int SmtpPort { get; set; } = 465;
        public bool UseSsl { get; set; } = true;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
