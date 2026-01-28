using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Identity;
using System.Text;

//autenticacion de azure
string namespaceURL = "eventhub4388.servicebus.windows.net";
string eventHubName = "myEventHub";

DefaultAzureCredentialOptions options = new() { 
  ExcludeEnvironmentCredential = true,
  ExcludeManagedIdentityCredential = true
};

//cantidad de eventos
int numOfEvents = 3;

//Crear Producer (envia eventos)
EventHubProducerClient producerClient = new EventHubProducerClient(
    namespaceURL,
    eventHubName,
    new DefaultAzureCredential(options)
);

//crear un objeto batch para registrar los 3 eventos
using EventDataBatch eventBach = await producerClient.CreateBatchAsync();

var random = new Random();
for (int i=1; i<= numOfEvents; i++) { 
    int randomNumber = random.Next(1, 200);
    string eventBody = $"Event {randomNumber}";
    eventBach.TryAdd(new EventData(Encoding.UTF8.GetBytes(eventBody)));
}


try
{
    await producerClient.SendAsync(eventBach);
    Console.WriteLine($"{numOfEvents} generados");
    Console.WriteLine("Pulsa enter para continuar....");
    Console.ReadLine();

}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
finally {
    await producerClient.DisposeAsync();
}



//Consumer (Consumidor de eventos)

await using var consumerClient = new EventHubConsumerClient(
    EventHubConsumerClient.DefaultConsumerGroupName,
    namespaceURL,
    eventHubName,
    new DefaultAzureCredential(options));

Console.Clear();
Console.WriteLine("Obteniendo todos los eventos del hub.....");

long totalEventcount = 0;

string [] partitionIds= await consumerClient.GetPartitionIdsAsync();

foreach (var paritionId in partitionIds)
{
    PartitionProperties properties = await consumerClient.GetPartitionPropertiesAsync(paritionId);
    if (!properties.IsEmpty && properties.LastEnqueuedSequenceNumber>= properties.BeginningSequenceNumber) {
        totalEventcount += (properties.LastEnqueuedSequenceNumber - properties.BeginningSequenceNumber + 1);
    }
}


int retrievedCount = 0;

await foreach (PartitionEvent partitionEvent in consumerClient.ReadEventsAsync(startReadingAtEarliestEvent:true)) {
    if (partitionEvent.Data != null) {
        string body = Encoding.UTF8.GetString(partitionEvent.Data.EventBody.ToArray());
        Console.WriteLine($"Retrived Event {body}");
        retrievedCount++;
        if (retrievedCount >= totalEventcount) {
            Console.WriteLine("Fin de obtención de eventos  enter para salir...");
            Console.ReadLine();
            return;
        }
    }

}




