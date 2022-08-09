namespace BleFlasher;

public partial class MainPage : ContentPage
{
    BleManager scanner;

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
        var device = scanner.GetDevice(0);
        await device.connect();
    }

    
}

