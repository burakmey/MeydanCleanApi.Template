namespace MeydanCleanApi.Template.Domain.Entities.Files;

/// <summary>
/// Lookup row for <see cref="Enums.FilePurposeType"/> (Gallery, Thumbnail, Datasheet, and so on).
/// </summary>
/// <remarks>
/// Kept as a table so attachment rows can carry a real foreign key. The database then refuses a
/// purpose value that does not exist, which an unchecked <c>int</c> column would happily accept.
/// </remarks>
public class FilePurpose : BasicEntity
{
}
