namespace ClaimsBasedAuth.Api.Entities;

// Claim names used across the app. One place, no typos.
// Claim TYPES are case-sensitive strings - "Department" and "department" are different claims.
public static class AppClaimTypes
{
    public const string Department = "department";
    public const string Subscription = "subscription";
}
