using Microsoft.AspNetCore.Identity;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.General;
using System;

namespace RackingSystem.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userMng = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                logger.LogInformation("Start seed database ... ");
                await context.Database.EnsureCreatedAsync();

                // Add User
                logger.LogInformation("Start create admin user ... ");
                string adminEmail = "calctronic@gmail.com";
                string adminPass = "Calc@123";
                if (await userMng.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = adminEmail,
                    };
                    var result = await userMng.CreateAsync(adminUser, adminPass);
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Create user admin successful.");
                    }
                    else
                    {
                        logger.LogInformation($"Create user admin failed due to '{result.Errors}'.");
                    }
                }

                // Add Doc Format for Reel
                logger.LogInformation("Start Add Doc Format for Reel ... ");
                if (context.DocFormat.Where(x => x.DocFormatType == EnumDocFormat.Reel.ToString()).Any() == false)
                {
                    var docF = new DocFormat
                    {
                        DocFormatType = EnumDocFormat.Reel.ToString(),
                        DocumentFormat = "A<00000000>",
                        NumberLength = 8,
                        NextRoundingNum = 1,
                        IsActive = true,
                        IsResetMonthly = false,
                    };
                    context.DocFormat.Add(docF);
                    await context.SaveChangesAsync();
                }

                // Add Doc Format for JobOrder
                logger.LogInformation("Start Add Doc Format for JobOrder ... ");
                if (context.DocFormat.Where(x => x.DocFormatType == EnumDocFormat.JO.ToString()).Any() == false)
                {
                    var docF = new DocFormat
                    {
                        DocFormatType = EnumDocFormat.Reel.ToString(),
                        DocumentFormat = "JO{yyMM}<000000>",
                        NumberLength = 8,
                        NextRoundingNum = 1,
                        IsActive = true,
                        IsResetMonthly = true,
                    };
                    context.DocFormat.Add(docF);
                    await context.SaveChangesAsync();
                }

                // Set Default Doc Format for Reel in Configuration
                if (context.DocFormat.Where(x => x.DocFormatType == EnumDocFormat.Reel.ToString()).Any() && context.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.DocFormat_Reel.ToString()).Any() == false)
                {
                    var docF = context.DocFormat.Where(x => x.DocFormatType == EnumDocFormat.Reel.ToString()).First();
                    var config = new Configuration
                    {
                        ConfigTitle = EnumConfiguration.DocFormat_Reel.ToString(),
                        ConfigValue = docF.DocFormat_Id.ToString(),
                    };
                    context.Configuration.Add(config);
                    await context.SaveChangesAsync();
                }

                // Auto add user access right for admin
                var admin = await userMng.FindByEmailAsync(adminEmail);
                if (admin != null)
                {
                    foreach (EnumUAC uac in Enum.GetValues(typeof(EnumUAC)))
                    {
                        if (context.UserAccessRight.Where(x => x.User_Id == admin.Id && x.UAC_Id == (int)uac).Any() == false)
                        {
                            var uacDB = new UserAccessRight()
                            {
                                UAC = uac.ToString(),
                                UAC_Id = (int)uac,
                                User_Id = admin.Id
                            };
                            context.UserAccessRight.Add(uacDB);
                        }
                        await context.SaveChangesAsync();
                    }
                }

                // Set Default PLC IP Address for racking
                if (context.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).Any() == false)
                {
                    var config = new Configuration
                    {
                        ConfigTitle = EnumConfiguration.PLC_IPAddr_Racking1.ToString(),
                        ConfigValue = "192.168.1.15",
                    };
                    context.Configuration.Add(config);
                    await context.SaveChangesAsync();
                }
                // Set Default PLC IP Address for gantry
                if (context.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).Any() == false)
                {
                    var config = new Configuration
                    {
                        ConfigTitle = EnumConfiguration.PLC_IPAddr_Gantry1.ToString(),
                        ConfigValue = "192.168.1.14",
                    };
                    context.Configuration.Add(config);
                    await context.SaveChangesAsync();
                }

                // Set Default Loader reserve for min col height
                if (context.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.Loader_ColMinReserve.ToString()).Any() == false)
                {
                    var config = new Configuration
                    {
                        ConfigTitle = EnumConfiguration.Loader_ColMinReserve.ToString(),
                        ConfigValue = "7",
                    };
                    context.Configuration.Add(config);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}
