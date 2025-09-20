using Hackathon_2025_ESG.Data;
using Hackathon_2025_ESG.Models.ERPSample;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hackathon_2025_ESG.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("ERPSystemSample/[action]")]
    public class ERPSystemController : Controller
    {
        private readonly Hackathon_2025_ESGContext _context;
        private readonly ILogger<ERPSystemController> _logger;

        public ERPSystemController(Hackathon_2025_ESGContext context, ILogger<ERPSystemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new ERPSystemViewModel
            {
                Companies = await _context.Company.ToListAsync(),
                Departments = await _context.Department.ToListAsync(),
                Employees = await _context.Employee.ToListAsync(),
                Vendors = await _context.Vendor.ToListAsync(),
                Products = await _context.Product.ToListAsync(),
                Orders = await _context.Order.ToListAsync(),
                OrderDetails = await _context.OrderDetail.ToListAsync(),
                EnvironmentalMetrics = await _context.EnvironmentalMetric.ToListAsync(),
                Compliances = await _context.Compliance.ToListAsync()
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create(string entityType)
        {
            Console.WriteLine("Create action called with entityType = " + entityType);
            ViewBag.EntityType = entityType;
            var model = GetEmptyEntityModel(entityType);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string entityType, object model)
        {
            _logger.LogInformation("Creating new entity of type {EntityType}", entityType);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
            var mappedEntity = MapFormDataToEntity(entityType, formData);

            if (mappedEntity == null)
            {
                return BadRequest("Invalid entity type or form data.");
            }
            _logger.LogInformation("Form data: {@FormData}", formData);

            _context.Add(mappedEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string entityType, string id)
        {
            ViewBag.EntityType = entityType;
            var entity = await FindEntityById(entityType, id);
            if (entity == null)
                return NotFound();

            var model = new DynamicEditViewModel
            {
                EntityType = entityType,
                Id = id,
                Properties = entity.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(entity))
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DynamicEditViewModel viewModel)
        {
            _logger.LogInformation("Edit POST - EntityType: {EntityType}, Id: {Id}", viewModel.EntityType, viewModel.Id);

            foreach (var kvp in viewModel.Properties)
            {
                _logger.LogInformation("Property Key: {Key}, Value: {Value}", kvp.Key, kvp.Value);
            }

            var existingEntity = await FindEntityById(viewModel.EntityType, viewModel.Id);
            if (existingEntity == null)
                return NotFound();

            // Update values
            foreach (var prop in existingEntity.GetType().GetProperties())
            {
                if (prop.Name.ToLower().Contains("id")) continue; // Skip Id

                if (viewModel.Properties.ContainsKey(prop.Name))
                {
                    var value = viewModel.Properties[prop.Name]?.ToString();
                    _logger.LogInformation("Updating Property: {PropertyName} with Value: {Value}", prop.Name, value);

                    if (prop.PropertyType == typeof(string))
                    {
                        if (!string.IsNullOrEmpty(value))
                            prop.SetValue(existingEntity, value);
                        else
                            _logger.LogWarning("Skipping {PropertyName} because value is null or empty.", prop.Name);
                    }
                    else if (prop.PropertyType == typeof(int) && int.TryParse(value, out var intVal))
                        prop.SetValue(existingEntity, intVal);
                    else if (prop.PropertyType == typeof(decimal) && decimal.TryParse(value, out var decVal))
                        prop.SetValue(existingEntity, decVal);
                    else if (prop.PropertyType == typeof(DateTime) && DateTime.TryParse(value, out var dateVal))
                        prop.SetValue(existingEntity, dateVal);
                    else if (prop.PropertyType == typeof(bool))
                        prop.SetValue(existingEntity, value?.ToLower() == "true" || value == "1");
                }
                else
                {
                    _logger.LogWarning("Property {PropertyName} not found in submitted data.", prop.Name);
                }
            }

            _context.Update(existingEntity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string entityType, string id)
        {
            ViewBag.EntityType = entityType;
            object? model = await FindEntityById(entityType, id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string entityType, string id)
        {
            object? model = await FindEntityById(entityType, id);
            if (model == null)
                return NotFound();

            _context.Remove(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private object GetEmptyEntityModel(string entityType)
        {
            return entityType.ToLower() switch
            {
                "company" => new Company(),
                "department" => new Department(),
                "employee" => new Employee(),
                "vendor" => new Vendor(),
                "product" => new Product(),
                "order" => new Order(),
                "orderdetail" => new OrderDetail(),
                "environmentalmetric" => new EnvironmentalMetric(),
                "compliance" => new Compliance(),
                _ => throw new ArgumentException("Invalid entity type.")
            };
        }

        private async Task<object?> FindEntityById(string entityType, string id)
        {
            return entityType.ToLower() switch
            {
                "company" => await _context.Company.FindAsync(id),
                "department" => await _context.Department.FindAsync(id),
                "employee" => await _context.Employee.FindAsync(id),
                "vendor" => await _context.Vendor.FindAsync(id),
                "product" => await _context.Product.FindAsync(id),
                "order" => await _context.Order.FindAsync(id),
                "orderdetail" => await _context.OrderDetail.FindAsync(id),
                "environmentalmetric" => await _context.EnvironmentalMetric.FindAsync(id),
                "compliance" => await _context.Compliance.FindAsync(id),
                _ => null
            };
        }

        private void UpdateEntityFromForm(object entity, Dictionary<string, string> formData)
        {
            var entityType = entity.GetType();
            foreach (var prop in entityType.GetProperties())
            {
                if (!formData.ContainsKey(prop.Name)) continue; // Skip missing fields
                if (prop.Name.ToLower().Contains("id")) continue; // Never overwrite IDs

                var value = formData[prop.Name];

                if (prop.PropertyType == typeof(string))
                    prop.SetValue(entity, value);
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(entity, int.Parse(value));
                else if (prop.PropertyType == typeof(decimal))
                    prop.SetValue(entity, decimal.Parse(value));
                else if (prop.PropertyType == typeof(DateTime))
                    prop.SetValue(entity, DateTime.Parse(value));
                else if (prop.PropertyType == typeof(bool))
                    prop.SetValue(entity, value.ToLower() == "true" || value == "1");
            }
        }

        private object? MapFormDataToEntity(string entityType, Dictionary<string, string> formData)
        {
            return entityType.ToLower() switch
            {
                "company" => new Company
                {
                    CompanyId = Guid.NewGuid().ToString(),
                    Name = formData["Name"],
                    Address = formData["Address"],
                    Country = formData["Country"],
                    Industry = formData["Industry"],
                    CreatedAt = DateTime.UtcNow
                },
                "vendor" => new Vendor
                {
                    VendorId = Guid.NewGuid().ToString(),
                    Name = formData["Name"],
                    Country = formData["Country"],
                    ESGRating = formData["ESGRating"],
                    Active = formData.ContainsKey("Active") && formData["Active"] == "true"
                },
                "department" => new Department
                {
                    DepartmentId = Guid.NewGuid().ToString(),
                    CompanyId = Guid.NewGuid().ToString(),
                    Name = formData["Name"]
                },
                "employee" => new Employee
                {
                    EmployeeId = Guid.NewGuid().ToString(),
                    DepartmentId = Guid.NewGuid().ToString(),
                    Name = formData["Name"],
                    Position = formData["Position"],
                    HireDate = DateTime.Parse(formData["HireDate"]),
                    Salary = decimal.Parse(formData["Salary"])
                },
                "product" => new Product
                {
                    ProductId = Guid.NewGuid().ToString(),
                    Name = formData["Name"],
                    Category = formData["Category"],
                    CarbonFootprint = decimal.Parse(formData["CarbonFootprint"]),
                    WaterUsage = decimal.Parse(formData["WaterUsage"]),
                    VendorId = Guid.NewGuid().ToString()
                },
                "order" => new Order
                {
                    OrderId = Guid.NewGuid().ToString(),
                    CompanyId = formData["CompanyId"],
                    OrderDate = DateTime.Parse(formData["OrderDate"]),
                    TotalAmount = 0
                },
                "orderdetail" => new OrderDetail
                {
                    OrderDetailId = Guid.NewGuid().ToString(),
                    OrderId = formData["OrderId"],
                    ProductId = Guid.NewGuid().ToString(),
                    Quantity = int.Parse(formData["Quantity"]),
                    Price = decimal.Parse(formData["Price"])
                },
                "environmentalmetric" => new EnvironmentalMetric
                {
                    MetricId = Guid.NewGuid().ToString(),
                    CompanyId = Guid.NewGuid().ToString(),
                    MetricDate = DateTime.Parse(formData["MetricDate"]),
                    EnergyUsage = decimal.Parse(formData["EnergyUsage"]),
                    WaterUsage = decimal.Parse(formData["WaterUsage"]),
                    WasteGenerated = decimal.Parse(formData["WasteGenerated"]),
                    Emissions = decimal.Parse(formData["Emissions"])
                },
                "compliance" => new Compliance
                {
                    ComplianceId = Guid.NewGuid().ToString(),
                    CompanyId = Guid.NewGuid().ToString(),
                    Name = formData["Name"],
                    CertificationDate = DateTime.Parse(formData["CertificationDate"]),
                    ExpiryDate = DateTime.Parse(formData["ExpiryDate"]),
                    Status = formData["Status"]
                },
                _ => null
            };
        }

    }

    public class ERPSystemViewModel
    {
        public List<Company>? Companies { get; set; }
        public List<Department>? Departments { get; set; }
        public List<Employee>? Employees { get; set; }
        public List<Vendor>? Vendors { get; set; }
        public List<Product>? Products { get; set; }
        public List<Order>? Orders { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
        public List<EnvironmentalMetric>? EnvironmentalMetrics { get; set; }
        public List<Compliance>? Compliances { get; set; }
    }

    public class DynamicEditViewModel
    {
        public string EntityType { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public Dictionary<string, object?> Properties { get; set; } = new();
    }
}
