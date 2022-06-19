using System.Runtime.InteropServices;
using NFCAccessSystemClient;

var clientConfig = new AppConfig("client-config.json");

// only set up GPIO helper on embedded platforms
GPIOHelper.GPIOHelper gpioHelper;

if (RuntimeInformation.OSArchitecture is Architecture.Arm or Architecture.Arm64)
{
    gpioHelper = new GPIOHelper.GPIOHelper();
}
else
{
    gpioHelper = null;
}


Thread healthChk = new Thread(() => HelperThread.HealthCheckThread(clientConfig, gpioHelper));
healthChk.Start();

Thread dbSync = new Thread(() => HelperThread.DbSyncThread(clientConfig, gpioHelper));
dbSync.Start();

var clientController = new ClientController(clientConfig);

clientController.Run(gpioHelper);