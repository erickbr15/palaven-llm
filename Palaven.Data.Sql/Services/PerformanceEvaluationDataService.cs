using Liara.Common.DataAccess;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Services;

public class PerformanceEvaluationDataService : IPerformanceEvaluationDataService
{
    private readonly PalavenDbContext _dbContext;
    private readonly IRepository<EvaluationSession> _evaluationSessionRepository;
    private readonly IRepository<BertScoreMetric> _bertScoreMetricRepository;
    private readonly IRepository<FineTunedLlmResponse> _fineTunedLlmResponseRepository;
    private readonly IRepository<FineTunedLlmWithRagResponse> _fineTunedLlmWithRagResponseRepository;
    private readonly IRepository<LlmResponse> _llmResponseRepository;
    private readonly IRepository<LlmWithRagResponse> _llmWithRagResponseRepository;

    public PerformanceEvaluationDataService(PalavenDbContext dbContext,
        IRepository<EvaluationSession> evaluationSessionRepository,
        IRepository<BertScoreMetric> bertScoreMetricRepository,
        IRepository<FineTunedLlmResponse> fineTunedLlmResponseRepository,
        IRepository<FineTunedLlmWithRagResponse> fineTunedLlmWithRagResponseRepository,
        IRepository<LlmResponse> llmResponseRepository,
        IRepository<LlmWithRagResponse> llmWithRagResponseRepository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _evaluationSessionRepository = evaluationSessionRepository ?? throw new ArgumentNullException(nameof(evaluationSessionRepository));
        _bertScoreMetricRepository = bertScoreMetricRepository ?? throw new ArgumentNullException(nameof(bertScoreMetricRepository));
        _fineTunedLlmResponseRepository = fineTunedLlmResponseRepository ?? throw new ArgumentNullException(nameof(fineTunedLlmResponseRepository));
        _fineTunedLlmWithRagResponseRepository = fineTunedLlmWithRagResponseRepository ?? throw new ArgumentNullException(nameof(fineTunedLlmWithRagResponseRepository));
        _llmResponseRepository = llmResponseRepository ?? throw new ArgumentNullException(nameof(llmResponseRepository));
        _llmWithRagResponseRepository = llmWithRagResponseRepository ?? throw new ArgumentNullException(nameof(llmWithRagResponseRepository));
    }

    public async Task<EvaluationSession> CreateEvaluationSessionAsync(EvaluationSession evaluationSession, CancellationToken cancellationToken)
    {
        evaluationSession.SessionId = Guid.NewGuid();
        evaluationSession.CreationDate = DateTime.Now;

        await _evaluationSessionRepository.AddAsync(evaluationSession, cancellationToken);

        return evaluationSession;
    }    

    public async Task<EvaluationSession> UpdateEvaluationSessionAsync(EvaluationSession evaluationSession, CancellationToken cancellationToken)
    {
        var evaluationSessionEntity = await _evaluationSessionRepository.GetByIdAsync(evaluationSession.SessionId, cancellationToken);
        if(evaluationSessionEntity == null)
        {
            throw new InvalidOperationException($"Evaluation session with id {evaluationSession.SessionId} not found.");
        }

        evaluationSessionEntity.StartDate = evaluationSession.StartDate;
        evaluationSessionEntity.EndDate = evaluationSession.EndDate;
        evaluationSessionEntity.BatchSize = evaluationSession.BatchSize;
        evaluationSessionEntity.LargeLanguageModel = evaluationSession.LargeLanguageModel;
        evaluationSessionEntity.IsActive = evaluationSession.IsActive;        
        evaluationSessionEntity.ModifiedDate = DateTime.Now;

        _evaluationSessionRepository.Update(evaluationSessionEntity);

        return evaluationSessionEntity;
    }

    public async Task UpsertChatCompletionPerformanceEvaluationAsync(BertScoreMetric chatCompletionPerformanceEvaluation, CancellationToken cancellationToken)
    {
        var existingEvaluation = _bertScoreMetricRepository.GetAll().SingleOrDefault(x => x.SessionId == chatCompletionPerformanceEvaluation.SessionId && x.BatchNumber == chatCompletionPerformanceEvaluation.BatchNumber);

        if(existingEvaluation == null)
        {
            chatCompletionPerformanceEvaluation.CreationDate = DateTime.Now;
            await _bertScoreMetricRepository.AddAsync(chatCompletionPerformanceEvaluation, cancellationToken);
        }
        else
        {
            existingEvaluation.BertScorePrecision = chatCompletionPerformanceEvaluation.BertScorePrecision;
            existingEvaluation.BertScoreRecall = chatCompletionPerformanceEvaluation.BertScoreRecall;
            existingEvaluation.BertScoreF1 = chatCompletionPerformanceEvaluation.BertScoreF1;
            existingEvaluation.ModifiedDate = DateTime.Now;

            _bertScoreMetricRepository.Update(existingEvaluation);
        }        
    }

