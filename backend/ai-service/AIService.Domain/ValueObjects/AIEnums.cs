namespace AIService.Domain.ValueObjects;

public enum ClassificationStatus
{
    Processing = 0,
    Completed = 1,
    Failed = 2
}

public enum RecommendationStatus
{
    Generated = 0,
    Accepted = 1,
    Rejected = 2,
    Expired = 3
}
