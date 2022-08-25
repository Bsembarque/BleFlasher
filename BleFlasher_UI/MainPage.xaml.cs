namespace BleFlasher_UI;
using BleFlasher;

public partial class MainPage : ContentPage
{
    BleFlasher flasher;
    Stream filestream;

    public bool isScanning
    {
        get{ 
            if (flasher != null)
                return flasher.isScanning();
            return false;
         }
    }
    public MainPage()
	{
		InitializeComponent();
        flasher = new BleFlasher();
        StopScanning.IsVisible = false;
        ConnectDevice.IsVisible = false;
    }

    private void OnStartScanningClicked(object sender, EventArgs e)
    {


        flasher.DiscoveredDevice -= Flasher_DiscoveredDevice;
        flasher.ProgressionUpdated -= Flasher_ProgressionUpdated;


        StartScanning.IsVisible = false;
        StopScanning.IsVisible = true;
        ConnectDevice.IsVisible = false;

        PickerScan.Items.Clear();


        flasher.DiscoveredDevice += Flasher_DiscoveredDevice;
        flasher.ProgressionUpdated += Flasher_ProgressionUpdated;

        flasher.startScanning();


    }

    private void Flasher_ProgressionUpdated(object sender, double e)
    {

            TransfertProgress.Progress = e;
    }

    private void Flasher_DiscoveredDevice(object sender, InTheHand.Bluetooth.BluetoothDevice d)
    {
       
            ConnectDevice.IsVisible = true;
     
        string name = d.Name + "(" + d.Id + ")";
        PickerScan.Items.Add(name);
        PickerScan.SelectedIndex = 0;

    }

    private void OnStopScanningClicked(object sender, EventArgs e)
    {
        flasher.stopScanning();

        StartScanning.IsVisible = true;
        StopScanning.IsVisible = false;

    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        ActivityC.IsRunning = true;
        flasher.stopScanning();

        var device = flasher.GetDevices()[PickerScan.SelectedIndex];
        if(await flasher.connect(device))
        {
 
            ConnectedLayout.IsVisible = true;
        }
        else
        {
            ConnectedLayout.IsVisible = false;
        }
        ActivityC.IsRunning = false;
    }
    private void OnDisconnectClicked(object sender, EventArgs e)
    {
        flasher.disconnect();
        ConnectedLayout.IsVisible = false;
        ActivityC.IsRunning = false;
    }


    private async void OnWriteClicked(object sender, EventArgs e)
    {
        ActivityR.IsRunning = true;

        Dispatcher.Dispatch(async () =>
        {

            if (flasher.isBinaryFile(filestream))
            {
                if (StartAddress.Text != null)
                {
                    uint start_address = 0;
                    if (StartAddress.Text.Contains("x"))
                    {
                        start_address = Convert.ToUInt32(StartAddress.Text, 16);
                    }
                    else
                    {
                        start_address = Convert.ToUInt32(StartAddress.Text, 10);
                    }
                    await flasher.writeBinaryFile(start_address, filestream);
                }
            }
            else
            {
               await flasher.writeHexFile(filestream);
            }
            ActivityR.IsRunning = false;
        });
    }


    private async void OnSelectFileClicked(object sender, EventArgs e)
    {


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

    }

    

}

