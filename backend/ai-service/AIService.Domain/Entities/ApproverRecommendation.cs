using AIService.Domain.ValueObjects;

namespace AIService.Domain.Entities;

public class ApproverRecommendation
{
    public Guid Id { get; private set; }
    public string EntityId { get; private set; }
    public string EntityType { get; private set; }
    public Guid RecommendedApproverId { get; private set; }
    public string RecommendedApproverName { get; private set; }
    public decimal ConfidenceScore { get; private set; }
    public string ReasoningFactors { get; private set; }
    public RecommendationStatus Status { get; private set; }
    public string ModelVersion { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public Guid TenantId { get; private set; }
    public bool IsAccepted { get; private set; }
    public Guid? AcceptedBy { get; private set; }
    public DateTime? AcceptedAt { get; private set; }

    private readonly List<RecommendationFactor> _factors = new();
    public IReadOnlyList<RecommendationFactor> Factors => _factors.AsReadOnly();

    private ApproverRecommendation() { } // For EF Core

    public ApproverRecommendation(string entityId, string entityType, Guid recommendedApproverId, 
        string recommendedApproverName, decimal confidenceScore, string reasoningFactors, 
        string modelVersion, Guid tenantId)
    {
        Id = Guid.NewGuid();
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        RecommendedApproverId = recommendedApproverId;
        RecommendedApproverName = recommendedApproverName ?? throw new ArgumentNullException(nameof(recommendedApproverName));
        ConfidenceScore = Math.Max(0, Math.Min(100, confidenceScore));
        ReasoningFactors = reasoningFactors ?? throw new ArgumentNullException(nameof(reasoningFactors));
        ModelVersion = modelVersion ?? throw new ArgumentNullException(nameof(modelVersion));
        TenantId = tenantId;
        Status = RecommendationStatus.Generated;
        GeneratedAt = DateTime.UtcNow;
        IsAccepted = false;
    }

    public void AddFactor(string factorType, string description, decimal weight)
    {
        var factor = new RecommendationFactor(Id, factorType, description, weight);
        _factors.Add(factor);
    }

    public void Accept(Guid acceptedBy)
    {
        IsAccepted = true;
        AcceptedBy = acceptedBy;
        AcceptedAt = DateTime.UtcNow;
        Status = RecommendationStatus.Accepted;
    }

    public void Reject()
    {
        Status = RecommendationStatus.Rejected;
    }
}

public class RecommendationFactor
{
    public Guid Id { get; private set; }
    public Guid RecommendationId { get; private set; }
    public string FactorType { get; private set; }
    public string Description { get; private set; }
    public decimal Weight { get; private set; }

    // Navigation property
    public ApproverRecommendation Recommendation { get; private set; } = null!;

    private RecommendationFactor() { } // For EF Core

    public RecommendationFactor(Guid recommendationId, string factorType, string description, decimal weight)
    {
        Id = Guid.NewGuid();
        RecommendationId = recommendationId;
        FactorType = factorType ?? throw new ArgumentNullException(nameof(factorType));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Weight = Math.Max(0, Math.Min(1, weight));
    }
}
