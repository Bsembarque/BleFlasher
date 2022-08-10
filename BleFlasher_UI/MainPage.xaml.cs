namespace BleFlasher_UI;
using BleFlasher;

public partial class MainPage : ContentPage
{
    BleFlasher flasher;

    public MainPage()
	{
		InitializeComponent();
        flasher = new BleFlasher();
    }

	private void OnStartScanningClicked(object sender, EventArgs e)
	{
        flasher.startScanning();
	}

    private void OnStopScanningClicked(object sender, EventArgs e)
    {
        flasher.stopScanning();
    }

    private async void OnConnectDevice1Clicked(object sender, EventArgs e)
    {
        var device = flasher.GetDevices()[0];
        await flasher.connect(device);
    }



    private async void OnEraseClicked(object sender, EventArgs e)
    {
        await flasher.erase(0x8008000,0x6000) ;
    }


    private async void OnWriteClicked(object sender, EventArgs e)
    {
        byte[] data = new byte[0x6000];
        Random.Shared.NextBytes(data);

        await flasher.write(0x8008000, data);
    }


    private async void OnSelectFileClicked(object sender, EventArgs e)
    {
        var file = await FilePicker.Default.PickAsync();
        var filestream = await file.OpenReadAsync();
        await flasher.writeBinaryFile(0x8008000, filestream);
    }
    

}

