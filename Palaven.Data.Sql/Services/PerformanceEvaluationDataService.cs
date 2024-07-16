using Liara.Common.DataAccess;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Services;

public class PerformanceEvaluationDataService : IPerformanceEvaluationDataService
{
    private readonly PalavenDbContext _dbContext;
    private readonly IRepository<EvaluationSession> _evaluationSessionRepository;    
    private readonly IRepository<FineTunedLlmResponse> _fineTunedLlmResponseRepository;
    private readonly IRepository<FineTunedLlmWithRagResponse> _fineTunedLlmWithRagResponseRepository;
    private readonly IRepository<LlmResponse> _llmResponseRepository;
    private readonly IRepository<LlmWithRagResponse> _llmWithRagResponseRepository;
    private readonly IRepository<BertScoreMetric> _bertScoreMetricRepository;
    private readonly IRepository<RougeScoreMetric> _rougeScoreMetricRepository;

    public PerformanceEvaluationDataService(PalavenDbContext dbContext,
        IRepository<EvaluationSession> evaluationSessionRepository,        
        IRepository<FineTunedLlmResponse> fineTunedLlmResponseRepository,
        IRepository<FineTunedLlmWithRagResponse> fineTunedLlmWithRagResponseRepository,
        IRepository<LlmResponse> llmResponseRepository,
        IRepository<LlmWithRagResponse> llmWithRagResponseRepository,
        IRepository<BertScoreMetric> bertScoreMetricRepository,
        IRepository<RougeScoreMetric> rougeScoreMetricRepository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _evaluationSessionRepository = evaluationSessionRepository ?? throw new ArgumentNullException(nameof(evaluationSessionRepository));        
        _fineTunedLlmResponseRepository = fineTunedLlmResponseRepository ?? throw new ArgumentNullException(nameof(fineTunedLlmResponseRepository));
        _fineTunedLlmWithRagResponseRepository = fineTunedLlmWithRagResponseRepository ?? throw new ArgumentNullException(nameof(fineTunedLlmWithRagResponseRepository));
        _llmResponseRepository = llmResponseRepository ?? throw new ArgumentNullException(nameof(llmResponseRepository));
        _llmWithRagResponseRepository = llmWithRagResponseRepository ?? throw new ArgumentNullException(nameof(llmWithRagResponseRepository));
        _bertScoreMetricRepository = bertScoreMetricRepository ?? throw new ArgumentNullException(nameof(bertScoreMetricRepository));
        _rougeScoreMetricRepository = rougeScoreMetricRepository ?? throw new ArgumentNullException(nameof(rougeScoreMetricRepository));
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

    public async Task<EvaluationSession?> GetEvaluationSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return await _evaluationSessionRepository.GetByIdAsync(sessionId, cancellationToken);
    }

    public async Task UpsertChatCompletionPerformanceEvaluationAsync(BertScoreMetric bertScoreMetrics, CancellationToken cancellationToken)
    {
        var existingEvaluation = _bertScoreMetricRepository
            .GetAll()
            .SingleOrDefault(x => x.SessionId == bertScoreMetrics.SessionId && x.BatchNumber == bertScoreMetrics.BatchNumber);

        if(existingEvaluation == null)
        {
            bertScoreMetrics.CreationDate = DateTime.Now;
            await _bertScoreMetricRepository.AddAsync(bertScoreMetrics, cancellationToken);
        }
        else
        {
            existingEvaluation.BertScorePrecision = bertScoreMetrics.BertScorePrecision;
            existingEvaluation.BertScoreRecall = bertScoreMetrics.BertScoreRecall;
            existingEvaluation.BertScoreF1 = bertScoreMetrics.BertScoreF1;
            existingEvaluation.ModifiedDate = DateTime.Now;

            _bertScoreMetricRepository.Update(existingEvaluation);
        }        
    }

    public async Task UpsertChatCompletionPerformanceEvaluationAsync(IEnumerable<RougeScoreMetric> rougeScoreMetrics, CancellationToken cancellationToken)
    {
        foreach (var rougeScoreMetric in rougeScoreMetrics.ToList())
        {
            await UpsertChatCompletionPerformanceEvaluationAsync(rougeScoreMetric, cancellationToken);
        }
    }

