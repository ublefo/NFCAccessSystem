using System.Device.Pwm;
using Iot.Device.ServoMotor;

namespace GPIOHelper;

public class GPIOHelper
{
    private ServoMotor ServoMotor { get; set; }

    private const string LedPathPrefix = "/sys/class/leds";
    private const string BuzzerPath = "/sys/class/leds/buzzer";

    private const string RemoteOkLedLabel = "white:remote-ok";
    private const string SyncLedLabel = "yellow:sync";
    private const string StatusRedLabel = "red:status";
    private const string StatusGreenLabel = "green:status";
    private const string StatusBlueLabel = "blue:status";


    public GPIOHelper()
    {
        // PWMChip0, channel 0 is connected to the servo
        ServoMotor = new ServoMotor(PwmChannel.Create(0, 0, 50));
    }

    public void ServoUnlock()
    {
        ServoMotor.WriteAngle(120);
        ServoMotor.Start();

        Thread.Sleep(2000);
        ServoMotor.WriteAngle(0);

        Thread.Sleep(1000);
        ServoMotor.Stop();
    }

    public void RemoteStatusLedUpdate(bool status)
    {
        var ledBrightnessPath = Path.Join(LedPathPrefix, RemoteOkLedLabel, "brightness");
        File.WriteAllText(ledBrightnessPath, status ? "1" : "0");
    }

    public void SyncLedTrig()
    {
        // sync led uses oneshot ledtrig
        var ledTrigPath = Path.Join(LedPathPrefix, SyncLedLabel, "shot");
        File.WriteAllText(ledTrigPath, "1");
    }

    public void StatusLedUpdate(string status)
    {
        var redLed = Path.Join(LedPathPrefix, StatusRedLabel, "brightness");
        var greenLed = Path.Join(LedPathPrefix, StatusGreenLabel, "brightness");
        var blueLed = Path.Join(LedPathPrefix, StatusBlueLabel, "brightness");

        // clear existing LED status
        File.WriteAllText(redLed, "0");
        File.WriteAllText(greenLed, "0");
        File.WriteAllText(blueLed, "0");

        switch (status)
        {
            case "red":
                File.WriteAllText(redLed, "1");
                break;
            case "green":
                File.WriteAllText(greenLed, "1");
                break;
            case "blue":
                File.WriteAllText(blueLed, "1");
                break;
            case "yellow":
                File.WriteAllText(redLed, "1");
                File.WriteAllText(greenLed, "1");
                break;
            case "magenta":
                File.WriteAllText(redLed, "1");
                File.WriteAllText(blueLed, "1");
                break;
            case "cyan":
                File.WriteAllText(greenLed, "1");
                File.WriteAllText(blueLed, "1");
                break;
            case "white":
                File.WriteAllText(redLed, "1");
                File.WriteAllText(greenLed, "1");
                File.WriteAllText(blueLed, "1");
                break;
        }
    }

    public void BuzzerOneShot()
    {
        var buzzer = Path.Join(BuzzerPath, "shot");
        File.WriteAllText(buzzer, "1");
    }

    public void BuzzerCustomLength(int length)
    {
        // yes, brightness, because this "borrows" the led driver
        var buzzer = Path.Join(BuzzerPath, "brightness");
        File.WriteAllText(buzzer, "1");
        Thread.Sleep(length);
        File.WriteAllText(buzzer, "0");

        // restore trigger since kernel sets it to none if you write to the brightness file
        var buzzerTrigger = Path.Join(BuzzerPath, "trigger");
        File.WriteAllText(buzzerTrigger, "oneshot");
        // restore on and off durations
        File.WriteAllText(Path.Join(BuzzerPath, "delay_on"), "100");
        File.WriteAllText(Path.Join(BuzzerPath, "delay_off"), "100");
    }
}