using System.Collections.Concurrent;

namespace PolicyBasedAuth.Api.Services;

// Stands in for wherever subscription data really lives (billing DB, Stripe, etc.).
public class SubscriptionStore
{
    private readonly ConcurrentDictionary<string, string> _tiers = new();

    public void SetTier(string userId, string tier) => _tiers[userId] = tier;

    public string GetTier(string userId) => _tiers.GetValueOrDefault(userId, "free");
}
