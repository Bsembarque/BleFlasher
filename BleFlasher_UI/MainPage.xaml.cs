namespace BleFlasher_UI;
using BleFlasher;

public partial class MainPage : ContentPage
{
    BleFlasher flasher;
    Stream filestream;

    public MainPage()
	{
		InitializeComponent();
        flasher = new BleFlasher();
        StopScanning.IsVisible = false;
        ConnectDevice.IsVisible = false;
    }

	private void OnStartScanningClicked(object sender, EventArgs e)
	{
        this.IsBusy = true;
        flasher.startScanning();

        StartScanning.IsVisible = false;
        StopScanning.IsVisible = true;
        ConnectDevice.IsVisible = false;

        this.IsBusy = false;
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
        this.IsBusy = true;

        var device = flasher.GetDevices()[PickerScan.SelectedIndex];
        if(await flasher.connect(device))
        {
 
            ConnectedLayout.IsVisible = true;
        }
        else
        {
            ConnectedLayout.IsVisible = false;
        }
        this.IsBusy = false;
    }
    private void OnDisconnectClicked(object sender, EventArgs e)
    {
        flasher.disconnect();
        ConnectedLayout.IsVisible = false;
    }


    private async void OnWriteClicked(object sender, EventArgs e)
    {
        this.IsBusy = true;

        if (flasher.isBinaryFile(filestream))
        {
            await flasher.writeBinaryFile(uint.Parse(StartAddress.Text), filestream);
        }
        else
        {
            flasher.writeHexFile(filestream);
        }
        this.IsBusy = false;
    }


    private async void OnSelectFileClicked(object sender, EventArgs e)
    {
        this.IsBusy = true;

        var selected_file = await FilePicker.Default.PickAsync();
        FileName.Text = selected_file.FullPath;
        filestream = await selected_file.OpenReadAsync();
        if (!flasher.isBinaryFile(filestream))
        {
            StartAddress.IsEnabled = false;
            StartAddress.Text = "AUTO";
        }
        else
        {
            StartAddress.IsEnabled = true;
            if(StartAddress.Text == "AUTO")
            {
                StartAddress.Text = "";
            }
        }
        this.IsBusy = false;
    }

    

}

