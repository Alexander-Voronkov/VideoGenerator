using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using PexelsDotNetSDK.Api;
using PexelsDotNetSDK.Models;

namespace VideoGenerator.Helpers;

public class PexelsClient
{
    private static HttpClient client;

    private static string _token = "";

    private static string _baseAddress = "https://api.pexels.com/";

    private static string _apiVersion = "v1/";

    private static int _timeoutSecs = 30;

    private static string _version = "1.0.11";

    private bool isValidColor(string color)
    {
        Regex regex = new Regex("^#(?:[0-9a-fA-F]{3}){1,2}$");
        if (!BaseConstants.COLORS.Contains(color.ToLower()))
        {
            return regex.IsMatch(color);
        }

        return true;
    }

    private bool isValidSize(string size)
    {
        return BaseConstants.SIZES.Contains(size.ToLower());
    }

    private bool isValidOrientation(string orientation)
    {
        return BaseConstants.ORIENTATIONS.Contains(orientation.ToLower());
    }

    public PexelsClient(string token)
    {
        _token = token;
        if (client == null)
        {
            CreateClient();
        }

        SetupClientAuthHeader(client);
    }

    private HttpClient CreateClient()
    {
        client = new HttpClient();
        SetupClientDefaults(client);
        return client;
    }

    protected virtual void SetupClientDefaults(HttpClient client)
    {
        client.Timeout = TimeSpan.FromSeconds((double)_timeoutSecs);
        client.BaseAddress = new Uri(_baseAddress);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "Pexels/.NET (" + _version + ")");
    }

    protected virtual void SetupClientAuthHeader(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Add("Authorization", _token);
    }

    //
    // Summary:
    //     This endpoint enables you to search Pexels for any topic that you would like.
    //     For example your query could be something broad like 'Nature', 'Tigers', 'People'.
    //     Or it could be something specific like 'Group of people working'.
    //
    // Parameters:
    //   query:
    //     The search query. Ocean, Tigers, Pears, etc.
    //
    //   orientation:
    //     Desired photo orientation. The current supported orientations are: landscape,
    //     portrait or square
    //
    //   size:
    //     Minimum photo size. The current supported sizes are: large(24MP), medium(12MP)
    //     or small(4MP)
    //
    //   color:
    //     Desired photo color. Supported colors: red, orange, yellow, green, turquoise,
    //     blue, violet, pink, brown, black, gray, white or any hexidecimal color code (eg.
    //     #ffffff)
    //
    //   locale:
    //     The locale of the search you are performing. The current supported locales are:
    //     'en-US' 'pt-BR' 'es-ES' 'ca-ES' 'de-DE' 'it-IT' 'fr-FR' 'sv-SE' 'id-ID' 'pl-PL'
    //     'ja-JP' 'zh-TW' 'zh-CN' 'ko-KR' 'th-TH' 'nl-NL' 'hu-HU' 'vi-VN' 'cs-CZ' 'da-DK'
    //     'fi-FI' 'uk-UA' 'el-GR' 'ro-RO' 'nb-NO' 'sk-SK' 'tr-TR' 'ru-RU'.
    //
    //   page:
    //     The number of the page you are requesting. Default: 1
    //
    //   pageSize:
    //     The number of results you are requesting per page. Default: 15 Max: 80
    public async Task<PhotoPage> SearchPhotosAsync(string query, string orientation = "", string size = "", string color = "", string locale = "", int page = 1, int pageSize = 15)
    {
        if (pageSize > 80)
        {
            pageSize = 80;
        }

        if (pageSize <= 0)
        {
            pageSize = 1;
        }

        if (page <= 0)
        {
            page = 1;
        }

        string text = $"{_apiVersion}search?query={Uri.EscapeDataString(query)}&page={page}&per_page={pageSize}";
        if (!string.IsNullOrEmpty(locale))
        {
            text = text + "&locale=" + locale;
        }

        if (!string.IsNullOrEmpty(orientation) && isValidOrientation(orientation))
        {
            text = text + "&orientation=" + orientation;
        }

        if (!string.IsNullOrEmpty(size) && isValidSize(size))
        {
            text = text + "&size=" + size;
        }

        if (!string.IsNullOrEmpty(color) && isValidColor(color))
        {
            text = text + "&color=" + HttpUtility.UrlEncode(color);
        }

        HttpResponseMessage response = await client.GetAsync(text);
        PhotoPage obj = await ProcessResult<PhotoPage>(response);
        obj.rateLimit = ProcessRateLimits(response);
        return obj;
    }

    //
    // Summary:
    //     This endpoint allows you to get all the collections you've created on Pexels.
    //
    //
    // Parameters:
    //   page:
    //     The number of the page you are requesting. Default: 1
    //
    //   pageSize:
    //     The number of results you are requesting per page. Default: 15 Max: 80
    public async Task<CollectionPage> CollectionsAsync(int page = 1, int pageSize = 15)
    {
        if (pageSize > 80)
        {
            pageSize = 80;
        }

        if (pageSize <= 0)
        {
            pageSize = 1;
        }

        if (page <= 0)
        {
            page = 1;
        }

        string requestUri = $"{_apiVersion}collections?page={page}&per_page={pageSize}";
        HttpResponseMessage response = await client.GetAsync(requestUri);
        CollectionPage obj = await ProcessResult<CollectionPage>(response);
        obj.rateLimit = ProcessRateLimits(response);
        return obj;
    }

    //
    // Summary:
    //     This endpoint returns a list of featured collections
    //
    // Parameters:
    //   page:
    //     The number of the page you are requesting. Default: 1
    //
    //   pageSize:
    //     The number of results you are requesting per page. Default: 15 Max: 80
    public async Task<CollectionPage> FeaturedCollectionsAsync(int page = 1, int pageSize = 15)
    {
        if (pageSize > 80)
        {
            pageSize = 80;
        }

        if (pageSize <= 0)
        {
            pageSize = 1;
        }

        if (page <= 0)
        {
            page = 1;
        }

        string requestUri = $"{_apiVersion}collections/featured?page={page}&per_page={pageSize}";
        HttpResponseMessage response = await client.GetAsync(requestUri);
        CollectionPage obj = await ProcessResult<CollectionPage>(response);
        obj.rateLimit = ProcessRateLimits(response);
        return obj;
    }

    //
    // Summary:
    //     This endpoint gets the media of a specific collection.
    //
    // Parameters:
    //   id:
    //     The Collection Id
    //
    //   page:
    //     The number of the page you are requesting. Default: 1
    //
    //   pageSize:
    //     The number of results you are requesting per page. Default: 15 Max: 80
    //
    //   type:
    //     Filter results to a specific media type (`videos` or `photos`). Leave blank for
    //     all results.
    public async Task<CollectionMediaPage> GetCollectionAsync(string id, int page = 1, int pageSize = 15, string type = null)
    {
        if (pageSize > 80)
        {
            pageSize = 80;
        }

        if (pageSize <= 0)
        {
            pageSize = 1;
        }

        if (page <= 0)
        {
            page = 1;
        }

        string requestUri = $"{_apiVersion}collections/{id}?page={page}&per_page={pageSize}&type={type}";
        HttpResponseMessage response = await client.GetAsync(requestUri);
        CollectionMediaPage obj = await ProcessResult<CollectionMediaPage>(response);
        obj.rateLimit = ProcessRateLimits(response);
        return obj;
    }

    //
    // Summary:
    //     This endpoint enables you to receive real-time photos curated by the Pexels team.
    //     We add at least one new photo per hour to our curated list so that you always
    //     get a changing selection of trending photos.
    //
    // Parameters:
    //   page:
    //     The number of the page you are requesting. Default: 1
    //
    //   pageSize:
    //     The number of results you are requesting per page. Default: 15 Max: 80
    public async Task<PhotoPage> CuratedPhotosAsync(int page = 1, int pageSize = 15)
    {
        if (pageSize > 80)
        {
            pageSize = 80;
        }

        if (pageSize <= 0)
        {
            pageSize = 1;
        }

        if (page <= 0)
        {
            page = 1;
        }

        string requestUri = $"{_apiVersion}curated?page={page}&per_page={pageSize}";
        HttpResponseMessage response = await client.GetAsync(requestUri);
        PhotoPage obj = await ProcessResult<PhotoPage>(response);
        obj.rateLimit = ProcessRateLimits(response);
        return obj;
    }

    //
    // Summary:
    //     Retrieve a specific Photo from its id.
    //
    // Parameters:
    //   id:
    //     The id of the photo you are requesting.
    public async Task<Photo> GetPhotoAsync(int id)
    {
        string requestUri = $"{_apiVersion}photos/{id}";
        return await ProcessResult<Photo>(await client.GetAsync(requestUri));
    }

    //
    // Summary:
    //     This endpoint enables you to search Pexels for any topic that you would like.
    //     For example your query could be something broad like 'Nature', 'Tigers', 'People'.
    //     Or it could be something specific like 'Group of people working'.
    //
    // Parameters:
    //   query:
    //     The search query. Ocean, Tigers, Pears, etc.
    //
    //   orientation:
    //     Desired photo orientation. The current supported orientations are: landscape,
    //     portrait or square
    //
    //   size:
    //     Minimum photo size. The current supported sizes are: large(24MP), medium(12MP)
    //     or small(4MP)
    //
    //   locale:
    //     The locale of the search you are performing. The current supported locales are:
    //     'en-US' 'pt-BR' 'es-ES' 'ca-ES' 'de-DE' 'it-IT' 'fr-FR' 'sv-SE' 'id-ID' 'pl-PL'
    //     'ja-JP' 'zh-TW' 'zh-CN' 'ko-KR' 'th-TH' 'nl-NL' 'hu-HU' 'vi-VN' 'cs-CZ' 'da-DK'
    //     'fi-FI' 'uk-UA' 'el-GR' 'ro-RO' 'nb-NO' 'sk-SK' 'tr-TR' 'ru-RU'.
    //
    //   page:
    //     The number of the page you are requesting. Default: 1
    //
    //   pageSize:
    //     The number of results you are requesting per page. Default: 15 Max: 80

    public class MyVideoFile
    {
        [JsonProperty("id")]
        public long id { get; set; }

        [JsonProperty("quality")]
        public string quality { get; set; }

        [JsonProperty("file_type")]
        public string fileType { get; set; }

        [JsonProperty("width")]
        public int? width { get; set; }

        [JsonProperty("height")]
        public int? height { get; set; }

        [JsonProperty("fps")]
        public double? fps { get; set; }

        [JsonProperty("link")]
        public string link { get; set; }
    }

    public class MyVideoPicture
    {
        [JsonProperty("id")]
        public long id { get; set; }

        [JsonProperty("picture")]
        public string picture { get; set; }

        [JsonProperty("nr")]
        public int nr { get; set; }
    }

    public class MyVideo
    {
        [JsonProperty("id")]
        public long id { get; set; }

        [JsonProperty("width")]
        public int width { get; set; }

        [JsonProperty("height")]
        public int height { get; set; }

        [JsonProperty("url")]
        public string url { get; set; }

        [JsonProperty("image")]
        public string image { get; set; }

        [JsonProperty("duration")]
        public int duration { get; set; }

        [JsonProperty("user")]
        public MyUser user { get; set; }

        [JsonProperty("video_files")]
        public IEnumerable<MyVideoFile> videoFiles { get; set; }

        [JsonProperty("video_pictures")]
        public IEnumerable<MyVideoPicture> videoPictures { get; set; }
    }

    public class MyUser
    {
        [JsonProperty("id")]
        public long id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("url")]
        public string url { get; set; }
    }

    public class MyVideoPage : Page
    {
        [JsonProperty("videos")]
        public IEnumerable<MyVideo> videos { get; set; }
    }

    public async Task<MyVideoPage> SearchVideosAsync(string query, string orientation = "", string size = "", string locale = "", int page = 1, int pageSize = 15, int minimalSeconds = 10)
    {
        if (pageSize > 80)
        {
            pageSize = 80;
        }

        if (pageSize <= 0)
        {
            pageSize = 1;
        }

        if (page <= 0)
        {
            page = 1;
        }

        string text = $"videos/search?query={Uri.EscapeDataString(query)}&page={page}&per_page={pageSize}";
        if (!string.IsNullOrEmpty(locale))
        {
            text = text + "&locale=" + locale;
        }

        if (!string.IsNullOrEmpty(orientation) && isValidOrientation(orientation))
        {
            text = text + "&orientation=" + orientation;
        }

        if (!string.IsNullOrEmpty(size) && isValidSize(size))
        {
            text = text + "&size=" + size;
        }

        if (minimalSeconds > 0)
        {
            text += $"&min_duration={minimalSeconds}";
        }

        HttpResponseMessage response = await client.GetAsync(text);
        MyVideoPage obj = await ProcessResult<MyVideoPage>(response);
        obj.rateLimit = ProcessRateLimits(response);
        return obj;
    }

    //
    // Summary:
    //     This endpoint enables you to receive the current popular Pexels videos.
    //
    // Parameters:
    //   page:
    //     The number of the page you are requesting. Default: 1
    //
    //   pageSize:
    //     The number of results you are requesting per page. Default: 15 Max: 80
    //
    //   minWidth:
    //     The minimum width in pixels of the returned videos.
    //
    //   minHeight:
    //     The minimum height in pixels of the returned videos.
    //
    //   minDurationSecs:
    //     The minimum duration in seconds of the returned videos.
    //
    //   maxDurationSecs:
    //     The maximum duration in seconds of the returned videos.
    public async Task<VideoPage> PopularVideosAsync(int page = 1, int pageSize = 15, int minWidth = 0, int minHeight = 0, int minDurationSecs = 0, int maxDurationSecs = 0)
    {
        if (pageSize > 80)
        {
            pageSize = 80;
        }

        if (pageSize <= 0)
        {
            pageSize = 1;
        }

        if (page <= 0)
        {
            page = 1;
        }

        string text = $"videos/popular?page={page}&per_page={pageSize}";
        if (minWidth > 0)
        {
            text += $"&min_width={minWidth}";
        }

        if (minHeight > 0)
        {
            text += $"&min_height={minHeight}";
        }

        if (minDurationSecs > 0)
        {
            text += $"&min_duration={minDurationSecs}";
        }

        if (maxDurationSecs > 0)
        {
            text += $"&max_duration={maxDurationSecs}";
        }

        HttpResponseMessage response = await client.GetAsync(text);
        VideoPage obj = await ProcessResult<VideoPage>(response);
        obj.rateLimit = ProcessRateLimits(response);
        return obj;
    }

    //
    // Summary:
    //     Retrieve a specific Video from its id.
    //
    // Parameters:
    //   id:
    //     The id of the video you are requesting.
    public async Task<Video> GetVideoAsync(int id)
    {
        string requestUri = $"videos/videos/{id}";
        return await ProcessResult<Video>(await client.GetAsync(requestUri));
    }

    private async Task<T> ProcessResult<T>(HttpResponseMessage response)
    {
        string text = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }

        throw new ErrorResponse(response.StatusCode, text);
    }

    private RateLimit ProcessRateLimits(HttpResponseMessage response)
    {
        try
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            IEnumerable<string> values;
            string text = (response.Headers.TryGetValues("X-Ratelimit-Reset", out values) ? values.FirstOrDefault() : null);
            IEnumerable<string> values2;
            string value = (response.Headers.TryGetValues("X-Ratelimit-Limit", out values2) ? values2.FirstOrDefault() : "0");
            IEnumerable<string> values3;
            string value2 = (response.Headers.TryGetValues("X-Ratelimit-Remaining", out values3) ? values3.FirstOrDefault() : "0");
            return new RateLimit
            {
                Limit = Convert.ToInt64(value),
                Remaining = Convert.ToInt64(value2),
                Reset = ((text != null) ? dateTime.AddMilliseconds(Convert.ToInt64(text)).ToLocalTime() : dateTime.ToLocalTime())
            };
        }
        catch (Exception)
        {
        }

        return null;
    }
}
