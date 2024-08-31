using Liara.Common.DataAccess;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Entities;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Services;

public class PerformanceEvaluationDataService : IPerformanceEvaluationDataService
{
    private readonly PalavenDbContext _dbContext;
    private readonly IRepository<EvaluationSession> _evaluationSessionRepository;
    private readonly IRepository<LlmResponse> _llmResponseRepository;

    public PerformanceEvaluationDataService(PalavenDbContext dbContext,
        IRepository<EvaluationSession> evaluationSessionRepository,
        IRepository<LlmResponse> llmResponseRepository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _evaluationSessionRepository = evaluationSessionRepository ?? throw new ArgumentNullException(nameof(evaluationSessionRepository));
        _llmResponseRepository = llmResponseRepository ?? throw new ArgumentNullException(nameof(llmResponseRepository));        
    }

    public async Task<EvaluationSession> CreateEvaluationSessionAsync(EvaluationSession evaluationSession, CancellationToken cancellationToken)
    {        
        evaluationSession.CreationDate = DateTime.Now;

        await _evaluationSessionRepository.AddAsync(evaluationSession, cancellationToken);        

        return evaluationSession;
    } 
    
    public async Task AddInstructionToEvaluationSessionAsync(IEnumerable<EvaluationSessionInstruction> instructions, CancellationToken cancellationToken)
    {
        foreach (var instruction in instructions)
        {
            await _dbContext.EvaluationSessionInstructions.AddAsync(instruction, cancellationToken);
        }
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

    public IQueryable<EvaluationSessionInstruction> GetEvaluationSessionInstructionQuery(Func<EvaluationSessionInstruction, bool> criteria)
    {
        return _dbContext.EvaluationSessionInstructions.Where(criteria).AsQueryable();
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

    public int SaveChanges()
    {
        return _dbContext.SaveChanges();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }    
}