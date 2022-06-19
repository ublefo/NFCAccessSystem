using System.Net;

namespace NFCAccessSystemClient;

public class ClientController
{
    private AppConfig ClientConfig { get; set; }
    private String KeyboardEventDevicePath { get; set; }

    public ClientController(AppConfig clientConfig)
    {
        ClientConfig = clientConfig;
        KeyboardEventDevicePath = "/dev/input/by-id/" + ClientConfig.KeyboardId;
    }

    public void Run(GPIOHelper.GPIOHelper gpioHelper)
    {
        IState currentState = new IdleState(this, gpioHelper);
        while (true) currentState = currentState.NextState(this, gpioHelper);
    }

    interface IState
    {
        IState NextState(ClientController controller, GPIOHelper.GPIOHelper gpioHelper);
    }

    class IdleState : IState
    {
        string Uid { get; set; }

        public IdleState(ClientController controller, GPIOHelper.GPIOHelper gpioHelper)
        {
            Console.WriteLine("----- Idle State -----");

            // if GPIO helper is configured reset LED status (blue = waiting)
            if (gpioHelper != null)
            {
                gpioHelper.StatusLedUpdate("blue");
            }

            var nfcHelper = new NFCHelper.NFCHelper();

            do
            {
                Thread.Sleep(200);
                Uid = nfcHelper.ReadUid();
            } while (Uid == null);

            // if GPIO helper is configured trigger buzzer
            if (gpioHelper != null)
            {
                gpioHelper.BuzzerOneShot();
            }
        }

        IState IState.NextState(ClientController controller, GPIOHelper.GPIOHelper gpioHelper)
        {
            return new AuthState(controller, gpioHelper, Uid);
        }
    }

    class AuthState : IState
    {
        public AuthState(ClientController controller, GPIOHelper.GPIOHelper gpioHelper, string uid)
        {
            Console.WriteLine("----- Auth State -----");

            // if GPIO helper is configured set LED to cyan (pinentry)
            if (gpioHelper != null)
            {
                gpioHelper.StatusLedUpdate("cyan");
            }

            string totp = "";

            using (var inputReader = new InputReader(controller.KeyboardEventDevicePath))
            {
                var pinPadReader = new PinPadReader(inputReader);
                Console.WriteLine("Reading TOTP: ");
                totp = pinPadReader.ReadPin(gpioHelper);
            }

            // if GPIO helper is configured set LED to white (authenticating)
            if (gpioHelper != null)
            {
                gpioHelper.StatusLedUpdate("white");
            }

            // HACK: BYPASS CERT VALIDATION, GET RID OF THIS ASAP
            // https://stackoverflow.com/a/23535112
            // ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            //    certificate.Issuer == "CN=localhost";

            // server selection
            WebRequest remoteServerHealthCheckRequest =
                WebRequest.Create(controller.ClientConfig.ServerAddress + "healthcheck");
            WebRequest localServerHealthCheckRequest =
                WebRequest.Create(controller.ClientConfig.LocalServerAddress + "healthcheck");

            Uri selectedServerUri = null;
            HttpWebResponse remoteServerHealthCheckResponse = null;
            HttpWebResponse localServerHealthCheckResponse = null;

            // check if remote server is ok
            try
            {
                remoteServerHealthCheckResponse = (HttpWebResponse) remoteServerHealthCheckRequest.GetResponse();
            }
            catch (System.Net.WebException e)
            {
                remoteServerHealthCheckResponse = (HttpWebResponse) e.Response;
            }


            if (remoteServerHealthCheckResponse != null)
            {
                if (remoteServerHealthCheckResponse.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Remote server healthcheck ok, selecting it.");
                    selectedServerUri = controller.ClientConfig.ServerAddress;
                }
                else
                {
                    Console.WriteLine("Remote server healthcheck reported errors.");
                }
            }
            else
            {
                Console.WriteLine("Remote server unreachable.");
            }

            // remote server connection failed, check local server
            if (selectedServerUri == null)
            {
                try
                {
                    localServerHealthCheckResponse = (HttpWebResponse) localServerHealthCheckRequest.GetResponse();
                }
                catch (System.Net.WebException e)
                {
                    localServerHealthCheckResponse = (HttpWebResponse) e.Response;
                }

                if (localServerHealthCheckResponse != null)
                {
                    if (localServerHealthCheckResponse.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("Local server healthcheck ok, selecting it.");
                        selectedServerUri = controller.ClientConfig.LocalServerAddress;
                    }
                    else
                    {
                        Console.WriteLine("Local server healthcheck reported errors.");
                    }
                }
                else
                {
                    Console.WriteLine("Local server unreachable.");
                }
            }

            // if no server is selected then both are unreachable
            if (selectedServerUri == null)
            {
                // TODO: produce visual indication for this
                Console.WriteLine("Both remote and local servers are unreachable. Giving up auth process.");
                return;
            }

            // server checks out, construct unlocking request
            NetworkCredential userCred = new NetworkCredential(uid, totp, null);
            CredentialCache credCache = new CredentialCache
                {{selectedServerUri, "Basic", userCred}};

            WebRequest unlockRequest = WebRequest.Create(selectedServerUri + "unlock");
            unlockRequest.Credentials = credCache;

            HttpWebResponse unlockResponse = null;
            try
            {
                unlockResponse = (HttpWebResponse) unlockRequest.GetResponse();
            }
            catch (System.Net.WebException e)
            {
                unlockResponse = (HttpWebResponse) e.Response;
            }

            if (unlockResponse != null)
            {
                switch (unlockResponse.StatusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine("Auth passed, unlocking.");

                        // if GPIO helper is configured, unlock and play long beep, LED to green
                        if (gpioHelper != null)
                        {
                            gpioHelper.StatusLedUpdate("green");
                            gpioHelper.BuzzerCustomLength(1500);
                            gpioHelper.ServoUnlock();
                        }

                        break;
                    case HttpStatusCode.Unauthorized:
                        Console.WriteLine("Auth failed.");
                        // if GPIO helper is configured, play three short beeps, set led to red
                        if (gpioHelper != null)
                        {
                            gpioHelper.StatusLedUpdate("red");
                            for (int i = 0; i < 3; i++)
                            {
                                gpioHelper.BuzzerOneShot();
                                Thread.Sleep(500);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine(unlockResponse.StatusCode);
                        // if GPIO helper is configured, play three long beeps, set led to magenta
                        if (gpioHelper != null)
                        {
                            gpioHelper.StatusLedUpdate("magenta");
                            for (int i = 0; i < 3; i++)
                            {
                                gpioHelper.BuzzerCustomLength(1500);
                                Thread.Sleep(200);
                            }
                        }

                        break;
                }
            }
            else
            {
                // if GPIO helper is configured, play three long beeps, set led to magenta
                if (gpioHelper != null)
                {
                    gpioHelper.StatusLedUpdate("magenta");
                    for (int i = 0; i < 3; i++)
                    {
                        gpioHelper.BuzzerCustomLength(1500);
                        Thread.Sleep(200);
                    }
                }

                Console.WriteLine("Connection failed.");
            }
        }


        IState IState.NextState(ClientController controller, GPIOHelper.GPIOHelper gpioHelper)
        {
            return new IdleState(controller, gpioHelper);
        }
    }
}