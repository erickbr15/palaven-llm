using Liara.Persistence.Abstractions;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Model.Persistence.Entities;
using Palaven.Infrastructure.Model.Persistence.Views;

namespace Palaven.Persistence.EntityFramework.Services;

public class PerformanceEvaluationDataService : IEvaluationSessionDataService
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

    public IQueryable<EvaluationSession> GetEvaluationSessionQuery(Func<EvaluationSession, bool> criteria)
    {
        return _dbContext.EvaluationSessions.Where(criteria).AsQueryable();
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
        var existingResponse = _llmResponseRepository.GetAll().SingleOrDefault(x => 
            x.EvaluationSessionId == chatCompletionResponse.EvaluationSessionId && 
            x.InstructionId == chatCompletionResponse.InstructionId &&
            x.BatchNumber == chatCompletionResponse.BatchNumber &&
            x.EvaluationExerciseId == chatCompletionResponse.EvaluationExerciseId);

        if (existingResponse == null)
        {
            chatCompletionResponse.CreationDate = DateTime.Now;
            return _llmResponseRepository.AddAsync(chatCompletionResponse, cancellationToken);
        }
        else
        {
            existingResponse.Response = chatCompletionResponse.Response;
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
            var llmResponse = _llmResponseRepository.GetById(new { item.EvaluationSessionId, item.InstructionId, item.EvaluationExerciseId });

            llmResponse!.CleanResponse = cleaningStrategy(llmResponse.Response);
            llmResponse.ModifiedDate = DateTime.Now;

            _llmResponseRepository.Update(llmResponse);
        }        
    }

    public IList<LlmResponseView> FetchChatCompletionLlmResponses(Func<LlmResponseView, bool> selectionCriteria)
    {
        var responses = (from response in _dbContext.LlmResponses
                         join evaluationSession in _dbContext.EvaluationSessions on response.EvaluationSessionId equals evaluationSession.SessionId
                         join instruction in _dbContext.Instructions on response.InstructionId equals instruction.InstructionId
                         select new LlmResponseView
                         {
                             EvaluationSessionId = evaluationSession.SessionId,
                             DatasetId = evaluationSession.DatasetId,
                             BatchSize = evaluationSession.BatchSize,
                             LargeLanguageModel = evaluationSession.LargeLanguageModel,
                             DeviceInfo = evaluationSession.DeviceInfo,
                             EvaluationExercise = ChatCompletionExcerciseType.GetChatCompletionExcerciseTypeDescription(response.EvaluationExerciseId),
                             InstructionId = instruction.InstructionId,
                             BatchNumber = response.BatchNumber,
                             Instruction = instruction.Instruction,
                             Response = instruction.Response,
                             Category = instruction.Category,
                             LlmResponseToEvaluate = response.CleanResponse,
                             ElapsedTime = response.ElapsedTime
                         })
                         .Where(selectionCriteria)
                         .ToList();

        return responses;
    }

    public IList<InstructionEntity> FetchChatCompletionLlmInstructions(Guid evaluationSessionId, int evaluationExerciseId, int batchNumber)
    {
        var evaluationSession = _dbContext.EvaluationSessions.Single(x => x.SessionId == evaluationSessionId);

        var datasetId = evaluationSession.DatasetId;

        var batchSize = evaluationSession.BatchSize;
        var offset = (batchNumber - 1) * batchSize;

        var batchInstructionIds = (from es in _dbContext.EvaluationSessions
                                   join esi in _dbContext.EvaluationSessionInstructions on es.SessionId equals esi.EvaluationSessionId
                                   join i in _dbContext.Instructions on esi.InstructionId equals i.InstructionId
                                   where es.SessionId == evaluationSessionId &&
                                         esi.InstructionPurpose == "test"
                                   select i.InstructionId)
                           .Skip(offset)
                           .Take(batchSize);

        var instructions = (from es in _dbContext.EvaluationSessions
                            join esi in _dbContext.EvaluationSessionInstructions on es.SessionId equals esi.EvaluationSessionId
                            join i in _dbContext.Instructions on esi.InstructionId equals i.InstructionId
                            join llmr in _dbContext.LlmResponses on new
                            {
                                i.InstructionId,
                                EvaluationExerciseId = evaluationExerciseId,
                                BatchNumber = batchNumber
                            }
                            equals new
                            {
                                llmr.InstructionId,
                                llmr.EvaluationExerciseId,
                                llmr.BatchNumber
                            }
                            into llmrJoin
                            from llmrLeft in llmrJoin.DefaultIfEmpty()
                            where es.SessionId == evaluationSessionId &&
                                  es.DatasetId == datasetId &&
                                  esi.InstructionPurpose == "test" &&
                                  batchInstructionIds.Contains(i.InstructionId) &&
                                  llmrLeft.Instruction == null                            
                            select i).ToList();
        
        return instructions;
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