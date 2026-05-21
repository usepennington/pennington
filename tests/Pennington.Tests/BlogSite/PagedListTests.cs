using Pennington.BlogSite.Services;

namespace Pennington.Tests.BlogSite;

public class PagedListTests
{
    [Fact]
    public void ComputesTotalPagesFromTotalItemsAndPageSize()
    {
        new PagedList<int>([1, 2, 3], Page: 1, PageSize: 10, TotalItems: 25)
            .TotalPages.ShouldBe(3);
    }

    [Fact]
    public void TotalPagesIsAtLeastOneForEmptyList()
    {
        new PagedList<int>([], Page: 1, PageSize: 10, TotalItems: 0)
            .TotalPages.ShouldBe(1);
    }

    [Fact]
    public void HasPreviousReportsFalseForFirstPage()
    {
        new PagedList<int>([1], 1, 10, 25).HasPrevious.ShouldBeFalse();
        new PagedList<int>([1], 2, 10, 25).HasPrevious.ShouldBeTrue();
    }

    [Fact]
    public void HasNextReportsFalseOnLastPage()
    {
        var paged = new PagedList<int>([1], Page: 3, PageSize: 10, TotalItems: 25);
        paged.HasNext.ShouldBeFalse();
        paged.HasPrevious.ShouldBeTrue();
    }

    [Fact]
    public void TotalPagesIsOneWhenPageSizeIsZero()
    {
        new PagedList<int>([], Page: 1, PageSize: 0, TotalItems: 5)
            .TotalPages.ShouldBe(1);
    }
}
