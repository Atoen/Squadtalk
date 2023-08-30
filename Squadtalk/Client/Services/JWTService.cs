using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RestSharp;
using Squadtalk.Client.Options;

namespace Squadtalk.Client.Services;

public delegate void TokenUpdatedHandler(JwtSecurityToken securityToken);

public sealed class JWTService
{
	private readonly JwtServiceOptions _options;
	private readonly RestClient _restClient;
	private const string EmptyToken = "token";

	private readonly JwtSecurityTokenHandler _tokenHandler = new();
	private CancellationTokenSource _cancellationTokenSource = new();

	public Action? UnableToRefreshToken;
	public Action<int>? RetryingToRefreshToken;
	private Task? _updateTask;

	public event TokenUpdatedHandler? TokenUpdated;

	public JWTService(IOptions<JwtServiceOptions> options, RestClient restClient)
	{
		_options = options.Value;
		_restClient = restClient;
	}

	public string Token { get; private set; } = EmptyToken;
	public JwtSecurityToken SecurityToken { get; private set; } = new();

	public void SetFirstToken(string token)
	{
		ArgumentException.ThrowIfNullOrEmpty(token);
		Token = token;

		_updateTask = TokenUpdateLoopAsync(true, _cancellationTokenSource.Token);
	}

	public bool IsTokenSet => Token != EmptyToken;

	public void ClearToken() => Token = EmptyToken;

	public async Task CancelPendingRequests()
	{
		_cancellationTokenSource.Cancel();

		if (_updateTask is not null)
		{
			await _updateTask;
		}

		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();
	}

	private async Task TokenUpdateLoopAsync(bool startWithDelay, CancellationToken cancellationToken)
	{
		try
		{
			if (startWithDelay)
			{
				SecurityToken = _tokenHandler.ReadJwtToken(Token);
				TokenUpdated?.Invoke(SecurityToken);
				
				await DelayNextRequest(cancellationToken);
			}
		
			while (!cancellationToken.IsCancellationRequested)
			{
				var token = await GetTokenAsync(cancellationToken);
				if (token is null) break;
		
				Token = token;
				SecurityToken = _tokenHandler.ReadJwtToken(token);
				TokenUpdated?.Invoke(SecurityToken);
				
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
		while (attempt < _options.RetryAttempts)
		{
			attempt++;

			var token = await RequestNewTokenAsync(cancellationToken);
			if (token is not null) return token;

			RetryingToRefreshToken?.Invoke(attempt);
			
			var delay = _options.RetryDelays[Math.Min(attempt, _options.RetryDelays.Length) - 1];
			await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
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