namespace BleFlasher_UI;

using BleFlasher;

public partial class MainPage : ContentPage
{
    BleFlasher flasher;
    Stream filestream;

    string[] SUPPORTED_EXT_FILES = new[] { "hex", "bin", "raw" };

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

    private async void OnStartScanningClicked(object sender, EventArgs e)
    {


        flasher.DiscoveredDevice -= Flasher_DiscoveredDevice;
        flasher.ProgressionUpdated -= Flasher_ProgressionUpdated;


        StartScanning.IsVisible = false;
        StopScanning.IsVisible = true;
        ConnectDevice.IsVisible = false;

        PickerScan.Items.Clear();


        flasher.DiscoveredDevice += Flasher_DiscoveredDevice;
        flasher.ProgressionUpdated += Flasher_ProgressionUpdated;

        await flasher.startScanning();


    }

    private void Flasher_ProgressionUpdated(object sender, double e)
    {

        Dispatcher.Dispatch(() => TransfertProgress.Progress = e) ;
    }

    private void Flasher_DiscoveredDevice(object sender, InTheHand.Bluetooth.BluetoothDevice d)
    {

        Dispatcher.Dispatch(() => DiscoveredDevice(sender, d));

    }

    private void DiscoveredDevice(object sender, InTheHand.Bluetooth.BluetoothDevice d)
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

        StartScanning.IsVisible = true;
        StopScanning.IsVisible = false;
        await flasher.stopScanning();

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

    }


    private async void OnSelectFileClicked(object sender, EventArgs e)
    {

        PickOptions options = new PickOptions();
        options.PickerTitle = "Select file to write";
   
        options.FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
       { DevicePlatform.Android, SUPPORTED_EXT_FILES },
       { DevicePlatform.iOS, SUPPORTED_EXT_FILES },
       { DevicePlatform.WinUI,SUPPORTED_EXT_FILES },
    });

        var selected_file = await FilePicker.Default.PickAsync(options);
        if(selected_file == null)
        {
            FileName.Text = "";
            StartAddress.IsEnabled = true;
            if (StartAddress.Text == "AUTO")
            {
                StartAddress.Text = "";
            }
            return;

        }
        FileName.Text = selected_file.FullPath;
        if (FileName.Text.EndsWith(".hex", StringComparison.OrdinalIgnoreCase))
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
        try
        { 

        filestream = await selected_file.OpenReadAsync();

        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);

        }

    }


    private async void OnCreateFileClicked(object sender, EventArgs e)
    {
        PickOptions options = new PickOptions();
        options.PickerTitle = "Select file to create";

        options.FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
       { DevicePlatform.Android, SUPPORTED_EXT_FILES },
       { DevicePlatform.iOS, SUPPORTED_EXT_FILES },
       { DevicePlatform.WinUI,SUPPORTED_EXT_FILES },
    });


        var selected_file = await FilePicker.Default.PickAsync(options);

        if (selected_file.FileName.EndsWith(".hex", StringComparison.OrdinalIgnoreCase))
        {

            selected_file.FileName = "export.hex";
        }
        else
        {

            selected_file.FileName = "export.bin";
        }
        if (selected_file == null)
        {

        }
        FileNameR.Text = selected_file.FullPath;
    }
    private async void OnReadClicked(object sender, EventArgs e)
    {
        ActivityR.IsRunning  = true;

        if (StartAddressR.Text == null || SizeR == null)
        {
            return;
        }

        uint start_address = 0;
        if (StartAddressR.Text.Contains("x"))
        {
            start_address = Convert.ToUInt32(StartAddressR.Text, 16);
        }
        else
        {
            start_address = Convert.ToUInt32(StartAddressR.Text, 10);
        }

        uint size = 0;
        if (SizeR.Text.Contains("x"))
        {
            size = Convert.ToUInt32(SizeR.Text, 16);
        }
        else
        {
            size = Convert.ToUInt32(SizeR.Text, 10);
        }


        var stream = new System.IO.FileStream(FileNameR.Text, FileMode.OpenOrCreate);
        stream.Position = 0;

        if (FileNameR.Text.EndsWith(".hex", StringComparison.OrdinalIgnoreCase))
        {
            await flasher.ReadToHexFile(stream,start_address, size);
        }
        else
        {
            await flasher.ReadToBinaryFile(stream, start_address, size);
        }

        stream.Close();

        ActivityR.IsRunning = false;
    }
        
}

