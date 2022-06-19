using System;
using System.Net;
using System.Threading;

namespace NFCAccessSystemClient;

public class HelperThread
{
    public static void HealthCheckThread(AppConfig clientConfig, GPIOHelper.GPIOHelper gpioHelper)
    {
        while (true)
        {
            // set up healthcheck request

            // HACK: BYPASS CERT VALIDATION, GET RID OF THIS ASAP
            // https://stackoverflow.com/a/23535112
            // ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            //     certificate.Issuer == "CN=localhost";

            // server selection
            WebRequest remoteServerHealthCheckRequest =
                WebRequest.Create(clientConfig.ServerAddress + "healthcheck");
            WebRequest localServerHealthCheckRequest =
                WebRequest.Create(clientConfig.LocalServerAddress + "healthcheck");

            // time out simple queries quickly
            remoteServerHealthCheckRequest.Timeout = 2000;
            localServerHealthCheckRequest.Timeout = 2000;

            HttpWebResponse remoteServerHealthCheckResponse = null;
            HttpWebResponse localServerHealthCheckResponse = null;

            bool remoteSvrOkay = false;
            bool localSvrOkay = false;

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
                    Console.WriteLine("Remote server healthcheck ok.");
                    remoteSvrOkay = true;
                }
                else
                {
                    Console.WriteLine("Remote server healthcheck reported errors.");
                    remoteSvrOkay = false;
                }
            }
            else
            {
                Console.WriteLine("Remote server unreachable.");
                remoteSvrOkay = false;
            }

            // set remote server LED if GPIO helper is configured
            if (gpioHelper != null)
            {
                gpioHelper.RemoteStatusLedUpdate(remoteSvrOkay);
            }


            // remote server bad, check local server
            if (!remoteSvrOkay)
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
                        Console.WriteLine("Local server healthcheck ok.");
                        localSvrOkay = true;
                    }
                    else
                    {
                        Console.WriteLine("Local server healthcheck reported errors.");
                        localSvrOkay = false;
                    }
                }
                else
                {
                    Console.WriteLine("Local server unreachable.");
                    localSvrOkay = false;
                }
            }

            Thread.Sleep(2000);
        }
    }


    public static void DbSyncThread(AppConfig clientConfig, GPIOHelper.GPIOHelper gpioHelper)
    {
        while (true)
        {
            try
            {
                DbSync(clientConfig, gpioHelper);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Thread.Sleep(10000);
        }
    }


    public static void DbSync(AppConfig clientConfig, GPIOHelper.GPIOHelper gpioHelper)
    {
        String LocalSvrDbPath = clientConfig.LocalServerDbPath;
        String TmpDbPath = "/tmp/acs-new.db";

        // HACK: BYPASS CERT VALIDATION, GET RID OF THIS ASAP
        // https://stackoverflow.com/a/23535112
        // ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
        //    certificate.Issuer == "CN=localhost";

        // download remote db as a byte array in memory
        byte[] remoteDb = null;

        NetworkCredential userCred = new NetworkCredential("DbSync", clientConfig.DbAccessKey, null);
        CredentialCache credCache = new CredentialCache
            {{clientConfig.ServerAddress, "Basic", userCred}};

        try
        {
            using (var client = new WebClient())
            {
                client.Credentials = credCache;
                remoteDb = client.DownloadData(clientConfig.ServerAddress + "dbsync");
            }
        }
        catch (Exception e)
        {
            // if remote db fails to download, do not proceed
            // Console.WriteLine(e);
            Console.WriteLine("Database download failed.");
            return;
        }

        byte[] localDb = null;

        try
        {
            localDb = File.ReadAllBytes(LocalSvrDbPath);
        }
        catch (Exception e)
        {
            // if local db cannot be loaded, do not proceed
            Console.WriteLine(e);
            return;
        }


        if (remoteDb != null && localDb != null)
        {
            if (remoteDb.SequenceEqual(localDb))
            {
                // if files are identical, return
                Console.WriteLine("Remote and local db are identical, no need for updates.");
                return;
            }
        }

        Console.WriteLine("Remote and local db are different, replacing.");
        using (var destFileStream = File.Create(TmpDbPath))
        {
            destFileStream.Write(remoteDb);
            destFileStream.Flush();
            destFileStream.Close();
        }

        try
        {
            File.Move(TmpDbPath, LocalSvrDbPath, true);
            Console.WriteLine("Database file updated.");
        }
        catch (Exception e)
        {
            // if file replacement failed, return
            Console.WriteLine(e);
            return;
        }

        // at this point, file has been replaced, issuing dbrefresh request to local server

        WebRequest localServerHealthCheckRequest =
            WebRequest.Create(clientConfig.LocalServerAddress + "dbrefresh");

        HttpWebResponse localServerHealthCheckResponse = null;

        // call refresh
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
                Console.WriteLine("Database reload successful.");
                // trigger sync indicator LED if GPIO helper is configured
                if (gpioHelper != null)
                {
                    gpioHelper.SyncLedTrig();
                }
            }
            else
            {
                Console.WriteLine("Local server reported errors.");
            }
        }
        else
        {
            Console.WriteLine("Local server unreachable.");
        }
    }
}