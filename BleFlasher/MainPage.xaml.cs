namespace BleFlasher;

public partial class MainPage : ContentPage
{
    BleManager scanner;
    Device device;

    public MainPage()
	{
		InitializeComponent();
        scanner = new BleManager();


    }

	private void OnStartScanningClicked(object sender, EventArgs e)
	{
        scanner.startScanning();
	}

    private void OnStopScanningClicked(object sender, EventArgs e)
    {
        scanner.stopScanning();
    }

    private async void OnConnectDevice1Clicked(object sender, EventArgs e)
    {
        device = scanner.GetDevice(0);
        await device.connect();
    }



    private async void OnEraseClicked(object sender, EventArgs e)
    {
        await device.erase(0x8008000,0x6000) ;
    }


    private async void OnWriteClicked(object sender, EventArgs e)
    {
        byte[] data = new byte[0x6000];
        Random.Shared.NextBytes(data);

        await device.write(0x8008000, data);
    }
    

}

