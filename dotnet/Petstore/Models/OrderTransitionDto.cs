namespace Petstore.Models;

public sealed record OrderTransitionDto(
    string FromStatus,
    string ToStatus,
    string Actor,
    DateTime OccurredAt);
