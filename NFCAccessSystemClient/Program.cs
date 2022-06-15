using NFCAccessSystemClient;

var clientConfig = new AppConfig("client-config.json");
var clientController = new ClientController(clientConfig);

clientController.Run();