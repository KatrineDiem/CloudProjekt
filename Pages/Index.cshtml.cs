using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Data.Tables;
using IBAS_kantine.Models; 
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IBAS_kantine.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly string _connectionString;
        private TableClient _tableClient;

        // Liste til at opbevare kantinens menu
        public List<MenuItems> MenuItems { get; set; }

        
        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration["AzureStorageConnectionString"];  
            _tableClient = new TableClient(_connectionString, "EmkatCloudProjekt"); 
            MenuItems = new List<MenuItems>(); 
        }

        // OnGetAsync metode, der henter data fra Azure Table Storage
        public async Task OnGetAsync()
{
    // Hent alle rækker fra tabellen
    var entities = _tableClient.QueryAsync<TableEntity>();

    // Liste med ugedage i den ønskede rækkefølge
    var weekDaysOrder = new List<string> { "Man", "Tirs", "Ons", "Tor", "Fre" };

    // Liste til at opbevare menu items
    var menuItemsList = new List<MenuItems>();

    await foreach (var entity in entities)
    {
        try
        {
            string koldRet = entity.GetString("KoldRet");
            string varmRet = entity.GetString("VarmRet");

            var menuItems = new MenuItems
            {
                Day = entity.RowKey,  
                KoldRet = koldRet,    
                VarmRet = varmRet   
            };

            menuItemsList.Add(menuItems);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved hentning af data for RowKey {entity.RowKey}: {ex.Message}");
        }
    }

    // Sortér menuItems efter rækkefølgen af ugedage
    MenuItems = menuItemsList.OrderBy(item => weekDaysOrder.IndexOf(item.Day)).ToList();
}

    }
}




    
