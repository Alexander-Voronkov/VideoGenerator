﻿using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using VideoGenerator.Configurations;
using VideoGenerator.Exceptions;
using VideoGenerator.Extensions;
using VideoGenerator.Infrastructure;
using VideoGenerator.Models.FilmMetadata;
using VideoGenerator.Services.Interfaces;

namespace VideoGenerator.Services.Implementations;

public class FilmMetadataScraperService : IFilmMetadataScraperService
{
    private readonly ILogger _logger;
    private readonly IOptions<Configuration> _config;
    private readonly HttpClient _omdbClient;
    private readonly HttpClient _tmdbClient;
    private readonly HttpClient _kinopoiskClient;
    private readonly HttpClient _hdrezkaClient;
    private readonly ApplicationDbContext _context;
    private readonly HtmlDocument _doc;

    public FilmMetadataScraperService(
        ILogger<FilmMetadataScraperService> logger,
        IOptions<Configuration> config,
        IHttpClientFactory clientFactory,
        ApplicationDbContext context)
    {
        _logger = logger;
        _config = config;
        _omdbClient = clientFactory.CreateClient("OMDB");
        _tmdbClient = clientFactory.CreateClient("TMDB");
        _kinopoiskClient = clientFactory.CreateClient("KINOPOISK");
        _hdrezkaClient = clientFactory.CreateClient("HDREZKA");
        _context = context;
        _doc = new();
    }

    public async Task<FilmMetadata[]> GetPopularFilmsListAsync(CancellationToken token = default)
    {
        var films = await _omdbClient
            .GetFromJsonAsync<FilmMetadata[]>($"" +
            $"&");

        return films;
    }

    public async Task<FilmMetadata> GetFilmMetadataAsync(string filmName, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string[]> GetSearchResultsByFilmNameAsync(string filmName, CancellationToken token = default)
    {
        var result = await _hdrezkaClient.GetAsync($"https://rezka.cc/ajax_search" +
            $"?q={filmName}", token);

        if (!result.IsSuccessStatusCode)
        {
            throw new FilmDataRetrievingError($"Error while retrieving film \"{filmName}\" metadata from hdrezka.");
        }

        var content = await result.Content.ReadAsStreamAsync(token);
        _doc.Load(content);

        var searchboxContainer = _doc.DocumentNode.SelectSingleNode("//*[@id=\"search_results-wrapper\"]/div");
        var filmLinks = searchboxContainer.ChildNodes
            .Where(n => n.Depth == 1)
            .Select(n => n.Attributes["href"].Value)
            .ToArray();

        return filmLinks;
    }

    public async Task<DownloadLink[]> GetFilmDownloadLinksAsync(string filmLink, CancellationToken token = default)
    {
        var result = await _hdrezkaClient.GetAsync(filmLink);

        if (!result.IsSuccessStatusCode)
        {
            throw new FilmDataRetrievingError($"Error while retrieving download link for film by link \"{filmLink}\" from hdrezka.");
        }

        var content = await result.Content.ReadAsStreamAsync();
        _doc.Load(content);

        var downloadLinksContainer = _doc.DocumentNode
            .SelectSingleNode("//*[@id=\"main\"]/div/div[2]/div[3]/div[2]/ul/li/div[3]/div");
        var downloadLinksElements = downloadLinksContainer.ChildNodes
            .Where(dl => dl.Depth == 1)
            .Select(elem => new DownloadLink
            {
                Quality = elem.InnerText.ToFilmQuality(),
                TorrentDownloadUrl = elem.Attributes["href"].Value,
            })
            .ToArray();

        return downloadLinksElements;
    }
}