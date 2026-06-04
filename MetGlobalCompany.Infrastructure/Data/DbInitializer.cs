using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetGlobalCompany.Domain.Entities;
using MetGlobalCompany.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MetGlobalCompany.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly Random Rnd = new Random(42);

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Contractors.AnyAsync()) return;

        var startDate = new DateTime(2026, 5, 20, 8, 0, 0);
        var endDate = new DateTime(2026, 5, 27, 18, 0, 0);

        DateTime GetRandomDate()
        {
            var range = (endDate - startDate).TotalMinutes;
            return startDate.AddMinutes(Rnd.NextDouble() * range);
        }

        var units = new[]
        {
            new Unit { Name = "т" },
            new Unit { Name = "кг" },
            new Unit { Name = "шт" },
            new Unit { Name = "пог. м" },
            new Unit { Name = "усл. ед" }
        };
        await context.Units.AddRangeAsync(units);

        var categories = new[]
        {
            new NomenclatureCategory { Name = "Сортовой прокат" },
            new NomenclatureCategory { Name = "Листовой прокат" },
            new NomenclatureCategory { Name = "Трубный прокат" },
            new NomenclatureCategory { Name = "Метизы и проволока" },
            new NomenclatureCategory { Name = "Услуги" }
        };
        await context.NomenclatureCategories.AddRangeAsync(categories);

        var priceTypes = new[]
        {
            new PriceType { Name = "Оптовая", CurrencyCode = "RUB", IsIncludesVat = true },
            new PriceType { Name = "Крупный опт", CurrencyCode = "RUB", IsIncludesVat = true },
            new PriceType { Name = "Закупочная", CurrencyCode = "RUB", IsIncludesVat = true }
        };
        await context.PriceTypes.AddRangeAsync(priceTypes);

        var contractorsNames = new[]
        {
            "ПАО Северсталь", "ПАО НЛМК", "ПАО ММК", "ЕВРАЗ НТМК", "ПАО Мечел", "ТМК", "ОМК", "АО Загорский трубный завод", "ЧТПЗ", "УГМК",
            "ООО Альфа-Строй", "АО Монолит", "ООО СпецМонтаж", "ООО СтройИнвест", "ПАО ПИК", "ООО Самолет", "АО ЛСР", "ООО ФСК", "ГК Инград", "ООО Гранель",
            "ООО МеталлТрейд", "АО СпецСталь", "ООО ПромКомплект", "ЗАО РегионСнаб", "ООО Вектор", "ООО Омега", "АО Сигма", "ООО ЭнергоСтрой", "ПАО Газпром", "АО Роснефть",
            "ООО ТрансНефть", "ЗАО Лукойл", "ООО МеталлИнвест", "ООО СтальПром", "АО ТехноСфера", "ООО Инновация", "ООО СтройМастер", "ООО ГлобалМеталл", "ООО МеталлСервис", "ООО Авангард",
            "ООО СоюзМеталл", "АО Арматура-Центр", "ООО ТрубПром", "ООО Стандарт", "ООО Магистраль", "ООО Каркас", "ООО Монолит-Капитал", "ООО МетПромКомплект", "АО СевЗапМеталл", "ООО УралСибТрейд"
        };

        var contractors = new List<Contractor>();
        for (int i = 0; i < 50; i++)
        {
            contractors.Add(new Contractor
            {
                Name = contractorsNames[i],
                FullName = $"Общество '{contractorsNames[i]}'",
                Inn = Rnd.Next(100000000, 999999999).ToString() + Rnd.Next(1, 9).ToString(),
                Kpp = "770101001",
                Type = i < 10 ? ContractorType.Supplier : ContractorType.Buyer,
                Phone = $"+7 (495) {Rnd.Next(100, 999)}-{Rnd.Next(10, 99)}-{Rnd.Next(10, 99)}"
            });
        }
        await context.Contractors.AddRangeAsync(contractors);

        var contracts = new List<Contract>();
        for (int i = 0; i < 50; i++)
        {
            var contractor = contractors[i];
            var pType = contractor.Type == ContractorType.Supplier ? priceTypes[2] : (i % 2 == 0 ? priceTypes[0] : priceTypes[1]);

            contracts.Add(new Contract
            {
                Number = $"ДОГ-{2026}-{i + 1:D3}",
                Date = GetRandomDate().AddDays(-10),
                Name = contractor.Type == ContractorType.Supplier ? "Договор поставки" : "Договор реализации",
                IsActive = true,
                Contractor = contractor,
                PriceType = pType
            });
        }
        await context.Contracts.AddRangeAsync(contracts);

        var nomNames = new[]
        {
            "Арматура А500С 6мм", "Арматура А500С 8мм", "Арматура А500С 10мм", "Арматура А500С 12мм", "Арматура А500С 14мм", "Арматура А500С 16мм", "Арматура А500С 18мм", "Арматура А500С 20мм", "Арматура А500С 22мм", "Арматура А500С 25мм",
            "Труба проф. 20х20х2", "Труба проф. 40х20х2", "Труба проф. 40х40х2", "Труба проф. 50х50х3", "Труба проф. 60х40х3", "Труба проф. 60х60х3", "Труба проф. 80х80х4", "Труба проф. 100х100х4", "Труба проф. 120х120х5", "Труба проф. 150х150х6",
            "Лист г/к 2мм", "Лист г/к 3мм", "Лист г/к 4мм", "Лист г/к 5мм", "Лист г/к 6мм", "Лист г/к 8мм", "Лист г/к 10мм", "Лист г/к 12мм", "Лист г/к 16мм", "Лист г/к 20мм",
            "Балка двутавр 10Б1", "Балка двутавр 12Б1", "Балка двутавр 16Б1", "Балка двутавр 20Б1", "Балка двутавр 25Б1", "Балка двутавр 30Б1", "Балка двутавр 35Б1", "Балка двутавр 40Б1", "Балка двутавр 20Ш1", "Балка двутавр 30Ш1",
            "Швеллер 10П", "Швеллер 12П", "Швеллер 14П", "Швеллер 16П", "Швеллер 20П", "Уголок 25х25х4", "Уголок 40х40х4", "Уголок 50х50х5", "Уголок 75х75х6", "Уголок 100х100х8"
        };

        var nomenclatures = new List<Nomenclature>();
        for (int i = 0; i < 50; i++)
        {
            var catIndex = i < 10 ? 0 : (i < 20 ? 2 : (i < 30 ? 1 : 0));
            nomenclatures.Add(new Nomenclature
            {
                Name = nomNames[i],
                Article = $"АРТ-{i + 1:D4}",
                Category = categories[catIndex],
                Unit = units[0],
                IsService = false
            });
        }
        await context.Nomenclatures.AddRangeAsync(nomenclatures);

        var priceSettings = new List<PriceSetting>();
        for (int i = 0; i < 50; i++)
        {
            var settingDate = GetRandomDate().AddDays(-5);
            var setting = new PriceSetting
            {
                Number = $"УЦ-{i + 1:D5}",
                Date = settingDate,
                Comment = "Плановая переоценка прайс-листа",
                IsPosted = true, // ПРОВЕДЕНО
                Details = new List<PriceSettingDetail>()
            };

            var itemsCount = Rnd.Next(3, 8);
            for (int j = 0; j < itemsCount; j++)
            {
                var nom = nomenclatures[Rnd.Next(nomenclatures.Count)];
                var pt = priceTypes[Rnd.Next(priceTypes.Length)];
                var basePrice = Rnd.Next(40000, 120000);

                if (!setting.Details.Any(d => d.NomenclatureId == nom.Id && d.PriceTypeId == pt.Id))
                {
                    setting.Details.Add(new PriceSettingDetail
                    {
                        Nomenclature = nom,
                        PriceType = pt,
                        Price = pt.Name == "Закупочная" ? basePrice * 0.8m : basePrice
                    });
                }
            }
            priceSettings.Add(setting);
        }
        await context.PriceSettings.AddRangeAsync(priceSettings);

        var purchaseInvoices = new List<PurchaseInvoice>();
        for (int i = 0; i < 50; i++)
        {
            var supplierContract = contracts.Where(c => c.Contractor.Type == ContractorType.Supplier).OrderBy(x => Rnd.Next()).First();
            var docDate = GetRandomDate();

            var invoice = new PurchaseInvoice
            {
                Number = $"ПТУ-{docDate:yyyyMMdd}-{i + 1:D3}",
                Date = docDate,
                Contractor = supplierContract.Contractor,
                Contract = supplierContract,
                IsPosted = true, // ПРОВЕДЕНО
                Details = new List<PurchaseInvoiceDetail>()
            };

            var itemsCount = Rnd.Next(2, 6);
            decimal total = 0;
            for (int j = 0; j < itemsCount; j++)
            {
                var nom = nomenclatures[Rnd.Next(nomenclatures.Count)];
                var qty = Rnd.Next(500, 2000); // Огромное количество для предотвращения минусовых остатков
                var price = Rnd.Next(45000, 75000);
                var sum = qty * price;

                if (!invoice.Details.Any(d => d.NomenclatureId == nom.Id))
                {
                    invoice.Details.Add(new PurchaseInvoiceDetail { Nomenclature = nom, Quantity = qty, Price = price, Sum = sum });
                    total += sum;
                }
            }
            invoice.TotalAmount = total;
            purchaseInvoices.Add(invoice);
        }
        await context.PurchaseInvoices.AddRangeAsync(purchaseInvoices);

        var orders = new List<Order>();
        var salesInvoices = new List<SalesInvoice>();
        var payments = new List<PaymentDocument>();

        for (int i = 0; i < 50; i++)
        {
            var buyerContract = contracts.Where(c => c.Contractor.Type == ContractorType.Buyer).OrderBy(x => Rnd.Next()).First();
            var docDate = GetRandomDate();

            var order = new Order
            {
                Number = $"ЗК-{docDate:yyyyMMdd}-{i + 1:D3}",
                Date = docDate,
                Contractor = buyerContract.Contractor,
                Contract = buyerContract,
                Status = "Отгружен",
                IsPosted = true, // ПРОВЕДЕНО
                OrderDetails = new List<OrderDetail>()
            };

            var itemsCount = Rnd.Next(1, 5);
            decimal total = 0;
            for (int j = 0; j < itemsCount; j++)
            {
                var nom = nomenclatures[Rnd.Next(nomenclatures.Count)];
                var qty = Rnd.Next(5, 50);
                var price = Rnd.Next(65000, 95000);
                var sum = qty * price;

                if (!order.OrderDetails.Any(d => d.NomenclatureId == nom.Id))
                {
                    order.OrderDetails.Add(new OrderDetail { Nomenclature = nom, Quantity = qty, Price = price, Sum = sum });
                    total += sum;
                }
            }
            order.TotalAmount = total;
            orders.Add(order);

            var invoice = new SalesInvoice
            {
                Number = $"РТУ-{docDate:yyyyMMdd}-{i + 1:D3}",
                Date = docDate.AddHours(2),
                Contractor = buyerContract.Contractor,
                Contract = buyerContract,
                BaseOrder = order,
                IsPosted = true, // ПРОВЕДЕНО
                TotalAmount = total,
                Details = order.OrderDetails.Select(od => new SalesInvoiceDetail
                {
                    Nomenclature = od.Nomenclature,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    Sum = od.Sum
                }).ToList()
            };
            salesInvoices.Add(invoice);

            payments.Add(new PaymentDocument
            {
                Number = $"ВХ-{docDate:yyyyMMdd}-{i + 1:D3}",
                Date = docDate.AddDays(-1),
                Amount = total,
                Type = PaymentType.Incoming,
                Contractor = buyerContract.Contractor,
                Contract = buyerContract,
                Purpose = $"Оплата по счету № {order.Number}",
                IsPosted = true // ПРОВЕДЕНО
            });

            var supplierContract = contracts.Where(c => c.Contractor.Type == ContractorType.Supplier).OrderBy(x => Rnd.Next()).First();
            payments.Add(new PaymentDocument
            {
                Number = $"ИСХ-{docDate:yyyyMMdd}-{i + 1:D3}",
                Date = docDate,
                Amount = Rnd.Next(100000, 5000000),
                Type = PaymentType.Outgoing,
                Contractor = supplierContract.Contractor,
                Contract = supplierContract,
                Purpose = "Оплата поставщику за металлопрокат",
                IsPosted = true // ПРОВЕДЕНО
            });
        }
        await context.Orders.AddRangeAsync(orders);
        await context.SalesInvoices.AddRangeAsync(salesInvoices);
        await context.Payments.AddRangeAsync(payments);

        // 1 ФАЗА: Сохраняем документы, чтобы Entity Framework присвоил им первичные ключи (Id)
        await context.SaveChangesAsync();

        // 2 ФАЗА: Создание движений по регистрам накопления для проведенных документов

        var priceLedgers = new List<PriceLedger>();
        foreach (var ps in priceSettings)
        {
            foreach (var d in ps.Details)
            {
                priceLedgers.Add(new PriceLedger
                {
                    Period = ps.Date,
                    RegistrarId = ps.Id,
                    NomenclatureId = d.Nomenclature.Id,
                    PriceTypeId = d.PriceType.Id,
                    Price = d.Price
                });
            }
        }

        var inventoryLedgers = new List<InventoryLedger>();
        var settlementLedgers = new List<SettlementLedger>();

        foreach (var pi in purchaseInvoices)
        {
            foreach (var d in pi.Details)
            {
                if (!d.Nomenclature.IsService)
                {
                    inventoryLedgers.Add(new InventoryLedger
                    {
                        Period = pi.Date,
                        RegistrarName = nameof(PurchaseInvoice),
                        RegistrarId = pi.Id,
                        NomenclatureId = d.Nomenclature.Id,
                        MovementType = MovementType.Receipt,
                        Quantity = d.Quantity
                    });
                }
            }
            settlementLedgers.Add(new SettlementLedger
            {
                Period = pi.Date,
                RegistrarName = nameof(PurchaseInvoice),
                RegistrarId = pi.Id,
                ContractorId = pi.Contractor.Id,
                ContractId = pi.Contract.Id,
                MovementType = MovementType.Expense,
                Amount = pi.TotalAmount
            });
        }

        foreach (var si in salesInvoices)
        {
            foreach (var d in si.Details)
            {
                if (!d.Nomenclature.IsService)
                {
                    inventoryLedgers.Add(new InventoryLedger
                    {
                        Period = si.Date,
                        RegistrarName = nameof(SalesInvoice),
                        RegistrarId = si.Id,
                        NomenclatureId = d.Nomenclature.Id,
                        MovementType = MovementType.Expense,
                        Quantity = d.Quantity
                    });
                }
            }
            settlementLedgers.Add(new SettlementLedger
            {
                Period = si.Date,
                RegistrarName = nameof(SalesInvoice),
                RegistrarId = si.Id,
                ContractorId = si.Contractor.Id,
                ContractId = si.Contract.Id,
                MovementType = MovementType.Receipt,
                Amount = si.TotalAmount
            });
        }

        foreach (var pay in payments)
        {
            var movType = pay.Type == PaymentType.Incoming ? MovementType.Expense : MovementType.Receipt;
            settlementLedgers.Add(new SettlementLedger
            {
                Period = pay.Date,
                RegistrarName = nameof(PaymentDocument),
                RegistrarId = pay.Id,
                ContractorId = pay.Contractor.Id,
                ContractId = pay.Contract.Id,
                MovementType = movType,
                Amount = pay.Amount
            });
        }

        await context.PriceLedgers.AddRangeAsync(priceLedgers);
        await context.InventoryLedgers.AddRangeAsync(inventoryLedgers);
        await context.SettlementLedgers.AddRangeAsync(settlementLedgers);

        // 3 ФАЗА: Сохраняем движения регистров
        await context.SaveChangesAsync();
    }
}