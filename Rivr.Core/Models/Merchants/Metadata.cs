namespace Rivr.Core.Models.Merchants;

/// <summary>
/// Represents the metadata of a response.
/// </summary>
public class Metadata
{
    /// <summary>
    /// The page number of the response.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The page size of the response.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of items in the response.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// The total number of pages in the response.
    /// </summary>
    public int TotalPages { get; set; }
}