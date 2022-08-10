namespace BleFlasher_UI;
using BleFlasher;

public partial class MainPage : ContentPage
{
    BleFlasher flasher;

    public MainPage()
	{
		InitializeComponent();
        flasher = new BleFlasher();
        StopScanning.IsVisible = false;
        ConnectDevice.IsVisible = false;
        DisconnectDevice.IsVisible = false;
    }

	private void OnStartScanningClicked(object sender, EventArgs e)
	{
        flasher.startScanning();

        StartScanning.IsVisible = false;
        StopScanning.IsVisible = true;
        ConnectDevice.IsVisible = false;
        DisconnectDevice.IsVisible = false;
    }

    private void OnStopScanningClicked(object sender, EventArgs e)
    {
        flasher.stopScanning();

        var devices = flasher.GetDevices();


        PickerScan.Items.Clear();

        foreach (var device in devices)
        {
            string item = device.Name + "(" + device.Id + ")";

            if (!PickerScan.Items.Contains(item))
            {
                PickerScan.Items.Add(item);
            }
        }
        PickerScan.SelectedIndex = 0;

        StartScanning.IsVisible = true;
        StopScanning.IsVisible = false;
        ConnectDevice.IsVisible = true;
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        var device = flasher.GetDevices()[PickerScan.SelectedIndex];
        if(await flasher.connect(device))
        {
            PickerScan.IsVisible = false;
            ConnectDevice.IsVisible = false;
            DisconnectDevice.IsVisible = true;
            StartScanning.IsVisible = false;
            StopScanning.IsVisible = false;
        }
        else
        {
            PickerScan.IsVisible = true;
            ConnectDevice.IsVisible = true;
            DisconnectDevice.IsVisible = false;
        }

    }
    private void OnDisconnectClicked(object sender, EventArgs e)
    {
        flasher.disconnect();
        StartScanning.IsVisible = true;
        PickerScan.IsVisible = true;
        ConnectDevice.IsVisible = true;
        DisconnectDevice.IsVisible = false;
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

