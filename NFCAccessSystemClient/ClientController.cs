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

    public void Run()
    {
        IState currentState = new IdleState(this);
        while (true) currentState = currentState.NextState(this);
    }

    interface IState
    {
        IState NextState(ClientController controller);
    }

    class IdleState : IState
    {
        string Uid { get; set; }

        public IdleState(ClientController controller)
        {
            Console.WriteLine("----- Idle State -----");

            var nfcHelper = new NFCHelper.NFCHelper();

            do
            {
                Thread.Sleep(200);
                Uid = nfcHelper.ReadUid();
            } while (Uid == null);
        }

        IState IState.NextState(ClientController controller)
        {
            return new AuthState(controller, Uid);
        }
    }

    class AuthState : IState
    {
        public AuthState(ClientController controller, string uid)
        {
            Console.WriteLine("----- Auth State -----");

            var inputReader = new InputReader(controller.KeyboardEventDevicePath);
            var pinPadReader = new PinPadReader(inputReader);

            Console.WriteLine("Reading TOTP: ");
            var totp = pinPadReader.ReadPin();

            // HACK: BYPASS CERT VALIDATION, GET RID OF THIS ASAP
            // https://stackoverflow.com/a/23535112
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
                certificate.Issuer == "CN=localhost";

            // TODO: check if server is online
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
                        // TODO: GPIO unlock
                        break;
                    case HttpStatusCode.Unauthorized:
                        Console.WriteLine("Auth failed.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Connection failed.");
            }
        }


        IState IState.NextState(ClientController controller)
        {
            return new IdleState(controller);
        }
    }
}

// bg thread: periodical db refresh (on a timer)        