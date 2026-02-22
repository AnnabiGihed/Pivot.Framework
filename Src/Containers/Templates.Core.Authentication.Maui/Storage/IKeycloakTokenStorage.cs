namespace Templates.Core.Authentication.Maui.Storage;

/// <summary>Contract for the token storage layer.</summary>
public interface IKeycloakTokenStorage
{
	Task<KeycloakTokenSet?> GetAsync(CancellationToken ct = default);
	Task SaveAsync(KeycloakTokenSet tokens, CancellationToken ct = default);
	Task ClearAsync(CancellationToken ct = default);
}