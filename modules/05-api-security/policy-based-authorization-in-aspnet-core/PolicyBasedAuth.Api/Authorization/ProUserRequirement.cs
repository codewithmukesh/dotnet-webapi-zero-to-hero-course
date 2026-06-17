using Microsoft.AspNetCore.Authorization;

namespace PolicyBasedAuth.Api.Authorization;

// A requirement is just a marker carrying data (none needed here).
// The LOGIC lives in handlers - and one requirement can have several.
public class ProUserRequirement : IAuthorizationRequirement;
