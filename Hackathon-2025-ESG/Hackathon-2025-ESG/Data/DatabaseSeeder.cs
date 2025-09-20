using Hackathon_2025_ESG.Models.ERPSample;
using Microsoft.EntityFrameworkCore;

namespace Hackathon_2025_ESG.Data
{
    public static class DatabaseSeeder
    {
        private static Random _random = new Random();

        public static async Task SeedAsync(Hackathon_2025_ESGContext context)
        {
            if (context.Company.Any() || context.Vendor.Any() || context.Product.Any())
            {
                // Skip seeding if data already exists
                return;
            }

            // ========== VENDORS ==========
            var vendors = new List<Vendor>
            {
                new() { VendorId = "V001", Name = "GreenTech Supplies", Country = "Malaysia", ESGRating = "A", Active = true },
                new() { VendorId = "V002", Name = "EcoLogistics", Country = "Singapore", ESGRating = "B", Active = true },
                new() { VendorId = "V003", Name = "WaterWorks Ltd", Country = "Thailand", ESGRating = "A", Active = true },
                new() { VendorId = "V004", Name = "WastePro Solutions", Country = "Vietnam", ESGRating = "C", Active = true },
                new() { VendorId = "V005", Name = "AgroGlobal", Country = "Indonesia", ESGRating = "B", Active = true },
                new() { VendorId = "V006", Name = "SunPower Components", Country = "Philippines", ESGRating = "A", Active = true },
                new() { VendorId = "V007", Name = "EcoMaterials Co", Country = "China", ESGRating = "B", Active = true },
                new() { VendorId = "V008", Name = "SmartPack", Country = "Malaysia", ESGRating = "C", Active = true },
                new() { VendorId = "V009", Name = "FreshFarm Supplies", Country = "Thailand", ESGRating = "A", Active = true },
                new() { VendorId = "V010", Name = "RecycleMax", Country = "Singapore", ESGRating = "B", Active = true }
            };
            context.Vendor.AddRange(vendors);
            await context.SaveChangesAsync();

            // ========== COMPANIES ==========
            var companies = new List<Company>
            {
                new() { CompanyId = "C001", Name = "EcoFarm Sdn Bhd", Address = "123 Green Road, KL", Country = "Malaysia", Industry = "Agriculture", CreatedAt = DateTime.UtcNow },
                new() { CompanyId = "C002", Name = "SmartEnergy Corp", Address = "456 Solar Lane, Penang", Country = "Malaysia", Industry = "Energy", CreatedAt = DateTime.UtcNow },
                new() { CompanyId = "C003", Name = "HydroPure Solutions", Address = "789 Waterfall Ave, Bangkok", Country = "Thailand", Industry = "Water Treatment", CreatedAt = DateTime.UtcNow },
                new() { CompanyId = "C004", Name = "LogiChain Ltd", Address = "25 Supply Chain Blvd, Singapore", Country = "Singapore", Industry = "Logistics", CreatedAt = DateTime.UtcNow },
                new() { CompanyId = "C005", Name = "Waste2Wealth", Address = "12 Recycle St, Hanoi", Country = "Vietnam", Industry = "Waste Management", CreatedAt = DateTime.UtcNow }
            };
            context.Company.AddRange(companies);
            await context.SaveChangesAsync();

            // ========== DEPARTMENTS ==========
            var departments = new List<Department>();
            foreach (var company in companies)
            {
                departments.Add(new Department { DepartmentId = Guid.NewGuid().ToString(), CompanyId = company.CompanyId, Name = "Production" });
                departments.Add(new Department { DepartmentId = Guid.NewGuid().ToString(), CompanyId = company.CompanyId, Name = "Logistics" });
                departments.Add(new Department { DepartmentId = Guid.NewGuid().ToString(), CompanyId = company.CompanyId, Name = "Finance" });
            }
            context.Department.AddRange(departments);
            await context.SaveChangesAsync();

            // ========== EMPLOYEES ==========
            var positions = new[] { "Manager", "Technician", "Engineer", "Sales Executive", "Support Staff", "Driver" };
            var employees = new List<Employee>();
            foreach (var dept in departments)
            {
                for (int i = 0; i < 8; i++)
                {
                    employees.Add(new Employee
                    {
                        EmployeeId = Guid.NewGuid().ToString(),
                        DepartmentId = dept.DepartmentId,
                        Name = $"Employee {dept.Name} {_random.Next(100, 999)}",
                        Position = positions[_random.Next(positions.Length)],
                        HireDate = DateTime.UtcNow.AddDays(-_random.Next(1000)),
                        Salary = _random.Next(2500, 8000)
                    });
                }
            }
            context.Employee.AddRange(employees);
            await context.SaveChangesAsync();

            // ========== PRODUCTS ==========
            var productTemplates = new[]
            {
                new { Name = "Organic Lettuce", Category = "Vegetable", CarbonFootprint = 0.2m, WaterUsage = 5m },
                new { Name = "Kale", Category = "Vegetable", CarbonFootprint = 0.25m, WaterUsage = 6m },
                new { Name = "Spinach", Category = "Vegetable", CarbonFootprint = 0.3m, WaterUsage = 7m },
                new { Name = "Solar Panel Type A", Category = "Energy", CarbonFootprint = 12.5m, WaterUsage = 0m },
                new { Name = "Recycled Paper", Category = "Material", CarbonFootprint = 0.6m, WaterUsage = 3m }
            };

            var products = new List<Product>();
            foreach (var vendor in vendors)
            {
                for (int i = 0; i < 3; i++)
                {
                    var template = productTemplates[_random.Next(productTemplates.Length)];
                    products.Add(new Product
                    {
                        ProductId = Guid.NewGuid().ToString(),
                        Name = template.Name,
                        Category = template.Category,
                        CarbonFootprint = template.CarbonFootprint,
                        WaterUsage = template.WaterUsage,
                        VendorId = vendor.VendorId.Substring(1) // Example: 'V001' -> 1
                    });
                }
            }
            context.Product.AddRange(products);
            await context.SaveChangesAsync();

            // ========== ORDERS ==========
            var orders = new List<Order>();
            for (int i = 0; i < 50; i++)
            {
                var company = companies[_random.Next(companies.Count)];
                orders.Add(new Order
                {
                    OrderId = Guid.NewGuid().ToString(),
                    CompanyId = company.CompanyId,
                    OrderDate = DateTime.UtcNow.AddDays(-_random.Next(365)),
                    TotalAmount = 0 // will be calculated later
                });
            }
            context.Order.AddRange(orders);
            await context.SaveChangesAsync();

            // ========== ORDER DETAILS ==========
            var orderDetails = new List<OrderDetail>();
            foreach (var order in orders)
            {
                int numItems = _random.Next(1, 5);
                for (int i = 0; i < numItems; i++)
                {
                    var product = products[_random.Next(products.Count)];
                    var quantity = _random.Next(1, 20);
                    var price = _random.Next(10, 100);

                    orderDetails.Add(new OrderDetail
                    {
                        OrderDetailId = Guid.NewGuid().ToString(),
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        Quantity = quantity,
                        Price = price
                    });

                    order.TotalAmount += quantity * price;
                }
            }
            context.OrderDetail.AddRange(orderDetails);
            await context.SaveChangesAsync();

            // ========== ENVIRONMENTAL METRICS ==========
            var metrics = new List<EnvironmentalMetric>();
            foreach (var company in companies)
            {
                for (int i = 0; i < 10; i++)
                {
                    metrics.Add(new EnvironmentalMetric
                    {
                        MetricId = Guid.NewGuid().ToString(),
                        CompanyId = company.CompanyId,
                        MetricDate = DateTime.UtcNow.AddDays(-_random.Next(365)),
                        EnergyUsage = _random.Next(500, 5000),
                        WaterUsage = _random.Next(1000, 10000),
                        WasteGenerated = _random.Next(50, 500),
                        Emissions = _random.Next(10, 100)
                    });
                }
            }
            context.EnvironmentalMetric.AddRange(metrics);
            await context.SaveChangesAsync();

            // ========== COMPLIANCE RECORDS ==========
            var complianceTypes = new[] { "ISO14001", "ISO9001", "CarbonNeutral" };
            var compliances = new List<Compliance>();

            foreach (var company in companies)
            {
                for (int i = 0; i < 3; i++)
                {
                    var startDate = DateTime.UtcNow.AddMonths(-_random.Next(12));
                    compliances.Add(new Compliance
                    {
                        ComplianceId = Guid.NewGuid().ToString(),
                        CompanyId = company.CompanyId,
                        Name = complianceTypes[_random.Next(complianceTypes.Length)],
                        CertificationDate = startDate,
                        ExpiryDate = startDate.AddYears(1),
                        Status = "Active"
                    });
                }
            }
            context.Compliance.AddRange(compliances);
            await context.SaveChangesAsync();
        }
    }
}
