using CorySignalGenerator.Models;
using CorySignalGenerator.Wave;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace CorySynthUI.Models
{
    public class DeviceModel : PropertyChangeModel
    {

        ISettingsViewModel settingsModel;
        private DeviceModel(ISettingsViewModel settings)
        {
            settingsModel = settings;

            AudioRenderer = new RenderAudioDevice(50);
            MidiCapture = new CaptureMidiDevice();
            MidiCapture.MessageReceived += MidiCapture_MessageReceived;
            AudioDeviceWrapper = new DeviceWatchWrapper(DeviceClass.AudioRender);
            AudioDeviceWrapper.SelectedDeviceChanged += AudioRenderDevices_SelectedDeviceChanged;
            
            MidiDeviceWrapper = new DeviceWatchWrapper(WindowsPreview.Devices.Midi.MidiInPort.GetDeviceSelector());
            MidiDeviceWrapper.SelectedDeviceChanged += MidiCaptureDevices_SelectedDeviceChanged;
        }

        
        public async static Task<DeviceModel> CreateDeviceModelAsync(ISettingsViewModel settings)
        {
            var model = new DeviceModel(settings);

            model.AudioDeviceWrapper.SelectedDevice = settings.AudioRenderDevice;
            model.MidiDeviceWrapper.SelectedDevice = settings.MidiCaptureDevice;
            //if (!String.IsNullOrEmpty(settings.AudioRenderDeviceId))
            //    await model.AudioDeviceWrapper.SetSelectedDeviceFromIdAsync(settings.AudioRenderDeviceId);

//            if (!String.IsNullOrEmpty(settings.MidiCaptureDeviceId))
//                await model.MidiDeviceWrapper.SetSelectedDeviceFromIdAsync(settings.MidiCaptureDeviceId);

            return model;
            
        }

        public event Windows.Foundation.TypedEventHandler<IMidiInputDevice, CorySignalGenerator.Sequencer.Midi.MidiInputMessageEventArgs> MidiMessageReceived;


        void MidiCapture_MessageReceived(IMidiInputDevice sender, CorySignalGenerator.Sequencer.Midi.MidiInputMessageEventArgs args)
        {
            if (MidiMessageReceived != null)
                MidiMessageReceived(sender, args);
        }

        public void SetSampleProvider(ISampleProvider provider)
        {
            AudioRenderer.ChangeSampleProvider(provider);
        }

        /// <summary>
        /// The selected audio device
        /// </summary>
        public Device SelectedAudioRenderDevice
        {
            get { return AudioDeviceWrapper.SelectedDevice; }
            set { AudioDeviceWrapper.SelectedDevice = value; }
        }
        public ObservableCollection<Device> AudioRenderDevices { get { return AudioDeviceWrapper.Devices; } }

        void AudioRenderDevices_SelectedDeviceChanged(object sender, DeviceChangedEventArgs<Device> e)
        {
            OnPropertyChanged("SelectedAudioRenderDevice");
            if(settingsModel.AudioRenderDevice != e.NewDevice)
                settingsModel.AudioRenderDevice = e.NewDevice;
            AudioRenderer.ChangeAudioDevice(e.NewDevice);

        }

        /// <summary>
        /// The selected midi input
        /// </summary>
        public Device SelectedMidiCaptureDevice
        {
            get { return MidiDeviceWrapper.SelectedDevice; }
            set { MidiDeviceWrapper.SelectedDevice = value; }
        }

        public ObservableCollection<Device> MidiCaptureDevices { get { return MidiDeviceWrapper.Devices; } }

        void MidiCaptureDevices_SelectedDeviceChanged(object sender, DeviceChangedEventArgs<Device> e)
        {
            OnPropertyChanged("SelectedMidiCaptureDevice");
            if(settingsModel.MidiCaptureDevice != e.NewDevice)
                settingsModel.MidiCaptureDevice = e.NewDevice;
            MidiCapture.ChangeDevice(e.NewDevice);
        }


        protected IAudioRenderer AudioRenderer { get; private set; }

        protected DeviceWatchWrapper AudioDeviceWrapper { get; private set; }

        protected IMidiInputDevice MidiCapture { get; private set; }

        protected DeviceWatchWrapper MidiDeviceWrapper { get; private set; }


    }
}
