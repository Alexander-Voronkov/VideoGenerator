namespace VideoGenerator.Configurations;

public class Configuration
{
    public string CHAT_API_KEY { get; set; }
    public string YOUTUBE_DOWNLOAD_API_KEY { get; set; }
    public string YOUTUBE_DATA_API_KEY { get; set; }
    public string TIKTOK_DOWNLOAD_API_KEY { get; set; }
    public string TIKTOK_DATA_API_KEY { get; set; }
    public long MAX_TELEGRAM_FILE_SIZE { get; set; }
    public string CONNECTION_STRING { get; set; }
    public int TELEGRAM_API_ID { get; set; }
    public string TELEGRAM_API_HASH { get; set; }
    public string TELEGRAM_API_PHONE { get; set; }
    public string DETECT_LANGUAGE_API_KEY { get; set; }
    public string DEEPL_API_KEY { get; set; }
    public string[] SOURCE_GROUPS { get; set; }
    public string[] GENERAL_TOPICS { get; set; }
}
