using System;
using System.Linq;
using System.Text;
using System.Windows;
using MetGlobalCompany.Application.Interfaces;
using MetGlobalCompany.Infrastructure.Data;
using MetGlobalCompany.Infrastructure.Repositories;
using MetGlobalCompany.Infrastructure.Services;
using MetGlobalCompany.WPF.Services;
using MetGlobalCompany.WPF.ViewModels;
using MetGlobalCompany.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MetGlobalCompany.WPF;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=MetGlobalCompanyDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
                services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));

                services.AddSingleton<ICurrentUserService, CurrentUserService>();
                services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));

                services.AddHttpClient<IDadataService, DadataService>();
                services.AddTransient<IDocumentPostingService, DocumentPostingService>();
                services.AddTransient<IExportService, ExportService>();
                services.AddTransient<IBankStatementService, BankStatementService>();
                services.AddTransient<ISalesAnalyticsService, SalesAnalyticsService>();
                services.AddTransient<IWordExportService, WordExportService>();
                services.AddTransient<IPriceService, PriceService>();
                services.AddTransient<IExcelImportService, ExcelImportService>();

                services.AddSingleton<IDialogService, DialogService>();

                services.AddSingleton<MainViewModel>();
                services.AddSingleton<SalesDashboardViewModel>();
                services.AddSingleton<ContractorsViewModel>();
                services.AddSingleton<ContractsViewModel>();
                services.AddSingleton<NomenclatureViewModel>();
                services.AddSingleton<SalesInvoicesViewModel>();
                services.AddSingleton<PurchaseInvoicesViewModel>();
                services.AddSingleton<OrdersViewModel>();
                services.AddSingleton<PaymentsViewModel>();
                services.AddSingleton<AllDocumentsViewModel>();
                services.AddSingleton<PriceTypesViewModel>();
                services.AddSingleton<PriceSettingsViewModel>();

                services.AddTransient<ContractorFormViewModel>();
                services.AddTransient<ContractFormViewModel>();
                services.AddTransient<NomenclatureCategoryFormViewModel>();
                services.AddTransient<NomenclatureFormViewModel>();
                services.AddTransient<OrderFormViewModel>();
                services.AddTransient<SalesInvoiceFormViewModel>();
                services.AddTransient<PurchaseInvoiceFormViewModel>();
                services.AddTransient<PaymentFormViewModel>();
                services.AddTransient<Torg12PreviewViewModel>();
                services.AddTransient<PriceTypeFormViewModel>();
                services.AddTransient<PriceSettingFormViewModel>();
                services.AddTransient<ContractorSelectViewModel>();
                services.AddTransient<ContractSelectViewModel>();
                services.AddTransient<NomenclatureSelectViewModel>();
                services.AddTransient<OrderDetailFormViewModel>();
                services.AddTransient<SalesInvoiceDetailFormViewModel>();
                services.AddTransient<PurchaseInvoiceDetailFormViewModel>();
                services.AddTransient<PriceSettingDetailFormViewModel>();
                services.AddTransient<ImportViewModel>();
                services.AddTransient<HelpViewModel>();

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        using (var scope = _host.Services.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var dbContext = await dbFactory.CreateDbContextAsync();
            await dbContext.Database.EnsureCreatedAsync();
            await DbInitializer.SeedAsync(dbContext);
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }
}