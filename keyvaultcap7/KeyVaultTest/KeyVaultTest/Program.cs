using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

//url key vault
string keyVaultUrl = "https://mykeyvaultaz17461.vault.azure.net/";

//Crear el cliente de secret

DefaultAzureCredentialOptions options = new() { 
    ExcludeEnvironmentCredential = true,
    ExcludeManagedIdentityCredential=true
};

var client = new SecretClient(new Uri(keyVaultUrl), 
    new DefaultAzureCredential(options));

//Crear menu
// Main application loop - continues until user types 'quit'
while (true)
{
    // Display menu options to the user
    Console.Clear();
    Console.WriteLine("\nPlease select an option:");
    Console.WriteLine("1. Create a new secret");
    Console.WriteLine("2. List all secrets");
    Console.WriteLine("Type 'quit' to exit");
    Console.Write("Enter your choice: ");

    // Read user input and convert to lowercase for easier comparison
    string? input = Console.ReadLine()?.Trim().ToLower();

    // Check if user wants to exit the application
    if (input == "quit")
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    // Process the user's menu selection
    switch (input)
    {
        case "1":
            // Call the method to create a new secret
            await CreateSecretAsync(client);
            break;
        case "2":
            // Call the method to list all existing secrets
            await ListSecretsAsync(client);
            break;
        default:
            // Handle invalid input
            Console.WriteLine("Invalid option. Please enter 1, 2, or 'quit'.");
            break;
    }
}

//metodo para crear un secreto

async Task CreateSecretAsync(SecretClient client) {
    try {
        Console.Clear();
        Console.WriteLine("generando nuevo secreto...");

        Console.WriteLine("nombre secreto: ");
        string? secretName = Console.ReadLine()?.Trim();

        Console.WriteLine("valor de secreto: ");
        string? secretValue = Console.ReadLine()?.Trim();

        if (string .IsNullOrEmpty(secretName) && string.IsNullOrEmpty(secretValue)) {
            return;
        }

        //almacenando en azure

        var secret = new KeyVaultSecret(secretName, secretValue);
        await client.SetSecretAsync(secret);

        Console.WriteLine($"secreto ${secretName} creado en azure");
        Console.WriteLine("enter para continuar....");
        Console.ReadLine();


    }
    catch (Exception ex) {
        Console.WriteLine($"ERROR CREAR SECRET {ex.Message}");
    }
}



//metodo para listar secretos
async Task ListSecretsAsync(SecretClient client) {
    try {
        Console.Clear();
        Console.WriteLine("Listando todos los secretos");
        Console.WriteLine("-.-.-.-.-.-..-.-.-.-.-.-.");

        //obteniendo todos los secretos
        var secretProperties = client.GetPropertiesOfSecretsAsync();
        bool hasSecrets = false;

        await foreach (var secretProperty in secretProperties) {
            hasSecrets = true;
            var secret = await client.GetSecretAsync(secretProperty.Name);

            /*información del secreto*/
            Console.WriteLine($"Name: {secret.Value.Name}");
            Console.WriteLine($"Value: {secret.Value.Value}");
            Console.WriteLine($"Creado: {secret.Value.Properties.CreatedOn}");
            Console.WriteLine("-*-*-*-*-*-*-*-*-*-*-*-*-*");

        }

        if (!hasSecrets) {
            Console.WriteLine("No hay secretos");
        }


    }
    catch (Exception ex) {
        Console.WriteLine($"ERROR LISTAR SECRETOS {ex.Message}");
    }

    Console.WriteLine("Pulsa enter para continuar....");
    Console.ReadLine();

}
