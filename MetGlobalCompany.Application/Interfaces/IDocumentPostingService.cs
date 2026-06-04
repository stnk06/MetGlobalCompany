using System.Threading;
using System.Threading.Tasks;

namespace MetGlobalCompany.Application.Interfaces;

public interface IDocumentPostingService
{
    Task<bool> PostSalesInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<bool> UnpostSalesInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);

    Task<bool> PostPurchaseInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<bool> UnpostPurchaseInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);

    Task<bool> PostPaymentAsync(int paymentId, CancellationToken cancellationToken = default);
    Task<bool> UnpostPaymentAsync(int paymentId, CancellationToken cancellationToken = default);

    Task<bool> PostPriceSettingAsync(int priceSettingId, CancellationToken cancellationToken = default);
    Task<bool> UnpostPriceSettingAsync(int priceSettingId, CancellationToken cancellationToken = default);
}