using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Templates.Core.Authentication.Maui.Storage;

/// <summary>
/// Persists the Keycloak token set using MAUI's platform SecureStorage.
/// On Android/iOS this maps to the OS keychain. On Windows it uses DPAPI.
///
/// All methods are thread-safe.
/// </summary>
public sealed class KeycloakTokenStorage : IKeycloakTokenStorage
{
	private const string StorageKey = "keycloak_token_set";
	private readonly SemaphoreSlim _lock = new(1, 1);

	public async Task<KeycloakTokenSet?> GetAsync(CancellationToken ct = default)
	{
		await _lock.WaitAsync(ct);
		try
		{
			var json = await SecureStorage.Default.GetAsync(StorageKey);
			if (string.IsNullOrEmpty(json)) return null;

			return JsonSerializer.Deserialize<KeycloakTokenSet>(json);
		}
		catch
		{
			// If SecureStorage fails (e.g. key removed, device re-enrolled) treat as logged out
			return null;
		}
		finally { _lock.Release(); }
	}

	public async Task SaveAsync(KeycloakTokenSet tokens, CancellationToken ct = default)
	{
		await _lock.WaitAsync(ct);
		try
		{
			var json = JsonSerializer.Serialize(tokens);
			await SecureStorage.Default.SetAsync(StorageKey, json);
		}
		finally { _lock.Release(); }
	}

	public async Task ClearAsync(CancellationToken ct = default)
	{
		await _lock.WaitAsync(ct);
		try
		{
			SecureStorage.Default.Remove(StorageKey);
			await Task.CompletedTask;
		}
		finally { _lock.Release(); }
	}
}