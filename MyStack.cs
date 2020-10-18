using Microsoft.VisualBasic.CompilerServices;
using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Core;
using Pulumi.Azure.KeyVault;
using Pulumi.Azure.Storage;

class MyStack : Stack
{
    private const string AppServicePlanBaseName = "sample-pulumi-plan";
    private const string FunctionAppBaseName = "sample-pulumi-app";
    private const string ResourceGroupBaseName = "sample-pulumi-rg";
    private const string KeyvaultBaseName = "sample-pulumi-kv";
    private const string StorageAccountBaseName = "samplepulumistore";

    public MyStack()
    {
        // Get Pulumi Configs
        var config = new Config();
        var appServicePlanSize = config.Get("appServicePlanSize") ?? "S1";
        var appServicePlanTier = config.Get("appServicePlanTier") ?? "Standard";
        var deploymentAgentOId = config.Get("deploymentAgentOId") ?? "11111111-1011-1111-1111-111111111111";
        var env = config.Get("env") ?? "test";
        var location = config.Get("location") ?? "WestEurope";
        var tenantId = config.Get("tenantId") ?? "11111111-1011-1111-1111-111111111111";

        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup($"{ResourceGroupBaseName}-{env}", new ResourceGroupArgs {
            Location = location,
            Name = $"{ResourceGroupBaseName}-{env}",
        });

        // Create an Azure Storage Account
        var storageAccount = new Account($"{StorageAccountBaseName}{env}", new AccountArgs
        {
            Name = $"{StorageAccountBaseName}{env}",
            ResourceGroupName = resourceGroup.Name,
            AccountReplicationType = "LRS",
            AccountTier = "Standard"
        });

        // Create an App Service Plan
        var appServicePlan = new Plan($"{AppServicePlanBaseName}-{env}", new PlanArgs
        {
            Location = resourceGroup.Location,
            MaximumElasticWorkerCount = 1,
            Name = $"{AppServicePlanBaseName}-{env}",
            ResourceGroupName = resourceGroup.Name,
            Sku = new PlanSkuArgs
            {
                Size = appServicePlanSize,
                Tier = appServicePlanTier
            }
        });

        // Get Function App Settings
        var appSettings = GetAppSettingsMap(config);

        // Create a Function App
        var functionApp = new FunctionApp($"{FunctionAppBaseName}-{env}", new FunctionAppArgs {
            Name = $"{FunctionAppBaseName}-{env}",
            StorageAccountName = storageAccount.Name,
            AppServicePlanId = appServicePlan.Id,
            Location = resourceGroup.Location,
            ResourceGroupName = resourceGroup.Name,
            StorageAccountAccessKey = storageAccount.PrimaryAccessKey,
            Identity = new FunctionAppIdentityArgs
            {
                Type = "SystemAssigned"
            },
            Version = "~3",
            AppSettings = appSettings,
            SiteConfig = new FunctionAppSiteConfigArgs
            {
                AlwaysOn = true,
                Use32BitWorkerProcess = false,
                WebsocketsEnabled = false
            }
        });

        // Create keyvault
        var keyvault = new KeyVault($"{KeyvaultBaseName}-{env}", new KeyVaultArgs
        {
            Location = resourceGroup.Location,
            Name = $"{KeyvaultBaseName}-{env}",
            ResourceGroupName = resourceGroup.Name,
            SkuName = "standard",
            TenantId = tenantId
        });

        // Set Access Policies
        var functionKeyvaultAccessPolicy = new AccessPolicy($"{KeyvaultBaseName}-{env}-fn-policy", new AccessPolicyArgs
        {
            ObjectId = functionApp.Identity.Apply(identity => identity?.PrincipalId ?? "11111111-1011-1111-1111-111111111111"),
            KeyVaultId = keyvault.Id,
            TenantId = tenantId,
            KeyPermissions = new string[] { "get", "list" },
            SecretPermissions = new string[] { "get", "list", "set", "delete" },
            CertificatePermissions = new string[] { "get", "list" },
        });

        var deplymentAgentKeyvaultAccessPolicy = new AccessPolicy($"{KeyvaultBaseName}-{env}-agent-policy", new AccessPolicyArgs
        {
            ObjectId = deploymentAgentOId,
            KeyVaultId = keyvault.Id,
            TenantId = tenantId,
            KeyPermissions = new string[] { "get", "list" },
            SecretPermissions = new string[] { "get", "list", "set", "delete" },
            CertificatePermissions = new string[] { "get", "list" },
        });

        // Add Storage Account Secret
        var storageAccountSecret = new Secret($"{KeyvaultBaseName}-{env}-secret", new SecretArgs {
            KeyVaultId = keyvault.Id,
            Name = "StorageAccountConnectionString",
            Value = storageAccount.PrimaryConnectionString
        });

        // Export the Function App Url
        this.FunctionAppUrl = functionApp.DefaultHostname.Apply(hostName => $"https://{hostName}");
    }

    private InputMap<string> GetAppSettingsMap(Config config)
    {
        var appSettings = new InputMap<string>();
        appSettings.Add("CLOUD_SERVICE_CONFIG", config.Require("env"));

        return appSettings;
    }

    [Output]
    public Output<string> FunctionAppUrl { get; set; }
}