    public async Task UpsertChatCompletionResponseAsync(IEnumerable<FineTunedLlmResponse> chatCompletionResponses, CancellationToken cancellationToken)
    {
        foreach (var chatCompletionResponse in chatCompletionResponses.ToList())
        {
            await UpsertChatCompletionResponseAsync(chatCompletionResponse, cancellationToken);
        }
    }

    private Task UpsertChatCompletionResponseAsync(FineTunedLlmResponse chatCompletionResponse, CancellationToken cancellationToken)
    {
        var existingResponse = _fineTunedLlmResponseRepository.GetAll().SingleOrDefault(x => x.SessionId == chatCompletionResponse.SessionId && x.InstructionId == chatCompletionResponse.InstructionId);

        if(existingResponse == null)
        {
            chatCompletionResponse.CreationDate = DateTime.Now;
            return _fineTunedLlmResponseRepository.AddAsync(chatCompletionResponse, cancellationToken);
        }
        else
        {
            existingResponse.ResponseCompletion = chatCompletionResponse.ResponseCompletion;
            existingResponse.ModifiedDate = DateTime.Now;

            _fineTunedLlmResponseRepository.Update(existingResponse);

            return Task.CompletedTask;
        }
    }

    public async Task UpsertChatCompletionResponseAsync(IEnumerable<FineTunedLlmWithRagResponse> chatCompletionResponses, CancellationToken cancellationToken)
    {
        foreach (var chatCompletionResponse in chatCompletionResponses.ToList())
        {
            await UpsertChatCompletionResponseAsync(chatCompletionResponse, cancellationToken);
        }
    }

    private Task UpsertChatCompletionResponseAsync(FineTunedLlmWithRagResponse chatCompletionResponse, CancellationToken cancellationToken)
    {
        var existingResponse = _fineTunedLlmWithRagResponseRepository.GetAll().SingleOrDefault(x => x.SessionId == chatCompletionResponse.SessionId && x.InstructionId == chatCompletionResponse.InstructionId);

        if(existingResponse == null)
        {
            chatCompletionResponse.CreationDate = DateTime.Now;
            return _fineTunedLlmWithRagResponseRepository.AddAsync(chatCompletionResponse, cancellationToken);
        }
        else
        {
            existingResponse.ResponseCompletion = chatCompletionResponse.ResponseCompletion;
            existingResponse.ModifiedDate = DateTime.Now;

            _fineTunedLlmWithRagResponseRepository.Update(existingResponse);

            return Task.CompletedTask;
        }
    }

    public async Task UpsertChatCompletionResponseAsync(IEnumerable<LlmResponse> chatCompletionResponses, CancellationToken cancellationToken)
    {
        foreach(var chatCompletionResponse in chatCompletionResponses.ToList())
        {
            await UpsertChatCompletionResponseAsync(chatCompletionResponse, cancellationToken);
        }
    }

    private Task UpsertChatCompletionResponseAsync(LlmResponse chatCompletionResponse, CancellationToken cancellationToken)
    {
        var existingResponse = _llmResponseRepository.GetAll().SingleOrDefault(x => x.SessionId == chatCompletionResponse.SessionId && x.InstructionId == chatCompletionResponse.InstructionId);

        if(existingResponse == null)
        {
            chatCompletionResponse.CreationDate = DateTime.Now;
            return _llmResponseRepository.AddAsync(chatCompletionResponse, cancellationToken);
        }
        else
        {
            existingResponse.ResponseCompletion = chatCompletionResponse.ResponseCompletion;
            existingResponse.ModifiedDate = DateTime.Now;

            _llmResponseRepository.Update(existingResponse);

            return Task.CompletedTask;
        }
    }

    public async Task UpsertChatCompletionResponseAsync(IEnumerable<LlmWithRagResponse> chatCompletionResponses, CancellationToken cancellationToken)
    {
        foreach(var chatCompletionResponse in chatCompletionResponses.ToList())
        {
            await UpsertChatCompletionResponseAsync(chatCompletionResponse, cancellationToken);
        }
    }

    private Task UpsertChatCompletionResponseAsync(LlmWithRagResponse chatCompletionResponse, CancellationToken cancellationToken)
    {
        var existingResponse = _llmWithRagResponseRepository.GetAll().SingleOrDefault(x => x.SessionId == chatCompletionResponse.SessionId && x.InstructionId == chatCompletionResponse.InstructionId);

        if(existingResponse == null)
        {
            chatCompletionResponse.CreationDate = DateTime.Now;
            return _llmWithRagResponseRepository.AddAsync(chatCompletionResponse, cancellationToken);
        }
        else
        {
            existingResponse.ResponseCompletion = chatCompletionResponse.ResponseCompletion;
            existingResponse.ModifiedDate = DateTime.Now;

            _llmWithRagResponseRepository.Update(existingResponse);

            return Task.CompletedTask;
        }
    }

    public int SaveChanges()
    {
        return _dbContext.SaveChanges();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}