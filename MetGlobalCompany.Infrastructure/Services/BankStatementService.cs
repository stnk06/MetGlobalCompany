using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.Domain.Enums;
using MetGlobalCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetGlobalCompany.Infrastructure.Services;

public class BankStatementService : IBankStatementService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public BankStatementService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<int> ImportFrom1CFormatAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var lines = await File.ReadAllLinesAsync(filePath, Encoding.GetEncoding(1251), cancellationToken);
        var importedCount = 0;

        bool inDocument = false;
        var docData = new Dictionary<string, string>();

        var contractors = await context.Contractors.Include(c => c.Contracts).ToListAsync(cancellationToken);
        var existingPayments = await context.Payments.Select(p => p.Number).ToListAsync(cancellationToken);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("СекцияДокумент=Платежное поручение") || line.StartsWith("СекцияДокумент=Платежный ордер"))
            {
                inDocument = true;
                docData.Clear();
                continue;
            }

            if (line.StartsWith("КонецДокумента") && inDocument)
            {
                inDocument = false;

                if (!docData.ContainsKey("Номер") || !docData.ContainsKey("Сумма")) continue;

                var number = docData["Номер"];
                if (existingPayments.Contains(number)) continue;

                var dateStr = docData.ContainsKey("Дата") ? docData["Дата"] : DateTime.Now.ToString("dd.MM.yyyy");
                if (!DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    date = DateTime.Now;

                var amountStr = docData["Сумма"].Replace(".", ",");
                if (!decimal.TryParse(amountStr, out var amount)) continue;

                var payerInn = docData.ContainsKey("ПлательщикИНН") ? docData["ПлательщикИНН"] : string.Empty;
                var payeeInn = docData.ContainsKey("ПолучательИНН") ? docData["ПолучательИНН"] : string.Empty;

                Contractor? contractor = null;
                PaymentType type = PaymentType.Incoming;

                var payerContractor = contractors.FirstOrDefault(c => c.Inn == payerInn);
                if (payerContractor != null)
                {
                    contractor = payerContractor;
                    type = PaymentType.Incoming;
                }
                else
                {
                    var payeeContractor = contractors.FirstOrDefault(c => c.Inn == payeeInn);
                    if (payeeContractor != null)
                    {
                        contractor = payeeContractor;
                        type = PaymentType.Outgoing;
                    }
                }

                if (contractor == null) continue;

                var contract = contractor.Contracts.FirstOrDefault(c => c.IsActive);

                var payment = new PaymentDocument
                {
                    Date = date,
                    Number = number,
                    Amount = amount,
                    Type = type,
                    Purpose = docData.ContainsKey("НазначениеПлатежа") ? docData["НазначениеПлатежа"] : string.Empty,
                    ContractorId = contractor.Id,
                    ContractId = contract?.Id,
                    IsPosted = false
                };

                await context.Payments.AddAsync(payment, cancellationToken);
                existingPayments.Add(number);
                importedCount++;
            }

            if (inDocument)
            {
                var separatorIndex = line.IndexOf('=');
                if (separatorIndex > 0)
                {
                    var key = line.Substring(0, separatorIndex);
                    var value = line.Substring(separatorIndex + 1);
                    docData[key] = value;
                }
            }
        }

        if (importedCount > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return importedCount;
    }
}