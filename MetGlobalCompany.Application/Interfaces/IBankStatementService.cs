using System.Threading;
using System.Threading.Tasks;

namespace MetGlobalCompany.Application.Interfaces;

public interface IBankStatementService
{
    Task<int> ImportFrom1CFormatAsync(string filePath, CancellationToken cancellationToken = default);
}