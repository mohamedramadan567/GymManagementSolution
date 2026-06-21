using GymManagement.DAL.Data.DbContexts;
using GymManagement.DAL.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GymManagement.DAL.Data.DataSeeding
{
    public static class GymDataSeeding
    {
        public static async Task SeedAsync(GymDbContext dbContext, string seedFolderPath, ILogger logger, CancellationToken ct = default)
        {
            try
            {
                if(!dbContext.Plans.Any())
                {
                    var plans = LoadDataFromJsonFile<Plan>(seedFolderPath, "plans.json");

                    //Add to Database
                    if(plans.Any())
                    {
                        await dbContext.Plans.AddRangeAsync(plans, ct);
                        await dbContext.SaveChangesAsync(ct);
                        logger.LogInformation($"Seeding Plans With Count {plans.Count}");
                    }

                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        public static List<T> LoadDataFromJsonFile<T>(string folderPath, string fileName)
        {
            var filePath = Path.Combine(folderPath, fileName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Seed Data File Not Found {filePath}");

            //Read Data from Json File plans.json As JsonString 
            var data = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            //Convert JsonString List<Plan>
            var result = JsonSerializer.Deserialize<List<T>>(data, options) ?? [];

            return result;

        }
    }
}
