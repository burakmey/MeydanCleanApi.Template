namespace MeydanCleanApi.Template.Domain.Enums;

/// <summary>
/// The role a file plays for the entity it is attached to.
/// </summary>
/// <remarks>
/// <para>
/// This is what stops the table count from growing. Product photos and a product manual are the same
/// relationship in two different roles, so they live in one attachment table separated by this value
/// rather than in two tables.
/// </para>
/// <para>
/// <strong>Adding a role:</strong> add a member here, add the matching row to <c>LookupDataSeederService</c>,
/// and you are done. No new table, no change to existing code.
/// </para>
/// </remarks>
public enum FilePurposeType
{
    /// <summary>Image shown in a gallery or carousel. Ordered by SortOrder.</summary>
    Gallery = 1,

    /// <summary>Small preview image used in lists and cards.</summary>
    Thumbnail = 2,

    /// <summary>Technical specification sheet, usually a PDF.</summary>
    Datasheet = 3,

    /// <summary>User manual or instruction booklet.</summary>
    Manual = 4,

    /// <summary>Certificate, warranty or other supporting document.</summary>
    Document = 5,

    /// <summary>Profile or avatar image for a person or organisation.</summary>
    Avatar = 6
}
