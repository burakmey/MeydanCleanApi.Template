using System.Linq.Expressions;
using MeydanCleanApi.Template.Application.Abstractions.Messaging;
using MeydanCleanApi.Template.Application.Abstractions.Repositories;
using MeydanCleanApi.Template.Application.Common.Models.FileStorage;
using MeydanCleanApi.Template.Domain.Entities.Files;

namespace MeydanCleanApi.Template.Application.Features.Files.Queries.GetFilesPaged;

/// <summary>
/// Query handler executing paged file queries.
/// </summary>
/// <remarks>
/// <para>
/// Initializes a new instance of the <see cref="GetFilesPagedQueryHandler"/> class.
/// </para>
/// <para>
/// This is an administrative view over the raw storage table, which is why the endpoint requires the
/// Admin policy. Its main use is spotting files still stuck in <c>Pending</c> — uploads that were
/// started and never confirmed. To list the files belonging to something, query that entity's
/// attachments instead.
/// </para>
/// </remarks>
public sealed class GetFilesPagedQueryHandler(
    IReadRepository<FileEntity, Guid> fileReadRepository
) : IRequestHandler<GetFilesPagedQuery, BaseResponse<GetFilesPagedQueryResponse>>
{
    private readonly IReadRepository<FileEntity, Guid> _fileReadRepository = fileReadRepository;

    /// <inheritdoc />
    public async Task<BaseResponse<GetFilesPagedQueryResponse>> Handle(GetFilesPagedQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Build the filter from whichever optional criteria were supplied.
        //    The term is lowered on both sides because LIKE in PostgreSQL is case sensitive, so
        //    matching it as typed would miss a file stored under a differently-cased name.
        var statusId = request.StatusId;
        var searchTerm = request.SearchTerm?.Trim().ToLowerInvariant();

        Expression<Func<FileEntity, bool>> filter = file =>
            (!statusId.HasValue || file.FileStatusId == statusId.Value)
            && (string.IsNullOrWhiteSpace(searchTerm) || file.OriginalFileName.ToLower().Contains(searchTerm));

        // 2. Page at the database level.
        var pagedEntities = await _fileReadRepository.GetPagedAsync(request.Page, predicate: filter, ct: ct);

        // 3. Project to the response DTO, which deliberately omits the internal storage path.
        var pagedDtos = pagedEntities.Map(f => new FileDto(
            f.Id,
            f.FileStorageId,
            f.FileStatusId,
            f.OriginalFileName,
            f.ContentType,
            f.SizeInBytes,
            f.CreatedAt
        ));

        return BaseResponse<GetFilesPagedQueryResponse>.Success(new GetFilesPagedQueryResponse(pagedDtos));
    }
}
