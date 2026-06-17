using System.Collections.Concurrent;

namespace ClaimsBasedAuth.Api.Services;

// Stands in for wherever subscription data really lives (billing DB, Stripe, etc.).
// The point: this data changes too often to be baked into a token.
public class SubscriptionStore
{
    private readonly ConcurrentDictionary<string, string> _tiers = new();

    public void SetTier(string userId, string tier) => _tiers[userId] = tier;

    public string GetTier(string userId) => _tiers.GetValueOrDefault(userId, "free");
}
