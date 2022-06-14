using SharpNFC;

namespace NFCHelper;

public class NFCHelper
{
    public string ReadUid()
    {
        using (var NfcContext = new NFCContext())
        {
            var nfcReaderName = NfcContext.ListDeviceNames().FirstOrDefault();

            if (nfcReaderName == null)
            {
                Console.WriteLine("No NFC Reader found!");
                return null;
            }

            Console.WriteLine("Using NFC Reader: " + nfcReaderName);

            using (var NfcReader = NfcContext.OpenDevice(nfcReaderName))
            {
                var nfcTarget = NfcReader.SelectPassive14443ATarget();
                if (nfcTarget.nti.szUidLen == 0)
                {
                    Console.WriteLine("No card found.");
                    return null;
                }

                byte[] uidArray = nfcTarget.nti.abtUid[4 .. 8];
                Console.WriteLine("UID is: " + BitConverter.ToString(uidArray));
                return Convert.ToHexString(uidArray, 0, 4);
            }
        }
    }
}