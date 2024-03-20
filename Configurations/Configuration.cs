namespace VideoGenerator.Configurations;

public class Configuration
{
    #region API_KEYS
    public string CHAT_API_KEY { get; set; }
    public string YOUTUBE_DOWNLOAD_API_KEY { get; set; }
    public string YOUTUBE_DATA_API_KEY { get; set; }
    public string TIKTOK_DOWNLOAD_API_KEY { get; set; }
    public string TIKTOK_DATA_API_KEY { get; set; }
    public string DETECT_LANGUAGE_API_KEY { get; set; }
    public string DEEPL_API_KEY { get; set; }
    public string OMDB_API_KEY { get; set; }
    public int TELEGRAM_API_ID { get; set; }
    public string TELEGRAM_API_HASH { get; set; }
    public string TELEGRAM_API_PHONE { get; set; }
    public string TMDB_API_KEY { get; set; }
    public string TMDB_BEARER_API_KEY { get; set; }
    public string KINOPOISK_API_KEY { get; set; }
    #endregion
    public long MAX_TELEGRAM_FILE_SIZE { get; set; }
    public string CONNECTION_STRING { get; set; }
    public string[] SOURCE_GROUPS { get; set; }
    public string[] GENERAL_TOPICS { get; set; }
    public int TELEGRAM_UPDATE_DELAY { get; set; }
    public int TELEGRAM_OPERATIONS_DELAY { get; set; }
    public string[] AVAILABLE_LANGUAGES { get; set; }
}
