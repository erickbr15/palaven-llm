using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Model.Ingest;

public static class CommandMappingExtensions
{
    public static EtlTaskDocument ToEtlTaskDocument(this StartTaxLawIngestCommand command)
    {
        var documentId = Guid.NewGuid();

        var etlTask = new EtlTaskDocument
        {
            Id = documentId.ToString(),
            TenantId = command.UserId,
            UserId = command.UserId,
            DocumentSchema = nameof(EtlTaskDocument),
            IsTaskCompleted = false
        };


        var fileName = $"{documentId}{command.FileExtension}";

        etlTask.Metadata.Add(EtlMetadataKeys.FileName, fileName);
        etlTask.Metadata.Add(EtlMetadataKeys.FileUntrustedName, command.UntrustedFileName);
        etlTask.Metadata.Add(EtlMetadataKeys.FileContentLocale, "es");
        etlTask.Metadata.Add(EtlMetadataKeys.LawAcronym, command.AcronymLaw);
        etlTask.Metadata.Add(EtlMetadataKeys.LawName, command.NameLaw);
        etlTask.Metadata.Add(EtlMetadataKeys.LawYear, command.YearLaw.ToString());

        return etlTask;
    }
}
