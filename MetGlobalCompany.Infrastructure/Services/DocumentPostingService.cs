using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.Domain.Enums;
using MetGlobalCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetGlobalCompany.Infrastructure.Services;

public class DocumentPostingService : IDocumentPostingService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DocumentPostingService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<bool> PostPurchaseInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var invoice = await context.PurchaseInvoices.Include(i => i.Details).ThenInclude(d => d.Nomenclature).FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
            if (invoice == null) return false;
            if (invoice.IsPosted) await UnpostPurchaseInvoiceInternalAsync(context, invoiceId, cancellationToken);

            foreach (var detail in invoice.Details)
            {
                if (detail.Nomenclature.IsService) continue;
                context.InventoryLedgers.Add(new InventoryLedger { Period = invoice.Date, RegistrarName = nameof(PurchaseInvoice), RegistrarId = invoice.Id, NomenclatureId = detail.NomenclatureId, MovementType = MovementType.Receipt, Quantity = detail.Quantity });
            }

            context.SettlementLedgers.Add(new SettlementLedger { Period = invoice.Date, RegistrarName = nameof(PurchaseInvoice), RegistrarId = invoice.Id, ContractorId = invoice.ContractorId, ContractId = invoice.ContractId, MovementType = MovementType.Expense, Amount = invoice.TotalAmount });

            invoice.IsPosted = true;
            context.Update(invoice);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }

    public async Task<bool> UnpostPurchaseInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await UnpostPurchaseInvoiceInternalAsync(context, invoiceId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }

    private async Task<bool> UnpostPurchaseInvoiceInternalAsync(AppDbContext context, int invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await context.PurchaseInvoices.FindAsync(new object[] { invoiceId }, cancellationToken);
        if (invoice == null) return false;

        context.InventoryLedgers.RemoveRange(await context.InventoryLedgers.Where(r => r.RegistrarName == nameof(PurchaseInvoice) && r.RegistrarId == invoiceId).ToListAsync(cancellationToken));
        context.SettlementLedgers.RemoveRange(await context.SettlementLedgers.Where(r => r.RegistrarName == nameof(PurchaseInvoice) && r.RegistrarId == invoiceId).ToListAsync(cancellationToken));

        invoice.IsPosted = false;
        context.Update(invoice);
        return true;
    }

    public async Task<bool> PostSalesInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var invoice = await context.SalesInvoices.Include(i => i.Details).ThenInclude(d => d.Nomenclature).FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
            if (invoice == null) return false;
            if (invoice.IsPosted) await UnpostSalesInvoiceInternalAsync(context, invoiceId, cancellationToken);

            foreach (var detail in invoice.Details)
            {
                if (detail.Nomenclature.IsService) continue;

                var receipts = await context.InventoryLedgers.Where(l => l.NomenclatureId == detail.NomenclatureId && l.MovementType == MovementType.Receipt).SumAsync(l => l.Quantity, cancellationToken);
                var expenses = await context.InventoryLedgers.Where(l => l.NomenclatureId == detail.NomenclatureId && l.MovementType == MovementType.Expense).SumAsync(l => l.Quantity, cancellationToken);
                var currentStock = receipts - expenses;

                if (currentStock < detail.Quantity)
                {
                    throw new InvalidOperationException($"КРИТИЧЕСКАЯ ОШИБКА: Недостаточно товара '{detail.Nomenclature.Name}' на складе! Доступно: {currentStock}, Пытаемся списать: {detail.Quantity}.");
                }

                context.InventoryLedgers.Add(new InventoryLedger { Period = invoice.Date, RegistrarName = nameof(SalesInvoice), RegistrarId = invoice.Id, NomenclatureId = detail.NomenclatureId, MovementType = MovementType.Expense, Quantity = detail.Quantity });
            }

            context.SettlementLedgers.Add(new SettlementLedger { Period = invoice.Date, RegistrarName = nameof(SalesInvoice), RegistrarId = invoice.Id, ContractorId = invoice.ContractorId, ContractId = invoice.ContractId, MovementType = MovementType.Receipt, Amount = invoice.TotalAmount });

            invoice.IsPosted = true;
            context.Update(invoice);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }

    public async Task<bool> UnpostSalesInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await UnpostSalesInvoiceInternalAsync(context, invoiceId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }

    private async Task<bool> UnpostSalesInvoiceInternalAsync(AppDbContext context, int invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await context.SalesInvoices.FindAsync(new object[] { invoiceId }, cancellationToken);
        if (invoice == null) return false;

        context.InventoryLedgers.RemoveRange(await context.InventoryLedgers.Where(r => r.RegistrarName == nameof(SalesInvoice) && r.RegistrarId == invoiceId).ToListAsync(cancellationToken));
        context.SettlementLedgers.RemoveRange(await context.SettlementLedgers.Where(r => r.RegistrarName == nameof(SalesInvoice) && r.RegistrarId == invoiceId).ToListAsync(cancellationToken));

        invoice.IsPosted = false;
        context.Update(invoice);
        return true;
    }

    public async Task<bool> PostPaymentAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var payment = await context.Payments.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null) return false;

            if (!payment.ContractId.HasValue)
            {
                throw new InvalidOperationException($"Платежный документ № {payment.Number} не имеет привязки к договору. Выберите договор в карточке платежа для его проведения.");
            }

            if (payment.IsPosted) await UnpostPaymentInternalAsync(context, paymentId, cancellationToken);

            var movType = payment.Type == PaymentType.Incoming ? MovementType.Expense : MovementType.Receipt;

            context.SettlementLedgers.Add(new SettlementLedger
            {
                Period = payment.Date,
                RegistrarName = nameof(PaymentDocument),
                RegistrarId = payment.Id,
                ContractorId = payment.ContractorId,
                ContractId = payment.ContractId.Value,
                MovementType = movType,
                Amount = payment.Amount
            });

            payment.IsPosted = true;
            context.Update(payment);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }

    public async Task<bool> UnpostPaymentAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await UnpostPaymentInternalAsync(context, paymentId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }

    private async Task<bool> UnpostPaymentInternalAsync(AppDbContext context, int paymentId, CancellationToken cancellationToken)
    {
        var payment = await context.Payments.FindAsync(new object[] { paymentId }, cancellationToken);
        if (payment == null) return false;

        context.SettlementLedgers.RemoveRange(await context.SettlementLedgers.Where(r => r.RegistrarName == nameof(PaymentDocument) && r.RegistrarId == paymentId).ToListAsync(cancellationToken));

        payment.IsPosted = false;
        context.Update(payment);
        return true;
    }

    public async Task<bool> PostPriceSettingAsync(int priceSettingId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var setting = await context.PriceSettings.Include(p => p.Details).FirstOrDefaultAsync(p => p.Id == priceSettingId, cancellationToken);
            if (setting == null) return false;

            if (setting.IsPosted) await UnpostPriceSettingInternalAsync(context, priceSettingId, cancellationToken);

            foreach (var detail in setting.Details)
            {
                context.PriceLedgers.Add(new PriceLedger
                {
                    Period = setting.Date,
                    RegistrarId = setting.Id,
                    NomenclatureId = detail.NomenclatureId,
                    PriceTypeId = detail.PriceTypeId,
                    Price = detail.Price
                });
            }

            setting.IsPosted = true;
            context.Update(setting);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }

    public async Task<bool> UnpostPriceSettingAsync(int priceSettingId, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await UnpostPriceSettingInternalAsync(context, priceSettingId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }

    private async Task<bool> UnpostPriceSettingInternalAsync(AppDbContext context, int priceSettingId, CancellationToken cancellationToken)
    {
        var setting = await context.PriceSettings.FindAsync(new object[] { priceSettingId }, cancellationToken);
        if (setting == null) return false;

        context.PriceLedgers.RemoveRange(await context.PriceLedgers.Where(r => r.RegistrarId == priceSettingId).ToListAsync(cancellationToken));

        setting.IsPosted = false;
        context.Update(setting);
        return true;
    }
}