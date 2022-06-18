using NFCAccessSystemClient;

var clientConfig = new AppConfig("client-config.json");
var clientController = new ClientController(clientConfig);

Thread healthChk = new Thread(() => HelperThread.HealthCheckThread(clientConfig));
healthChk.Start();

Thread dbSync = new Thread(() => HelperThread.DbSyncThread(clientConfig));
dbSync.Start();

clientController.Run();