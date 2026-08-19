namespace MeydanCleanApi.Template.Domain.Interfaces.Entities;

/// <summary>
/// Defines a contract for entities that support custom display ordering.
/// </summary>
public interface ISortable
{
    /// <summary>
    /// Gets or sets the numerical sort order position. Lower numbers appear first.
    /// </summary>
    int SortOrder { get; set; }
}
