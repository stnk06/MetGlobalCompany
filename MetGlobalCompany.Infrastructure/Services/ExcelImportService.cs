using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using MetGlobalCompany.Application.DTOs;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.Domain.Enums;
using MetGlobalCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetGlobalCompany.Infrastructure.Services;

public class ExcelImportService : IExcelImportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ExcelImportService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ImportReportDto> ImportUnitsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var report = new ImportReportDto();
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        var dbUnits = await context.Units.ToListAsync(cancellationToken);
        var existingUnits = new Dictionary<string, Unit>(StringComparer.OrdinalIgnoreCase);
        foreach (var u in dbUnits)
        {
            if (!string.IsNullOrWhiteSpace(u.Name))
            {
                existingUnits[u.Name.Trim()] = u;
            }
        }

        foreach (var row in rows)
        {
            var name = row.Cell(1).GetString().Trim();
            var code = row.Cell(2).GetString().Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Пустое наименование.");
                continue;
            }

            if (existingUnits.TryGetValue(name, out var unit))
            {
                unit.Code = code;
                context.Units.Update(unit);
                report.UpdatedCount++;
            }
            else
            {
                var newUnit = new Unit { Name = name, Code = code };
                await context.Units.AddAsync(newUnit, cancellationToken);
                existingUnits[name] = newUnit;
                report.CreatedCount++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<ImportReportDto> ImportPriceTypesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var report = new ImportReportDto();
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        var dbTypes = await context.PriceTypes.ToListAsync(cancellationToken);
        var existingTypes = new Dictionary<string, PriceType>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in dbTypes)
        {
            if (!string.IsNullOrWhiteSpace(p.Name))
            {
                existingTypes[p.Name.Trim()] = p;
            }
        }

        foreach (var row in rows)
        {
            var name = row.Cell(1).GetString().Trim();
            var currency = row.Cell(2).GetString().Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Пустое наименование.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(currency)) currency = "RUB";

            if (existingTypes.TryGetValue(name, out var priceType))
            {
                priceType.CurrencyCode = currency;
                context.PriceTypes.Update(priceType);
                report.UpdatedCount++;
            }
            else
            {
                var newPriceType = new PriceType { Name = name, CurrencyCode = currency, IsIncludesVat = true };
                await context.PriceTypes.AddAsync(newPriceType, cancellationToken);
                existingTypes[name] = newPriceType;
                report.CreatedCount++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<ImportReportDto> ImportContractorsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var report = new ImportReportDto();
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        var dbContractors = await context.Contractors.ToListAsync(cancellationToken);
        var existingContractors = new Dictionary<string, Contractor>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in dbContractors)
        {
            if (!string.IsNullOrWhiteSpace(c.Inn))
            {
                existingContractors[c.Inn.Trim()] = c;
            }
        }

        foreach (var row in rows)
        {
            var shortName = row.Cell(1).GetString().Trim();
            var fullName = row.Cell(2).GetString().Trim();
            var typeStr = row.Cell(3).GetString().Trim();
            var inn = row.Cell(4).GetString().Trim();
            var kpp = row.Cell(5).GetString().Trim();
            var ogrn = row.Cell(6).GetString().Trim();
            var legalAddress = row.Cell(7).GetString().Trim();
            var physicalAddress = row.Cell(8).GetString().Trim();
            var contactPerson = row.Cell(9).GetString().Trim();
            var phone = row.Cell(10).GetString().Trim();
            var email = row.Cell(11).GetString().Trim();

            if (string.IsNullOrWhiteSpace(inn))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Пустой ИНН.");
                continue;
            }

            var type = ContractorType.Buyer;
            if (typeStr.Contains("Покупатель и Поставщик")) type = ContractorType.Both;
            else if (typeStr.Contains("Поставщик")) type = ContractorType.Supplier;

            if (existingContractors.TryGetValue(inn, out var contractor))
            {
                contractor.Name = string.IsNullOrWhiteSpace(shortName) ? contractor.Name : shortName;
                contractor.FullName = fullName;
                contractor.Type = type;
                contractor.Kpp = kpp;
                contractor.Ogrn = ogrn;
                contractor.LegalAddress = legalAddress;
                contractor.PhysicalAddress = physicalAddress;
                contractor.ContactPerson = contactPerson;
                contractor.Phone = phone;
                contractor.Email = email;
                context.Contractors.Update(contractor);
                report.UpdatedCount++;
            }
            else
            {
                var newContractor = new Contractor
                {
                    Name = string.IsNullOrWhiteSpace(shortName) ? "Без названия" : shortName,
                    FullName = fullName,
                    Type = type,
                    Inn = inn,
                    Kpp = kpp,
                    Ogrn = ogrn,
                    LegalAddress = legalAddress,
                    PhysicalAddress = physicalAddress,
                    ContactPerson = contactPerson,
                    Phone = phone,
                    Email = email
                };
                await context.Contractors.AddAsync(newContractor, cancellationToken);
                existingContractors[inn] = newContractor;
                report.CreatedCount++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<ImportReportDto> ImportNomenclaturesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var report = new ImportReportDto();
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        var dbNomenclatures = await context.Nomenclatures.ToListAsync(cancellationToken);
        var existingNomenclatures = new Dictionary<string, Nomenclature>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in dbNomenclatures)
        {
            if (!string.IsNullOrWhiteSpace(n.Name))
            {
                existingNomenclatures[n.Name.Trim()] = n;
            }
        }

        var dbUnits = await context.Units.ToListAsync(cancellationToken);
        var units = new Dictionary<string, Unit>(StringComparer.OrdinalIgnoreCase);
        foreach (var u in dbUnits)
        {
            if (!string.IsNullOrWhiteSpace(u.Name))
            {
                units[u.Name.Trim()] = u;
            }
        }

        foreach (var row in rows)
        {
            var name = row.Cell(1).GetString().Trim();
            var article = row.Cell(2).GetString().Trim();
            var unitName = row.Cell(3).GetString().Trim();
            var gost = row.Cell(4).GetString().Trim();
            var density = row.Cell(5).GetString().Trim();
            var isServiceStr = row.Cell(6).GetString().Trim().ToLower();

            if (string.IsNullOrWhiteSpace(name))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Пустое наименование.");
                continue;
            }

            if (!units.TryGetValue(unitName, out var unit))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Единица измерения '{unitName}' не найдена.");
                continue;
            }

            bool isService = isServiceStr == "да" || isServiceStr == "yes" || isServiceStr == "true" || isServiceStr == "1";

            if (existingNomenclatures.TryGetValue(name, out var nom))
            {
                nom.Article = article;
                nom.UnitId = unit.Id;
                nom.Gost = gost;
                nom.Density = density;
                nom.IsService = isService;
                context.Nomenclatures.Update(nom);
                report.UpdatedCount++;
            }
            else
            {
                var newNom = new Nomenclature
                {
                    Name = name,
                    Article = article,
                    UnitId = unit.Id,
                    Gost = gost,
                    Density = density,
                    IsService = isService
                };
                await context.Nomenclatures.AddAsync(newNom, cancellationToken);
                existingNomenclatures[name] = newNom;
                report.CreatedCount++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<ImportReportDto> ImportContractsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var report = new ImportReportDto();
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        var dbContracts = await context.Contracts.ToListAsync(cancellationToken);
        var existingContracts = new Dictionary<string, Contract>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in dbContracts)
        {
            if (!string.IsNullOrWhiteSpace(c.Number))
            {
                existingContracts[c.Number.Trim()] = c;
            }
        }

        var dbContractors = await context.Contractors.ToListAsync(cancellationToken);
        var contractors = new Dictionary<string, Contractor>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in dbContractors)
        {
            if (!string.IsNullOrWhiteSpace(c.Inn))
            {
                contractors[c.Inn.Trim()] = c;
            }
        }

        var dbTypes = await context.PriceTypes.ToListAsync(cancellationToken);
        var priceTypes = new Dictionary<string, PriceType>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in dbTypes)
        {
            if (!string.IsNullOrWhiteSpace(p.Name))
            {
                priceTypes[p.Name.Trim()] = p;
            }
        }

        foreach (var row in rows)
        {
            var number = row.Cell(1).GetString().Trim();
            var dateStr = row.Cell(2).GetString().Trim();
            var inn = row.Cell(3).GetString().Trim();
            var priceTypeName = row.Cell(4).GetString().Trim();

            if (string.IsNullOrWhiteSpace(number))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Пустой номер договора.");
                continue;
            }

            if (!contractors.TryGetValue(inn, out var contractor))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Контрагент с ИНН '{inn}' не найден.");
                continue;
            }

            int? priceTypeId = null;
            if (!string.IsNullOrWhiteSpace(priceTypeName) && priceTypes.TryGetValue(priceTypeName, out var pt))
            {
                priceTypeId = pt.Id;
            }

            DateTime date = DateTime.Today;
            var dateCell = row.Cell(2);
            if (dateCell.TryGetValue<DateTime>(out var d)) date = d.Date;
            else if (DateTime.TryParse(dateStr, out var pd)) date = pd.Date;

            if (existingContracts.TryGetValue(number, out var contract))
            {
                contract.Date = date;
                contract.ContractorId = contractor.Id;
                contract.PriceTypeId = priceTypeId;
                context.Contracts.Update(contract);
                report.UpdatedCount++;
            }
            else
            {
                var newContract = new Contract
                {
                    Number = number,
                    Date = date,
                    ContractorId = contractor.Id,
                    PriceTypeId = priceTypeId
                };
                await context.Contracts.AddAsync(newContract, cancellationToken);
                existingContracts[number] = newContract;
                report.CreatedCount++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<ImportReportDto> ImportPriceSettingsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var report = new ImportReportDto();
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        var dbNomenclatures = await context.Nomenclatures.ToListAsync(cancellationToken);
        var nomenclatures = new Dictionary<string, Nomenclature>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in dbNomenclatures)
        {
            if (!string.IsNullOrWhiteSpace(n.Name))
            {
                nomenclatures[n.Name.Trim()] = n;
            }
        }

        var dbTypes = await context.PriceTypes.ToListAsync(cancellationToken);
        var priceTypes = new Dictionary<string, PriceType>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in dbTypes)
        {
            if (!string.IsNullOrWhiteSpace(p.Name))
            {
                priceTypes[p.Name.Trim()] = p;
            }
        }

        var existingDocs = await context.PriceSettings.Include(p => p.Details).ToListAsync(cancellationToken);

        var groupedRows = rows.GroupBy(r =>
        {
            var cell = r.Cell(1);
            if (cell.TryGetValue<DateTime>(out var d)) return d.Date;
            if (DateTime.TryParse(cell.GetString().Trim(), out var pd)) return pd.Date;
            return DateTime.Today;
        });

        foreach (var group in groupedRows)
        {
            var date = group.Key;
            var doc = existingDocs.FirstOrDefault(d => d.Date.Date == date);
            bool isNew = false;

            if (doc == null)
            {
                doc = new PriceSetting { Date = date, Details = new List<PriceSettingDetail>() };
                isNew = true;
            }

            foreach (var row in group)
            {
                var nomName = row.Cell(2).GetString().Trim();
                var ptName = row.Cell(3).GetString().Trim();
                var priceStr = row.Cell(4).GetString().Trim().Replace(',', '.');

                if (!nomenclatures.TryGetValue(nomName, out var nom))
                {
                    report.ErrorsCount++;
                    report.Errors.Add($"Строка {row.RowNumber()}: Номенклатура '{nomName}' не найдена.");
                    continue;
                }

                if (!priceTypes.TryGetValue(ptName, out var pt))
                {
                    report.ErrorsCount++;
                    report.Errors.Add($"Строка {row.RowNumber()}: Тип цены '{ptName}' не найден.");
                    continue;
                }

                if (!decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price))
                {
                    report.ErrorsCount++;
                    report.Errors.Add($"Строка {row.RowNumber()}: Неверный формат цены.");
                    continue;
                }

                var existingItem = doc.Details.FirstOrDefault(i => i.NomenclatureId == nom.Id && i.PriceTypeId == pt.Id);
                if (existingItem != null)
                {
                    existingItem.Price = price;
                }
                else
                {
                    doc.Details.Add(new PriceSettingDetail { NomenclatureId = nom.Id, PriceTypeId = pt.Id, Price = price });
                }
            }

            if (isNew)
            {
                await context.PriceSettings.AddAsync(doc, cancellationToken);
                existingDocs.Add(doc);
                report.CreatedCount++;
            }
            else
            {
                context.PriceSettings.Update(doc);
                report.UpdatedCount++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<ImportReportDto> ImportPaymentsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var report = new ImportReportDto();
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        var dbContractors = await context.Contractors.ToListAsync(cancellationToken);
        var contractors = new Dictionary<string, Contractor>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in dbContractors)
        {
            if (!string.IsNullOrWhiteSpace(c.Inn))
            {
                contractors[c.Inn.Trim()] = c;
            }
        }

        var dbContracts = await context.Contracts.ToListAsync(cancellationToken);
        var contracts = new Dictionary<string, Contract>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in dbContracts)
        {
            if (!string.IsNullOrWhiteSpace(c.Number))
            {
                contracts[c.Number.Trim()] = c;
            }
        }

        var existingPayments = await context.Payments.ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            var number = row.Cell(1).GetString().Trim();
            var dateCell = row.Cell(2);
            DateTime date = DateTime.Today;
            if (dateCell.TryGetValue<DateTime>(out var d)) date = d.Date;
            else if (DateTime.TryParse(dateCell.GetString().Trim(), out var pd)) date = pd.Date;

            var typeStr = row.Cell(3).GetString().Trim().ToLower();
            var inn = row.Cell(4).GetString().Trim();
            var contractNum = row.Cell(5).GetString().Trim();
            var amountStr = row.Cell(6).GetString().Trim().Replace(',', '.');
            var purpose = row.Cell(7).GetString().Trim();

            if (string.IsNullOrWhiteSpace(number))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Пустой номер ПП.");
                continue;
            }

            if (!contractors.TryGetValue(inn, out var contractor))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Контрагент с ИНН '{inn}' не найден.");
                continue;
            }

            int? contractId = null;
            if (!string.IsNullOrWhiteSpace(contractNum) && contracts.TryGetValue(contractNum, out var contract))
            {
                contractId = contract.Id;
            }

            var type = typeStr.Contains("исходящий") ? PaymentType.Outgoing : PaymentType.Incoming;

            if (!decimal.TryParse(amountStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
            {
                report.ErrorsCount++;
                report.Errors.Add($"Строка {row.RowNumber()}: Неверный формат суммы.");
                continue;
            }

            var payment = existingPayments.FirstOrDefault(p => p.Number == number && p.Date.Date == date);
            if (payment != null)
            {
                payment.Type = type;
                payment.ContractorId = contractor.Id;
                payment.ContractId = contractId;
                payment.Amount = amount;
                payment.Purpose = purpose;
                context.Payments.Update(payment);
                report.UpdatedCount++;
            }
            else
            {
                var newPayment = new PaymentDocument
                {
                    Number = number,
                    Date = date,
                    Type = type,
                    ContractorId = contractor.Id,
                    ContractId = contractId,
                    Amount = amount,
                    Purpose = purpose
                };
                await context.Payments.AddAsync(newPayment, cancellationToken);
                existingPayments.Add(newPayment);
                report.CreatedCount++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return report;
    }
}