    private async Task UpsertChatCompletionPerformanceEvaluationAsync(RougeScoreMetric rougeScoreMetric, CancellationToken cancellationToken)
    {
        var existingEvaluation = _rougeScoreMetricRepository
            .GetAll()
            .SingleOrDefault(x => x.SessionId == rougeScoreMetric.SessionId && x.BatchNumber == rougeScoreMetric.BatchNumber && string.Equals(x.RougeType, rougeScoreMetric.RougeType, StringComparison.OrdinalIgnoreCase));

        if(existingEvaluation == null)
        {
            rougeScoreMetric.CreationDate = DateTime.Now;
            await _rougeScoreMetricRepository.AddAsync(rougeScoreMetric, cancellationToken);
        }
        else
        {
            existingEvaluation.RougePrecision = rougeScoreMetric.RougePrecision;
            existingEvaluation.RougeRecall = rougeScoreMetric.RougeRecall;
            existingEvaluation.RougeF1 = rougeScoreMetric.RougeF1;
            existingEvaluation.ModifiedDate = DateTime.Now;

            _rougeScoreMetricRepository.Update(existingEvaluation);            
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

    public void CleanChatCompletionResponses(Func<FineTunedLlmResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy)
    {
        var chatCompletionResponses = _fineTunedLlmResponseRepository.GetAll().Where(selectionCriteria).ToList();
        
        foreach (var item in chatCompletionResponses)
        {
            var llmResponse = _fineTunedLlmResponseRepository.GetById(item.Id);

            llmResponse!.LlmResponseToEvaluate = cleaningStrategy(llmResponse.ResponseCompletion);
            llmResponse.ModifiedDate = DateTime.Now;

            _fineTunedLlmResponseRepository.Update(llmResponse);
        }        
    }  

    public void CleanChatCompletionResponses(Func<FineTunedLlmWithRagResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy)
    {
        var chatCompletionResponses = _fineTunedLlmWithRagResponseRepository.GetAll().Where(selectionCriteria).ToList();
        
        foreach (var item in chatCompletionResponses)
        {
            var llmResponse = _fineTunedLlmWithRagResponseRepository.GetById(item.Id);

            llmResponse!.LlmResponseToEvaluate = cleaningStrategy(llmResponse.ResponseCompletion);
            llmResponse.ModifiedDate = DateTime.Now;

            _fineTunedLlmWithRagResponseRepository.Update(llmResponse);
        }        
    }

    public void CleanChatCompletionResponses(Func<LlmResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy)
    {
        var chatCompletionResponses = _llmResponseRepository.GetAll().Where(selectionCriteria).ToList();
        
        foreach (var item in chatCompletionResponses)
        {
            var llmResponse = _llmResponseRepository.GetById(item.Id);

            llmResponse!.LlmResponseToEvaluate = cleaningStrategy(llmResponse.ResponseCompletion);
            llmResponse.ModifiedDate = DateTime.Now;

            _llmResponseRepository.Update(llmResponse);
        }        
    }

    public void CleanChatCompletionResponses(Func<LlmWithRagResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy)
    {
        var chatCompletionResponses = _llmWithRagResponseRepository.GetAll().Where(selectionCriteria).ToList();
        
        foreach (var item in chatCompletionResponses)
        {
            var llmResponse = _llmWithRagResponseRepository.GetById(item.Id);

            llmResponse!.LlmResponseToEvaluate = cleaningStrategy(llmResponse.ResponseCompletion);
            llmResponse.ModifiedDate = DateTime.Now;

            _llmWithRagResponseRepository.Update(llmResponse);
        }        
    }

    public IList<LlmResponseView> FetchChatCompletionLlmResponses(Func<LlmResponseView, bool> selectionCriteria)
    {
        var responses = (from response in _dbContext.LlmResponses
                         join evaluationSession in _dbContext.EvaluationSessions on response.SessionId equals evaluationSession.SessionId
                         join instruction in _dbContext.Instructions on response.InstructionId equals instruction.Id                         
                         select new LlmResponseView
                         {
                             EvaluationSessionId = evaluationSession.SessionId,
                             DatasetId = evaluationSession.DatasetId,
                             BatchSize = evaluationSession.BatchSize,
                             LargeLanguageModel = evaluationSession.LargeLanguageModel,
                             DeviceInfo = evaluationSession.DeviceInfo,
                             ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmVanilla,
                             InstructionId = instruction.Id,
                             BatchNumber = response.BatchNumber,
                             Instruction = instruction.Instruction,
                             Response = instruction.Response,
                             Category = instruction.Category,
                             LlmResponseToEvaluate = response.LlmResponseToEvaluate,
                             ElapsedTime = response.ElapsedTime
                         }).Where(selectionCriteria).ToList();

        return responses;
    }

    public IList<LlmResponseView> FetchChatCompletionLlmWithRagResponses(Func<LlmResponseView, bool> selectionCriteria)
    {
        var responses = (from response in _dbContext.LlmWithRagResponses
                         join evaluationSession in _dbContext.EvaluationSessions on response.SessionId equals evaluationSession.SessionId
                         join instruction in _dbContext.Instructions on response.InstructionId equals instruction.Id
                         select new LlmResponseView
                         {
                             EvaluationSessionId = evaluationSession.SessionId,
                             DatasetId = evaluationSession.DatasetId,
                             BatchSize = evaluationSession.BatchSize,
                             LargeLanguageModel = evaluationSession.LargeLanguageModel,
                             DeviceInfo = evaluationSession.DeviceInfo,
                             ChatCompletionExcerciseType = ChatCompletionExcerciseType.LlmWithRag,
                             InstructionId = instruction.Id,
                             BatchNumber = response.BatchNumber,
                             Instruction = instruction.Instruction,
                             Response = instruction.Response,
                             Category = instruction.Category,
                             LlmResponseToEvaluate = response.LlmResponseToEvaluate,
                             ElapsedTime = response.ElapsedTime
                         }).Where(selectionCriteria).ToList();

        return responses;
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