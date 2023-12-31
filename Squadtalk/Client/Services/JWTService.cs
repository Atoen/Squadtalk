﻿using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RestSharp;
using Squadtalk.Client.Options;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public delegate void TokenUpdatedHandler(JwtSecurityToken securityToken);

public delegate Task TokenUpdatedHandlerAsync(string jwt);

public sealed class JwtService
{
    private const string EmptyToken = "token";
    private readonly JwtServiceOptions _options;
    private readonly RestClient _restClient;

    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    private Task? _updateTask;
    public Action<int>? RetryingToRefreshToken;

    public Action? UnableToRefreshToken;

    public JwtService(IOptions<JwtServiceOptions> options, RestClient restClient)
    {
        _options = options.Value;
        _restClient = restClient;
    }

    public string Token { get; private set; } = EmptyToken;
    public JwtSecurityToken SecurityToken { get; private set; } = new();

    public string Username { get; private set; } = string.Empty;
    public Guid Id { get; private set; }

    public bool IsTokenSet => Token != EmptyToken;

    public event TokenUpdatedHandler? TokenUpdated;
    public event TokenUpdatedHandlerAsync? TokenUpdatedAsync;

    public void SetFirstToken(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        Token = token;

        _updateTask = TokenUpdateLoopAsync(true, _cancellationTokenSource.Token);
    }

    public void ClearToken()
    {
        Token = EmptyToken;
    }

    public async Task CancelPendingRequests()
    {
        _cancellationTokenSource.Cancel();

        if (_updateTask is not null) await _updateTask;

        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private async Task UpdateToken(string token)
    {
        SecurityToken = _tokenHandler.ReadJwtToken(token);

        Username = SecurityToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value!;
        Id = Guid.Parse(SecurityToken.Claims.FirstOrDefault(x => x.Type == JwtClaims.Uid)?.Value!);

        TokenUpdated?.Invoke(SecurityToken);

        if (TokenUpdatedAsync is not null) await TokenUpdatedAsync.Invoke(token);
    }

    private async Task TokenUpdateLoopAsync(bool startWithDelay, CancellationToken cancellationToken)
    {
        try
        {
            if (startWithDelay)
            {
                await UpdateToken(Token);
                await DelayNextRequest(cancellationToken);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var token = await GetTokenAsync(cancellationToken);
                if (token is null) break;

                Token = token;
                await UpdateToken(token);

                await DelayNextRequest(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }

        UnableToRefreshToken?.Invoke();
    }

    private async Task DelayNextRequest(CancellationToken cancellationToken)
    {
        var lifetime = SecurityToken.ValidTo.ToLocalTime() - DateTime.Now;
        var requestDelay = lifetime * 0.8;

        await Task.Delay(requestDelay, cancellationToken);
    }

    private async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
    {
        var attempt = 0;
        while (attempt < _options.RetryDelays.Length)
        {
            var token = await RequestNewTokenAsync(cancellationToken);
            if (token is not null) return token;

            RetryingToRefreshToken?.Invoke(attempt);

            var delay = _options.RetryDelays[attempt];
            await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);

            attempt++;
        }

        return null;
    }

    private async Task<string?> RequestNewTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var request = new RestRequest("api/user/refresh-token", Method.Post)
                .AddHeader("Authorization", $"Bearer {Token}");

            var response = await _restClient.ExecuteAsync(request, cancellationToken);

            return response.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<string>(response.Content!)
                : null;
        }
        catch
        {
            return null;
        }
    }
}