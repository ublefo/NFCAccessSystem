using SharpNFC;

namespace NFCHelper;

public class NFCHelper
{
    private NFCContext NfcContext { get; set; }
    private NFCDevice NfcReader { get; set; }

    public NFCHelper()
    {
        NfcContext = new NFCContext();
        var nfcReaderName = NfcContext.ListDeviceNames().FirstOrDefault();

        if (nfcReaderName == null)
        {
            Console.WriteLine("No NFC Reader found!");
            NfcReader = null;
        }

        Console.WriteLine("Using NFC Reader: " + nfcReaderName);
        NfcReader = NfcContext.OpenDevice(nfcReaderName);
    }

    public string ReadUid()
    {
        if (NfcReader == null)
        {
            return null;
        }

        var nfcTarget = NfcReader.SelectPassive14443ATarget();
        if (nfcTarget.nti.szUidLen == 0)
        {
            // Console.WriteLine("No card found.");
            return null;
        }

        byte[] uidArray = nfcTarget.nti.abtUid[4 .. 8];
        Console.WriteLine("UID is: " + BitConverter.ToString(uidArray));
        return Convert.ToHexString(uidArray, 0, 4);
    }
}