# Sample Pulumi Dotnet
This is a Pulumi sample project for creating Azure resources using Dotnet.

## Setting up
To be able to run this code, you need to have the pulumi cli and dotnet installed
[Click here](https://dotnet.microsoft.com/download/dotnet-framework) to download and install the dotnet framework.

To setup, you'd need to run a few commands in your terminal.

Follow the steps below to setup pulumi on your computer
- Install the pulumi CLI
```
# Windows
choco install pulumi

# Mac
brew install pulumi

# Linux
curl -fsSL https://get.pulumi.com | sh
```

- Login to the Azure CLI
```
az login
```

- Create iac working directory and clone this project
```
mkdir iac && cd iac
git clone https://github.com/AdoraNwodo/sample-pulumi-dotnet.git
```

- Setup a pulumi backend by creating a storage account & storage container in Azure. 
- Add the necessary environment variables
```
$env:AZURE_STORAGE_ACCOUNT = "<storage-account-name>"
$env:AZURE_STORAGE_KEY = "<storage-account-key>"
$env:AZURE_KEYVAULT_AUTH_VIA_CLI = "true"

```
- Login to pulumi 
```
pulumi login --cloud-url azblob://<storage-container-name>
```

- Now, you've setup your pulumi project. The next thing is to create a pulumi stack.  
_Pulumi projects and stacks let you organize Pulumi code. Consider a Pulumi project to be analogous to a GitHub repo—a single place for code—and a stack to be an instance of that code with a separate configuration. For instance, Project Foo may have multiple stacks for different development environments (Dev, Test, or Prod), or perhaps for different cloud configurations (geographic region for example)._
_Each stack will have it's yaml config. If you project has a dev and test stack you will have two configs namely: `Pulumi.dev.yaml` & `Pulumi.test.yaml` and these files are where you add your separate configurations._
```
pulumi stack init dev
```
We are creating only a dev stack to test out (and build on) this code. However, you can always create multiple stacks for whatever reason. If you have multiple stacks, you can select your current working stack by running:
```
pulumi stack select [stackName]
```

## Running this project
Once you've successfully setup, feel free to run:
```
pulumi up
```
and watch your Azure resources get created. 
