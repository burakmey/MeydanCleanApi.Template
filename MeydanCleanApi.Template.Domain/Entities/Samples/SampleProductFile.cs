namespace MeydanCleanApi.Template.Domain.Entities.Samples;

/// <summary>
/// Links a <see cref="SampleProduct"/> to an uploaded file, in a particular role.
/// </summary>
/// <remarks>
/// <para>
/// One table holds every file a product has. Gallery photos, the thumbnail and the PDF datasheet are
/// all rows here, told apart by <c>FilePurposeId</c>. Adding a new kind of product document means
/// adding a value to <see cref="Enums.FilePurposeType"/>, not another table.
/// </para>
/// <para>
/// Copy this class when you add attachments to your own aggregate: derive from
/// <see cref="Base.BaseFileAttachment"/>, add the owner foreign key and navigation, and add any
/// typed fields your domain needs.
/// </para>
/// </remarks>
public class SampleProductFile : BaseFileAttachment
{
    /// <summary>
    /// Gets or sets the parent product foreign key identifier.
    /// </summary>
    public required Guid SampleProductId { get; set; }

    /// <summary>
    /// Gets or sets the parent product navigation property. Null unless the query loaded it.
    /// </summary>
    public SampleProduct? SampleProduct { get; set; }
}
