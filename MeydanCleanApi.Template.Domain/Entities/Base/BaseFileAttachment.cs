using MeydanCleanApi.Template.Domain.Entities.Files;
using MeydanCleanApi.Template.Domain.Interfaces.Entities;

namespace MeydanCleanApi.Template.Domain.Entities.Base;

/// <summary>
/// Base class for the table that links one aggregate to its uploaded files.
/// </summary>
/// <remarks>
/// <para>
/// <strong>The rule: one attachment table per aggregate root, not per file kind.</strong>
/// A product's photos and its PDF manual belong in the same table, told apart by
/// <see cref="FilePurposeId"/>. Creating a second table for documents would be modelling the file
/// format rather than the relationship.
/// </para>
/// <para>
/// <strong>Where to put extra fields:</strong> this base holds only what every attachment needs.
/// Anything specific to your domain goes on your own derived entity as a normal typed property, so
/// the shared base never has to change:
/// </para>
/// <code>
/// public class InvoiceFile : BaseFileAttachment
/// {
///     public required Guid InvoiceId { get; set; }
///     public Invoice? Invoice { get; set; }
///
///     // Your own fields — typed, queryable, validated like anything else.
///     public int? PageCount { get; set; }
///     public string? AltText { get; set; }
/// }
/// </code>
/// </remarks>
public abstract class BaseFileAttachment : GuidEntity, ISortable
{
    /// <summary>
    /// Gets or sets the primary key, aliased to <see cref="FileEntityId"/>.
    /// </summary>
    /// <remarks>
    /// Sharing the key with the file means an attachment row and its file are the same identifier, so
    /// no join is needed to go from one to the other. It also means a given file can be attached
    /// once and only once — upload it again if two owners need their own copy.
    /// </remarks>
    public override Guid Id
    {
        get => FileEntityId;
        set => FileEntityId = value;
    }

    /// <summary>
    /// Gets or sets the foreign key of the uploaded file this row points at.
    /// </summary>
    public required Guid FileEntityId { get; set; }

    /// <summary>
    /// Gets or sets the role this file plays for its owner (see <see cref="Enums.FilePurposeType"/>).
    /// </summary>
    public required int FilePurposeId { get; set; }

    /// <summary>
    /// Gets or sets the display order within the same purpose. Lower numbers appear first.
    /// </summary>
    public required int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the uploaded file itself, carrying the storage path and metadata.
    /// Null unless the query loaded it.
    /// </summary>
    public FileEntity? FileEntity { get; set; }

    /// <summary>
    /// Gets or sets the purpose lookup row. Null unless the query loaded it.
    /// </summary>
    public FilePurpose? FilePurpose { get; set; }
}
