using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Northstar.RetailCatalog.Application.Common;

public static class SkuViolation
{
    private const string UniqueViolationSqlState = "23505";

    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is DbException dbException
        && dbException.SqlState == UniqueViolationSqlState;
}
